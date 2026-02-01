# Changes Since Last Commit

## Weather Art
- Refined ASCII art with more vertical detail for rain/snow/thunder.
- Updated thunderstorm lightning to stacked `/ \ /` bolts and tightened spacing.
- Adjusted thunderstorm cloud alignment across sizes; centered lightning under the cloud.
- Replaced sun art with a new centered glyph and shifted the horizontal beam for alignment.
- Thunderstorm clouds now render darker, with lightning in yellow.

## CLI
- Added a `--today` flag to show only today, split into morning/afternoon/evening when hourly data is available.

## Warnings
- Added per-day warning lines in the forecast output.
- Added optional MeteoBlue warnings integration (requires `METEOBLUE_API_KEY`).

## Color Rendering
- Added per-character art colorization for clouds, rain, snow, and lightning.
- Tweaked ANSI gray variants to allow darker storm clouds.
