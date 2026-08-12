# Complete Tiny Font

A small Core Keeper mod that **completes the game's small pixel font**.

Core Keeper's small font (`thinTiny`) ships a reduced character set: digits,
ASCII and a handful of symbols. Any accented character — `ä`, `é`, `ñ`, `ł`,
Cyrillic — is missing, and the game silently substitutes a glyph from its
Chinese font, which renders at a different metric and looks deformed.

This mod replaces that font with a complete, hand-drawn 331-character build:
full Western European, partial Eastern European, Cyrillic and typographic
punctuation, all drawn to match the original's proportions.

Personal-use, non-commercial (Pugstorm EULA).

## Install

- **mod.io:** subscribe to the mod; Core Keeper downloads it on next launch.
- **Local build:** see `CLAUDE.md` → *Build and deploy*.

No dependencies, no configuration — subscribe and it works.

## What changes visibly

The game uses this font for the small numbers on slots — item counts in your
inventory and in recipe lists, stack sizes on items lying on the ground — and
for score text. Those become one pixel flatter and keep the original's exact
widths, so nothing numeric moves. Everything else the mod affects is text that
previously could not render at all.

Letters are a different matter: 25 of the characters the original had are a
pixel or two wider here, most visibly `m` and `M`. Vanilla barely uses this
font for letters, so you are unlikely to see it — but a mod with letter labels
in the small font may find them reflowing.

## Safe to add and remove

This mod only replaces the character data one font uses at load time. Removing
it puts `thinTiny` straight back to vanilla's reduced set — nothing it touches
depends on this mod having ever been installed, and no other part of the game
is affected.

## For mod authors

If your UI uses the small font, declare a dependency on this mod and drop your
own glyph workarounds.
