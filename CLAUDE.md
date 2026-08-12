# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with
code in this repository.

## What this repo is

A Core Keeper mod that replaces the game's small pixel font (`thinTiny`) with
a complete, hand-drawn 331-character build — full Western European accents,
partial Eastern European, Cyrillic and typographic punctuation, plus
generated kerning for every glyph. One Harmony patch against Pugstorm's
`CoreKeeperModSDK`. No content of its own; no dependencies. Personal-use,
non-commercial (Pugstorm EULA).

The parent `../CLAUDE.md` holds the mod-agnostic SDK/CrossOver guidance
shared with the sibling mods.

## Build and deploy

```bash
source .envrc           # or, from a worktree: source ../../../.envrc && source .envrc
../utils/build.sh       # Unity batchmode build; on Darwin auto-runs install-macos.sh
```

Unity Editor must be closed (it locks the project). `utils/link.sh` symlinks
the repo's `unity/` mirror into `$SDK_PATH/Assets/`; `build.sh` invokes it
idempotently on every run, so worktree switches and repo moves self-heal.

**Concurrent-build / shared-SDK caveat:** all sibling mods share one
`CoreKeeperModSDK` clone with a single `UnityLockfile`. If another session is
building, wait for the lock to release — do not kill it.

No automated tests — verification is a manual in-game check: look for the
small numbers on inventory/recipe slots and dropped-item stacks, and confirm
an accented or Cyrillic character (previously borrowed from the Chinese font
and visibly deformed) now renders as this mod's own glyph, at the same
spacing as before.

## Architecture

- **`CompleteTinyFontMod` (`IMod`)** — bootstrap. Holds the mod's
  `AssetBundle` handle (resolved in `EarlyInit`), and calls
  `ThinTinyFontPatch.TryApply()` from `Init()` as a late-arrival fallback —
  see below for why that call is needed at all.
- **`ThinTinyFontPatch`** — `[HarmonyPatch(typeof(TextManager), "Init2")]`
  postfix; the whole feature lives in its idempotent `TryApply()`.

### The `TextManager.Init2` anchor

`TextManager.Init2` is where the game constructs `Manager.text.thinTiny` (and
its sibling faces) in the first place, so a postfix on it is the earliest
point at which `Manager.text.thinTiny` exists to be mutated. `TryApply()` is
idempotent and bails silently if the font or the mod's `AssetBundle` isn't
ready yet, which is also why `CompleteTinyFontMod.Init()` calls it again as a
late-arrival path: if `Init2` already ran before this mod's own `Init()`, the
postfix never fired, and the second call is what actually applies the swap.

### Charset / glyph-index / atlas-cell identity

`thinTiny` normally carries its own `_customCharset` — 114 codepoints
starting at ASCII 33. The patch clears it (`f._customCharset = null`), which
falls back to the shared static `PugFont.latinCharset`: exactly 384
characters. The atlas is laid out to match — `Cols = 32`, cell size 8×12,
`Cells = 384` (32 × 12 rows) — in the same order `latinCharset` enumerates
its characters, so charset position, glyph index and atlas cell are one and
the same coordinate: cell `i` is glyph `i` is `latinCharset[i]`. No separate
glyph-index table is needed anywhere in the C#.

337 of the 384 cells are painted; 6 of those hold controller-button glyphs
that are intentionally left unmapped (their `Widths` digit is `'0'`, the same
marker used for a genuinely empty cell), leaving 331 characters that actually
render — the number quoted everywhere else in this mod's docs.

### Where `thinTiny` actually renders

Exactly **14** shipped assets use this font: the seven inventory/progress
slot prefabs, `RecipeSlot`, `RecipeCategorySlot`, `BossStatueRecipeSlot`,
`DroppedItem` (the stack-size label on an item lying on the ground),
`ConditionUI`, the score-text prefab, and the main-manager prefab.

**Damage numbers are not among them.** `CombatText.prefab` uses `thinSmall`,
not `thinTiny`, and CK's own `isDamageNumber → SetDefaultFont(thinTiny)`
branch is inert: rendering reads `style.fontFace`, that setter only ever
writes `defaultStyle.fontFace`, and the one copy between the two runs the
other way — `defaultStyle = style.GetCopy()`, in `Awake()`. The branch sets a
field nothing downstream ever reads.

Worth pinning down explicitly: this is the single most error-prone fact about
this mod. An earlier draft of this project's own documentation claimed damage
numbers were affected, and a sibling mod's `CLAUDE.md` still carries that
claim in one of its historical entries. Anyone extending this mod should find
the correct scope here, next to the code, rather than rediscovering it.

### Why `charDims` stays `(8, 10)` while the atlas cell is 12 px tall

`charDims` is a layout metric only — line advance, reported text
dimensions — not atlas geometry. Raising it to the atlas's 12 px cell height
would grow every line gap in existing mod UIs by 2 px.

The rect passed to `PugFont.InitCodePoints()` is deliberately one row taller
than the drawn glyph (`RectH = BoxH + 2 = 12`, not `BoxH = 10`), because CK's
own `InitCodePoints()` derives every sprite from `rect2 = (rect.y + 1,
rect.height - 1)` — it discards the rect's bottom row unconditionally. A rect
that exactly matched the drawn glyph would therefore drop its own last row
(this is what made every glyph render 1 px low before the atlas's vertical
shift). Padding the rect by that one blank row makes the sprite cover cell
rows 0..10 — the ten drawn rows plus one blank — with the pivot landing at
5/11. That puts the glyph's bottom edge 2 px below the pivot, which is
exactly vanilla's own value: `floor(9/2) - 2 = 2` for vanilla's 9-row rect,
`floor(11/2) - 3 = 2` for this build's 11-row one.

### Kerning

Rebuilding `glyphData` drops vanilla's own `kerning` byte arrays entirely, so
text would render wider than vanilla with no correction at all.
`ApplyKerning()` loads a generated `Cells × Cells` byte matrix
(`thinTiny_kerning.bytes`, 384 × 384 = 147,456 bytes) derived from this
atlas's own ink columns — the smallest gap between two glyphs' painted rows,
clamped to a sane range — and calibrated against vanilla's real kerning table
to **97.66%** agreement.

It deliberately does **not** then overwrite any pair with vanilla's real
value. Kerning describes the side bearings of specific glyph shapes, and this
build's glyphs differ from vanilla's own in exactly the ways that matter for
that number: digits are 5 px tall here vs. 6 in vanilla, and `C E F L` are
3 px wide here vs. 2. Importing vanilla's numbers would be systematically
wrong for a different set of shapes, not "more faithful" — the 97.66% figure
validates the generation *rule*, not a reason to fall back to vanilla's data.
(An earlier revision did restore vanilla's real pairs on top of the generated
matrix; it was removed once this was understood — see git history around
`005617e`.)

### Regenerating the atlas and kerning matrix

The Pixaki master (`sources/thinTiny.pixaki`) and its 12-revision review
document (`sources/thinTiny-review.md`) live in this repo — relocated from
`item-checklist`, where the glyph set originated before this mod existed.
Regenerate this mod's shipped atlas and kerning matrix from that master with:

```bash
python3 utils/pixaki_to_glyphs.py \
  --pixaki complete-tiny-font/sources/thinTiny.pixaki \
  --sheet complete-tiny-font/unity/CompleteTinyFont/Art/thinTiny_full.png \
  --kerning complete-tiny-font/unity/CompleteTinyFont/Art/thinTiny_kerning.bytes
```

Run from the parent `core_keeper/` directory — the tool is shared across
mods, not vendored here. `--check-only` (in place of `--sheet`/`--kerning`)
validates the master without writing anything; the current master reports
`OK — 337 painted cells, all invariants hold`. The `Widths` string in
`ThinTinyFontPatch.cs` is generated output too — regenerate and re-paste it
from the same master rather than hand-editing a digit; see that class's own
doc comment for what each digit encodes.

## macOS / CrossOver

Deployed through the fake-mod.io workaround (see parent `../CLAUDE.md`). This
mod's fake mod.io ID is **`9999987`**. Do not open the in-game Mods menu
while a fake-ID install is active; re-run `../utils/build.sh` to restore if
the cache is wiped.

## Publishing to mod.io

Not yet published — the real mod ID in
`unity/CompleteTinyFont/Editor/CompleteTinyFont_modio.asset` is still `0`.
When publishing, `../utils/upload.sh` uses the shared
`CoreKeeperModUtils.CLIPublishHelper.Publish` Editor class the same way as
every sibling mod: the version comes from the topmost `## [x.y.z]` entry of
`CHANGELOG.md`. `CK_MODIO_TYPE` is `Visual|Language|Library`. `requiredOn` is
`1` (Client) — this mod only changes client-side text rendering, so a server
lacking it must never block a join. The profile logo at
`unity/CompleteTinyFont/Editor/logo.png` is a 1024×1024 transparent PNG made
with the family logo pipeline (parent `../CLAUDE.md` § Logo / branding) — a
hand-painted type-case tray with glowing gold characters, replacing the
scaffold's original 64×64 grey placeholder. Worth remembering for any future
mod: `CLIPublishHelper` only rejects a *missing* logo asset, never a
placeholder one, so a real logo has to be swapped in deliberately before
publishing — it is never caught for you.

## Conventions

- Commit messages: Conventional Commits (`type(scope): subject`), imperative,
  no emoji.
- Documentation files (`CLAUDE.md`, `README.md`, `docs/`) are English; chat
  answers are German.
- Prefer `git commit --amend` / `git reset --soft` over fix-up commits on a
  personal branch, and `git rebase` over `git merge`.
