# PiPlayer

### 介绍
一款跨平台，可以远程控制宿主机播放视频、图片、音频的Web应用。可用于电子相册、播放器的控制后台。宿主机需要是桌面操作系统，例如：Windows、Ubuntu Desktop、Raspberry Pi OS with desktop等带有桌面的操作系统。

### Demo
视频列表:  
![](https://raw.githubusercontent.com/withsalt/PiPlayer/main/docs/images/video.png)

文件上传:  
![](https://raw.githubusercontent.com/withsalt/PiPlayer/main/docs/images/upload.png)

### 教程
#### 安装mpv、ffmpeg
windows x64平台中内置了mpv和ffmpeg，其他平台需要安装对应的mpv和ffmpeg。以raspberrypi为例：  
```shell
sudo apt install mpv ffmpeg
```

#### 设置自启动
在 /home/用户名/.config 目录下 创建文件夹`autostart`，然后把桌面应用程序放进去，就相当于你每次开机之后自动执行了桌面应用程序。  
以树莓派用户`pi`为例，应用程序目录为：`/home/pi/apps/piplayer`  
```shell
mkdir ~/.config/autostart
nano ~/.config/autostart/piplayer.desktop
```

新增以下内容：
```shell
[Desktop Entry]
Name = PiPlayer
Type = Application
Comment = RaspberryPi Player WebSite
Exec = /home/pi/apps/piplayer/PiPlayer --urls "http://*:6888"
Terminal = false
MultipulArgs = false
Categories = Application;
StartupNotify = ture
```
注意：
1. 替换PiPlayer可执行文件所在路径！！
2. 不能使用systemd来托管服务，使用systemd无法调用用户态的mpv播放器。

### 使用
在浏览器中打开http://<宿主机IP>:6888体验。  
