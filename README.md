# Original project https://github.com/continue88/VRCapture - all the credit and thanks to the original author! 🙏

### Introduction
Capture VR screenshots as well as frames for VR video in Unity. (360 captures from 2 eyes)

![Setting example](/StereoCaptureScript.png)

### Instructions：
1. Add the `StereoCapture` prefab as a child to the camera that is acting as the left eye
2. Make sure the output path exists, or set it to whatever you would like
3. If you want screenshots to start right away, check `Capture Start`
4. Start the game in the editor - when `Capture Start` is checked, screenshots will be written to the output path (this will be slow since they will be 2048x2048 each)
5. To make a video, use `ffmpeg` to combine the screenshots:
  `ffmpeg.exe -framerate 60 -i Captures/frame_%%5d.jpg -c:v libx264 -profile:v high -level 4.2 -r 60 -pix_fmt yuv420p -crf 18 -preset slower MyMovie_360_TB.mp4` (windows example, I will add a OSX one as well)
6. The video should be playable on VR devices, still to test YouTube

![Right click on menu](/CaptureMenu.png)

### Editor single screenshot
1. You can click-click on the `Stereo Capture (Script)` header and there will be a `Capture` option
2. A single screenshot will be written to the output folder

### Notes：
1. This process is quite slow especially if you are capturing at 60fps
2. Post-processing can be controlled via the EnableEffects property: Example: `ColorCorrectionCurves;ToneMapping;Bloom`
3. This script only writes out frames - `ffmpeg` is required to join them for a video. Audio is currently not recorded.

![Example screenshot](/frame_00001.jpg)
