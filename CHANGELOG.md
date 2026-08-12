# Changelog

All notable changes to this mod are documented here. The publish pipeline
reads the topmost `## [x.y.z]` entry as the version to publish.

## [1.0.1] - 2026-08-12

### Fixed

- Some letter pairs rendered with no gap between them at all, most visibly a
  lowercase `l` immediately followed by `t` (e.g. "Seltenheit", "Entdeckt"),
  where the two stems touched directly. Spacing between letters now always
  leaves at least a sliver of air, even for the tightest pairs.

## [1.0.0] - 2026-08-11

### Added

- Replaces Core Keeper's reduced `thinTiny` font with a complete 331-character
  build: full Western European accents, partial Eastern European, Cyrillic and
  typographic punctuation. Text that previously fell back to the Chinese font
  (and rendered deformed) now renders natively.
