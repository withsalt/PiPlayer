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
```shell
cd ~
nano .bash_profile
```

新增以下内容：
```shell
if ! pgrep "PiPlayer" >/dev/null 2>&1 ; then
    <替换为PiPlayer可执行文件所在路径>/PiPlayer --urls "http://*:6888" &
fi
```
注意：替换PiPlayer可执行文件所在路径！！  

其实就是把启动命令放在用户登录执行的脚本中。每次用户自动登录后，判断服务是否运行，如果没有运行，则启动服务，并指定端口为6888。

注意：不能使用systemd来托管服务，使用systemd无法调用用户态的mpv播放器。

### 使用
在浏览器中打开http://<宿主机IP>:6888体验。  
