# VRCapture

### 简介
    用来做unity3d下面的vr视频抓取，也就是360度上下3D视频抓取。
### 使用方法：
    1，把StereoCapture脚本拖放到抓取的物体上（相当于眼睛的位置）
    2，将材质球Bilt拖放到脚本的Bilt Material属性上
    3，设置抓取文件的路径，默认为[D:\Captures]目录不存在的话去创建一个
    3，将Capture Start勾上
    4，启动编辑器
    ...默认会抓取每秒60帧的2048x2048的360度上下图片到目标目录去。
    5，利用ffmpeg合成视频
      ffmpeg.exe -framerate 60 -i Captures/frame_%%5d.jpg -c:v libx264 -profile:v high -level 4.2 -r 60 -pix_fmt yuv420p -crf 18 -preset slower MyMovie_360_TB.mp4
    6，生成的视频拷贝到VR设备上，可以到VR设备上播放了（如三星gearvr，occlus，htc-vive什么的）
### 编辑器测试
    1，拖放StereoCapture脚本上去之后，选择object右键菜单选择[Capture]
    2，在目标文件夹下面会生成一个图片
### 注意：
    1，这个生成视频会灰常慢，我测试场景来算一般1秒2帧，一秒60帧视频，要半天的样子。
    2，这个脚本只是用来生成视频的图片，需要ffmpeg来把图片合并成视频
### 主要参考文献(urneal4)：
    https://www.unrealengine.com/zh-CN/blog/capturing-stereoscopic-360-screenshots-videos-movies-unreal-engine-4
