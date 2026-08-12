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

No automated tests. The cheapest check is the mod's own log line: after a
build reloads, `Player.log` should show `[Complete Tiny Font] thinTiny
replaced: 331 codepoints from a 257x144 atlas; kerning 337 rows`
(`TryApply()` in `ThinTinyFontPatch.cs`). A missing or different-looking line
means the running session is on a stale build, not a new bug — diagnosing
that once cost a full fix round when a screenshot showed "no change" for
exactly this reason. The codepoint count also drops if the `Widths` constant
is pasted at the wrong length, so the same line doubles as a free detector
for that mistake. Then verify visually: look for the small numbers on
inventory/recipe slots and dropped-item stacks, and confirm an accented or
Cyrillic character (previously borrowed from the Chinese font and visibly
deformed) now renders as this mod's own glyph, at the same spacing as
before.

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

337 of the 384 cells are painted; 6 of those hold controller-button glyphs that
never reach the screen, leaving 331 characters that actually render — the number
quoted everywhere else in this mod's docs.

The reason those six drop out is the charset, not the width table: they sit at
`latinCharset[2..7]`, which is `U+0020` six times over, and `InitCodePoints`
skips `' '`. CK reaches its own controller glyphs by glyph *index* instead, which
a codepoint-based replacement cannot address. Their `Widths` digits are `'7'`,
not `'0'` — a painted cell can never carry `'0'`, because the digit comes from
the magenta rect box and `validate()` rejects any glyph pixels without one.
Anyone "tidying" those six digits to `'0'` would change the generated `Widths`,
break byte-identical regeneration and drop six kerning rows.

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
numbers were affected, and the claim still survives — deliberately frozen, with
its correction alongside — in `item-checklist/docs/iteration-history.md`, which
records what was believed at the time. Anyone extending this mod should find the
correct scope here, next to the code, rather than rediscovering it.

### Why `charDims` stays `(8, 10)` while the atlas cell is 12 px tall

`charDims` is a layout metric only — line advance, reported text
dimensions — not atlas geometry. Raising it to the atlas's 12 px cell height
would grow every line gap in existing mod UIs by 2 px.

The rect passed to `PugFont.InitCodePoints()` is deliberately **two** rows
taller than the drawn glyph (`RectH = BoxH + 2 = 12`, not `BoxH = 10`), because
CK's own `InitCodePoints()` derives every sprite from `rect2 = (rect.y + 1,
rect.height - 1)` — it discards the rect's bottom row unconditionally. A rect
that exactly matched the drawn glyph would therefore drop its own last row
(this is what made every glyph render 1 px low before the atlas's vertical
shift). One of the two padding rows is discarded and one survives, so the
*sprite* comes out one row taller than the glyph: cell rows 0..10 — the ten
drawn rows plus one blank — with the pivot landing at 5/11. Do not read the
"one row" of the sprite back into `RectH`; `RectH = BoxH + 1` reproduces the
1-px-low bug. That puts the glyph's bottom edge 2 px below the pivot, which is
exactly vanilla's own value: `floor(9/2) - 2 = 2` for vanilla's 9-row rect,
`floor(11/2) - 3 = 2` for this build's 11-row one.

### Kerning

Rebuilding `glyphData` drops vanilla's own `kerning` byte arrays entirely, so
text would render wider than vanilla with no correction at all.
`ApplyKerning()` loads a generated `Cells × Cells` byte matrix
(`thinTiny_kerning.bytes`, 384 × 384 = 147,456 bytes) derived from this
atlas's own ink columns.

**The rule** (`kerning_pair()` in `utils/pixaki_to_glyphs.py`): for every
atlas row where both glyphs have ink, the gap is how far the second glyph's
ink would sit from the end of the first glyph's advance at zero kerning. The
kerning value is **one less than the smallest such gap across those shared
ink rows, clamped to `[0, KERNING_CLAMP]`** (`KERNING_CLAMP = 2`) — never the
raw gap itself. That "one less" exists so two glyph stems can never touch: an
earlier revision used the raw gap directly, which reproduced **97.66%** of
vanilla's own kerning table but let adjacent narrow stems collide outright
(`l` immediately followed by `t`, both advance 2 — the stems touched in
"Seltenheit" and "Entdeckt"). The margin-adjusted rule matches only **~84%**
of vanilla's table, and that is now deliberately not the criterion: vanilla's
numbers describe vanilla's own (wider) glyph shapes — digits render 6 px
tall there vs. 5 here, and `C E F L` are 2 px wide there vs. 3 — so agreement
with a different set of shapes was never proof of correctness for this one.
Collision-freedom on these glyphs is the bar, and leaving one column of air
is what clears it. An in-game experiment raised `KERNING_CLAMP` to 3 to see a
wider look; it changed only low-ink punctuation/symbol pairs (1,071 of them),
never a single letter or digit, so the difference was invisible in ordinary
text and the clamp was settled back at 2 (fix: `4d0361c` parent /
`130d543` mod repo; rejected experiment: `8792598` parent / `7acf3e0` mod
repo).

One side effect the margin buys for free: because it is uniform across every
glyph pair, digit-to-digit kerning no longer varies by which two digits are
adjacent, so a counter or coordinate display no longer subtly changes total
width as its digits change — the raw-gap variant did not guarantee that.

It also deliberately does **not** overwrite any pair with vanilla's real
value. Kerning describes the side bearings of specific glyph shapes, and this
build's glyphs differ from vanilla's own in exactly the ways that matter for
that number, as above. Importing vanilla's numbers would be systematically
wrong for a different set of shapes, not "more faithful" — the percentages
above validate the generation *rule*, not a reason to fall back to vanilla's
data. (An earlier revision did restore vanilla's real pairs on top of the
generated matrix; it was removed once this was understood — see git history
around `005617e`.)

**Five variants were measured and rejected**, so nobody has to re-derive them. Read the
percentages carefully: they are agreement with vanilla's own table over the 13,456 pairs
where both fonts have a comparable value, and **all of them except the last were measured
before the margin existed** — they are variations of the raw-gap rule, not of the shipped
one. Agreement calibrated the *rule*; it was never the acceptance criterion (see above):

| Variant | Agreement | Why rejected |
|---|---|---|
| raw gap, clamp 2 — the original rule | 97.66 % | narrow stems touched in game — `lt`, `td` |
| raw gap, clamp 3 or 4 | 97.2–97.5 % | worse agreement, and still no margin to prevent the collisions |
| raw gap, neighbouring ink rows tolerated | 89.5 % | looser and less predictable, with no collision it prevented |
| raw gap, no-overlap pairs default to 0 instead of the clamp | 97.42 % | glyphs sharing no ink row get no air at all |
| **margin**, clamp 3 | not measured | changes only 1,071 low-ink punctuation pairs against the shipped rule, zero letters or digits — invisible in ordinary text, judged in game |

The shipped rule (margin, clamp 2) agrees with vanilla on ~84 %. Do not read the ~97 %
rows as an argument for raising the clamp: they belong to a rule that has no margin at all,
and the one clamp-3 variant that does carry the margin was rejected by looking at the game.

**This rule is described three times** — this section, the class doc comment
on `ThinTinyFontPatch` itself, and `kerning_pair()`'s own docstring in
`utils/pixaki_to_glyphs.py` — and it has already changed three times while a
release landed in the middle of that history. All three need updating
together whenever the rule changes again; a stale copy in the C# comment is
exactly how the wrong accuracy figure ended up shipped once.

### Regenerating the atlas and kerning matrix

The Pixaki master (`sources/thinTiny.pixaki`) and its 13-revision review
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
`OK — 337 painted cells, all invariants hold`. Those invariants are the rect
box's geometry, the one-to-one match between boxes and painted cells, and —
since the ink-containment check was added — that no glyph paints outside its
own advance width or below its box, which the kerning pass would otherwise
clip silently and then measure the truncated glyph. The `Widths` string in
`ThinTinyFontPatch.cs` is generated output too — regenerate and re-paste it
from the same master rather than hand-editing a digit; see that class's own
doc comment for what each digit encodes.

`sources/glyph-templates/` holds the reference material for glyph work: two
tracked tools (`dump_log_to_json.py`, `build_glyph_grids.py`) and, beside
them, CK's own extracted font atlases plus `glyph_metrics.json` and the
`grids/` debug overlays. Those data files are Pugstorm assets and stay
**gitignored**, so a fresh clone gets the tools and an otherwise empty
directory — that is expected, not a broken checkout. `glyph_metrics.json`
can only be re-dumped from a running game (see that directory's `README.md`);
none of it is needed to regenerate the shipped atlas, which reads the Pixaki
master alone. The whole directory came over from `item-checklist` with the
master, where the glyph set originated.

## macOS / CrossOver

Deployed through the fake-mod.io workaround (see parent `../CLAUDE.md`). This
mod's fake mod.io ID is **`9999987`**. Do not open the in-game Mods menu
while a fake-ID install is active; re-run `../utils/build.sh` to restore if
the cache is wiped.

## Publishing to mod.io

Published — modId `6302514`, currently at version `1.0.1`, in
`unity/CompleteTinyFont/Editor/CompleteTinyFont_modio.asset` (`modId` is
stable across releases; only the modfile behind it changes per version). The
publish used the shared `CoreKeeperModUtils.CLIPublishHelper.Publish` Editor
class the same way as every sibling mod (`../utils/upload.sh`): the version
comes from the topmost `## [x.y.z]` entry of `CHANGELOG.md`, so each release
needs that entry updated before publishing, not just the code. `CK_MODIO_TYPE`
is `Visual|Language|Library`. `requiredOn` is `1` (Client) — this mod only
changes client-side text rendering, so a server lacking it must never block
a join. The profile logo at `unity/CompleteTinyFont/Editor/logo.png` is a
1024×1024 transparent PNG made with the family logo pipeline (parent
`../CLAUDE.md` § Logo / branding) — a hand-painted type-case tray with
glowing gold characters, replacing the scaffold's original 64×64 grey
placeholder. Worth remembering for any future mod: `CLIPublishHelper` only
rejects a *missing* logo asset, never a placeholder one, so a real logo has
to be swapped in deliberately before publishing — it is never caught for
you.

Both releases are tagged (annotated `1.0.0` and `1.0.1`). `origin`
(`git@github.com:Valgard/ck_complete_tiny_font.git`) carries `main` and both
tags. `backup` is configured to point at the Forgejo host, but the repository
does not exist there yet — it couldn't be created from the network reachable
at publish time — so `backup` is currently a remote with nowhere to push
to; create it there before relying on that remote.

## Conventions

- Commit messages: Conventional Commits (`type(scope): subject`), imperative,
  no emoji.
- Documentation files (`CLAUDE.md`, `README.md`, `docs/`) are English; chat
  answers are German.
- Prefer `git commit --amend` / `git reset --soft` over fix-up commits on a
  personal branch, and `git rebase` over `git merge`.
