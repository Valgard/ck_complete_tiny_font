# CK Font Glyph Templates & Tooling (Iter-25)

Reference material + reproducibility tooling for the Iter-25 thinTiny accented-glyph
fix. The **committed source of truth** is elsewhere — `sources/thinTiny.pixaki`
(the hand-drawn master) + `unity/CompleteTinyFont/Art/thinTiny_full.png` (the generated
sheet) + the `Widths` string in `unity/CompleteTinyFont/ThinTinyFontPatch.cs`. This
directory only re-derives them.

**Tracked vs. gitignored:** the `*.py` scripts are tracked (our tools). The extracted
CK atlases (`rrs*_raw.png`, `*_view.png`, …), `glyph_metrics.json`, and `grids/` are
**gitignored** — they are Pugstorm game assets / derived data, reference only.

## Atlas → FontFace map (CK 1.2.1.4)

| Face | Atlas (`texName`) | Size | Glyphs | has `ö`/`Ä`/`ß`? |
|---|---|---|:---:|:---:|
| **`thinTiny`** | `rrs5` | 256×40 | 114 | no (basic Latin only, no accents) |
| `thinSmall` | `rrsthin8` | 257×144 | 331 | yes |
| `thinMedium` | `rrs10thin` | 513×192 | 331 | yes |
| `boldSmall` | `rrs8` | 257×144 | 331 | yes |
| `boldMedium` | `rrs10` | 513×192 | 331 | yes |
| `boldLarge` | `rrs12b` | 514×192 | 212 | yes |
| `boldHuge` | `rrs18` | 641×432 | 341 | yes |
| `buttonFont` | `buttonfont_new` | 339×161 | 90 | — (controller glyphs) |

## Reproduction pipeline

```
(diagnostic build dumps font tables)  →  Player.log
   dump_log_to_json.py   Player.log         →  glyph_metrics.json   (Pugstorm data, gitignored)
   build_glyph_grids.py  glyph_metrics.json →  grids/  (debug overlays: bg + charDims + rects + atlas)

utils/pixaki_to_glyphs.py  thinTiny.pixaki  →  Art/thinTiny_full.png + kerning matrix + Widths string (stdout)
```

The shared tool no longer reads `glyph_metrics.json` — its grid geometry (32×12
cells of 8×12 px) is hardcoded, derived once from that dump and now stable.
`glyph_metrics.json` remains an input only to `build_glyph_grids.py`'s debug
overlays. Regenerate the atlas from the parent `core_keeper/` directory:

```bash
python3 utils/pixaki_to_glyphs.py \
  --pixaki complete-tiny-font/sources/thinTiny.pixaki \
  --sheet complete-tiny-font/unity/CompleteTinyFont/Art/thinTiny_full.png \
  --kerning complete-tiny-font/unity/CompleteTinyFont/Art/thinTiny_kerning.bytes
```

**`glyph_metrics.json` requires a re-dump.** It holds CK's per-face glyph rects +
codePoints, which can only be read from a runtime diagnostic dump (CK's `PugFont`
MonoBehaviours have no TypeTree, so UnityPy can't read them statically). The Iter-25
diagnostic block was removed when the fix shipped — see `dump_log_to_json.py`'s header
for how to re-add it if the glyph set ever needs regenerating.

## Pixaki master layers (`thinTiny.pixaki`)

**Atlas** = the glyph sprites, **Rects** = the per-glyph advance box (height 10 at cell
row 0, glyph-specific width), **Dims** = the nominal-cell checkerboard, **Background** =
cyan, plus a `Layer 1` helper strip. The thinSmall reference layer the master started with
was removed in revision 2 — comparisons run against `rrsthin8_raw.png` instead. Extraction
maps each codepoint to its thinSmall cell (col = x//8, row from top). The file format is
documented in `item-checklist/docs/research/pixaki-format.md` (it predates this repo and
still serves that mod's own Pixaki sprite pipeline).
