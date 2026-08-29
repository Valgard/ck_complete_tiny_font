# Complete Tiny Font — Roadmap

Points that are **deliberately cut to stand alone**, not a shopping list for a
release. The useful question is "which point next?", never "what goes into
version X" — a version collects whatever happened to be finished by then.

Each entry records what is already settled and what still has to be decided, so
picking one up does not mean re-deriving the groundwork.

## Screenshots — the mod has none

Nothing in `sources/` shows this mod running: the folder holds the logo
candidates and the `thinTiny` master, and `CK_DISCORD_MEDIA` in `.envrc` is
empty. The Discord thread, the mod.io gallery and the Workshop item therefore
all show the logo and nothing else — and only the logo is uploaded by the
publish pipeline, so a gallery picture is placed by hand.

**Settled: the obvious subject is the wrong one.** The font's most frequent
appearance is the small numbers on inventory and recipe slots, and those are
exactly where this mod changes least — one pixel flatter at identical widths, by
design, so nothing numeric moves. A picture of them shows nothing. What the mod
does becomes visible on an accented or Cyrillic character, which vanilla cannot
render and silently borrows from the Chinese font at a different metric. That
makes the subject a before/after pair rather than a single frame, and it means
the game's language has to be switched to reach those characters at all.

**Settled: the "before" costs a restart, and a stale build looks like success.**
The vanilla half has to be captured with the mod not loaded. Both halves need
the log line — `[Complete Tiny Font] thinTiny replaced: 331 codepoints from a
257x144 atlas; kerning 337 rows` — read *before* the shutter, in the direction
that frame expects it: this repo has already lost a full fix round to a
screenshot that showed "no change" because the session was running an old build.

**Settled: scaling is not a detail here.** The glyphs are a handful of pixels
tall, so a 1:1 crop barely carries the difference, and any enlargement has to be
nearest-neighbour — a smooth resample destroys precisely the pixel grid the mod
is about.

**To decide.** Whether before and after ship as two images or one composed
frame. Which UI surface and which language: German alone covers the accents,
Russian is needed for Cyrillic, and it is open whether one screen can honestly
carry both or whether that is two pairs. What the crop and the scale factor are.
And whether `CK_DISCORD_MEDIA` gets filled afterwards — which would mean adding
the images to the existing thread as a comment, since the thread is already
posted.
