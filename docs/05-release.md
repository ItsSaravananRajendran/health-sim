# 05 Release

## WebGL build
1. `unity build . --target WebGL` (check `unity build --help` for output path flags).
2. Test locally: `cd <build dir> && python3 -m http.server 8000`, open http://localhost:8000.
3. If the browser reports a compression error, set Player Settings, Publishing Settings,
   Compression Format to Disabled or Gzip and rebuild.

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
