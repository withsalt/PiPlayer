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
windows x64平台中内置了mpv和ffmpeg，其他平台需要安装对应的mpv和ffmpeg。  
以RaspberryPi为例：  
```shell
sudo apt install mpv ffmpeg
```

#### 安装应用程序
本项目比较小众，且一般情况下，需要二次开发。所以没有提供发行的二进制版本，需要自行编译打包。  

```
mkdir -p ~/apps && cd ~/apps
unzip piplayer.zip && chmod -R 755 piplayer && chmod +x piplayer/PiPlayer
```

#### 设置自启动
使用 systemd 服务设置自启动，systemd 是一个系统和服务管理器，可以用来管理开机启动的服务。虽然通常用于后台服务，但也可以配置启动桌面应用程序。  
1. 创建 systemd 服务文件  
   创建一个新的服务定义文件，例如 piplayer.service，并将其保存在 /etc/systemd/system/ 目录下：  
   ```
   sudo nano /etc/systemd/system/piplayer.service
   ```
2. 编辑服务文件  
   在文件中添加以下内容:  
   ```
   [Unit]
   Description=PiPlayer Application
   After=graphical.target
   Wants=network-online.target
   
   [Service]
   User=pi
   WorkingDirectory=/home/pi/apps/piplayer
   ExecStart=sudo /home/pi/apps/piplayer/PiPlayer --urls "http://*:80"
   Restart=on-failure
   Environment=DISPLAY=:0
   Environment=XAUTHORITY=/home/pi/.Xauthority
   
   [Install]
   WantedBy=graphical.target
   ```
3. 启用并启动服务  
   ```
   sudo systemctl daemon-reload
   sudo systemctl start piplayer.service
   #设置开机自启
   sudo systemctl enable piplayer.service
   ```


### 使用
在浏览器中打开http://<宿主机IP>体验。  
