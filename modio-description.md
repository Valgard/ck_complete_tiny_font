# Complete Tiny Font

**Every accented and Cyrillic character, finally drawn for the font that was
missing them.**

Core Keeper's small font (`thinTiny`) ships a reduced character set: digits,
ASCII and a handful of symbols. Any accented character — `ä`, `é`, `ñ`, `ł`,
Cyrillic — is missing, and the game silently substitutes a glyph from its
Chinese font, which renders at a different metric and looks deformed.

This mod replaces that font with a complete, hand-drawn 331-character build:
full Western European, partial Eastern European, Cyrillic and typographic
punctuation, all drawn to match the original's proportions.

## What it does

- Replaces `thinTiny`'s 114-character set with a hand-drawn 331-character
  build, complete with generated kerning for every glyph.
- Fixes every accented or Cyrillic character the small font previously
  couldn't render at all — those used to silently borrow a glyph from the
  Chinese font, at the wrong metric and visibly deformed.
- Keeps the original's spacing: existing glyphs render one pixel flatter,
  nothing shifts sideways.

## What changes visibly

The game uses this font for the small numbers on slots — item counts in your
inventory and in recipe lists, stack sizes on items lying on the ground — and
for score text. Everything else this mod touches is text that previously
could not render at all.

## Good to know

- No dependencies and nothing to configure — subscribe and it works.
- Safe to add and remove at any time: this only replaces the character data
  one font loads, so removing the mod puts `thinTiny` straight back to
  vanilla's reduced set.

## For mod authors

If your UI uses the small font, declare a dependency on this mod and drop
your own glyph workarounds.

---

*Built with the official Pugstorm Core Keeper Mod SDK. Personal-use,
non-commercial (Core Keeper EULA). Not affiliated with or endorsed by
Pugstorm.*
