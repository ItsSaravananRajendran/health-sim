# 05 Release

## WebGL build
1. For local testing, set Player Settings, Publishing Settings, Compression Format to
   **Disabled**. `python3 -m http.server` does not send `Content-Encoding` headers, so Gzip
   or Brotli builds fail to load there.
2. `unity build . --target WebGL` (check `unity build --help` for output path flags).
3. Test locally: `cd <build dir> && python3 -m http.server 8000`, open http://localhost:8000.
4. For hosting, either keep Compression Disabled, or use Gzip with **Decompression Fallback**
   turned on. GitHub Pages and itch.io do not let you set the headers a plain Gzip build
   needs.

## Hosting (free)
- itch.io: zip the build folder and upload as an HTML5 game, or
- GitHub Pages: push the build output to a `gh-pages` branch.
Publishing is public, so do it deliberately and only after the tests pass.

## Portfolio assets
- 60 to 90 second screen recording
- Live link and repo link
- README with learning goal, audience, how content is edited, how to test

## Resume and cover letter wording (use only after it is built)
Project line, result first, no invented numbers:
"Built Plate Check, a browser-based Unity (WebGL) nutrition counseling simulation where the
learner inspects a plate, fixes it and chooses counseling responses, with educator-editable
content stored as ScriptableObjects and automated scoring tests."
Then update the cover letter sentence about not having shipped Unity.
