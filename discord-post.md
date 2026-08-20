# Complete Tiny Font

Core Keeper's small font ships 114 characters — digits, ASCII, a few symbols.
Every accented letter is missing, so `ä`, `é`, `ñ`, `ł` and anything Cyrillic
silently borrow a glyph from the game's Chinese font, at a different metric.
That is why they look deformed rather than absent.

This replaces the font with a hand-drawn 331-character build: full Western
European, partial Eastern European, Cyrillic and typographic punctuation, all
drawn to the original's proportions, with kerning generated for every glyph.

Digits keep their exact vanilla widths, so item counts and stack labels do not
shift — they render one pixel flatter. Letters were redrawn too, and 25 of them
gained a pixel or two of width, `m` and `M` most visibly.

No dependencies, nothing to configure. Safe to add or remove at any time: it
only swaps the character data one font loads, so removing it puts the reduced
vanilla set straight back.

## For mod authors

If your UI uses the small font, depend on this and delete your glyph
workarounds.
