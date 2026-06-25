# Regenerating screenshots & demo GIF

The screenshots in `screenshots/` are rendered headlessly from the real UI - no display required.
`CaptureRunner` builds the exact same uGUI hierarchy the WebGL runtime builds, renders each screen
through a camera into a `RenderTexture`, and reads pixels back to PNG.

## How it works

- `Assets/Scripts/App/GameBootstrap.cs` -> `Construct()` creates the camera + canvas + `AppController`.
  This is shared by the runtime (`Start()`) and the capture harness, so captures match the shipped build.
- `Assets/Scripts/Editor/CaptureRunner.cs` (`Run`):
  1. opens an empty scene and calls `GameBootstrap.Construct`,
  2. points the camera at a 1600x900 `RenderTexture`,
  3. walks each screen (and a few action states: equip a skin, spend gems, unlock premium),
  4. pre-warms dynamic font glyphs, force-rebuilds layout, renders twice (font-texture rebuild
     lands on the second pass), then `ReadPixels` -> `EncodeToPNG`.
- Named screenshots (`01-play.png` ... `06-battlepass.png`) are a subset of the frame sequence; the
  full frame sequence is assembled into `demo.gif` with ffmpeg.

## Commands

```sh
UNITY="/Applications/Unity/Hub/Editor/6000.4.0f1/Unity.app/Contents/MacOS/Unity"
PROJ="$(pwd)"

# Render screenshots + frames (graphics on; do NOT pass -nographics)
"$UNITY" -batchmode -projectPath "$PROJ" \
  -executeMethod Breachpoint.EditorTools.CaptureRunner.Run \
  -captureOut "$PROJ/screenshots" -logFile -

# Build the demo GIF from the captured frame sequence
cd screenshots
ffmpeg -y -framerate 0.8 -i frames/frame_%02d.png \
  -vf "scale=1000:-1:flags=lanczos,palettegen=stats_mode=diff" /tmp/pal.png
ffmpeg -y -framerate 0.8 -i frames/frame_%02d.png -i /tmp/pal.png \
  -lavfi "scale=1000:-1:flags=lanczos[x];[x][1:v]paletteuse=dither=bayer:bayer_scale=3" \
  -loop 0 demo.gif
```

`CaptureRunner` writes both the per-frame sequence (under `screenshots/frames/`, used only for the GIF)
and the named PNGs. The WebGL build itself is produced by
`Breachpoint.EditorTools.BuildScript.BuildWebGL` (see README).
