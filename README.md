# PiPlayer

### 介绍
一款跨平台，可以远程控制宿主机播放视频、图片、音频的Web应用。宿主机需要是桌面操作系统，例如：Windows、Ubuntu Desktop、Raspberry Pi OS with desktop等带有桌面的操作系统。  

### Demo


### 服务配置
#### 安装mpv、ffmpeg
windows x64平台中内置了mpv和ffmpeg，其他平台需要安装对应的mpv和ffmpeg。以raspberrypi为例：  
```shell
sudo apt install mpv ffmpeg
```

#### PiPlayer服务
```shell
sudo nano /etc/systemd/system/piplayer.service
```

输入以下内容：
```shell
[Unit]
Description=PiPlayer Web Service
After=network.target

[Service]
WorkingDirectory=/opt/piplayer
ExecStart=/opt/piplayer/PiPlayer --urls "http://*:6888"
Restart=always
SyslogIdentifier=PiPlayer
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```
授权
```shell
sudo chmod 775 /etc/systemd/system/piplayer.service
```

运行
```shell
sudo systemctl start piplayer.service
sudo systemctl status piplayer.service
sudo systemctl stop piplayer.service
sudo systemctl enable piplayer.service
```

### 使用
在浏览器中打开http://<宿主机IP>:6888体验。  
