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

### Raspberry Pi OS (Bookworm) 显示配置修复指南  

该方案主要用于解决以下问题：  
1. 树莓派连接显示器后识别出奇怪的分辨率（如 85*40120）。  
2. 修改驱动后导致桌面环境无法启动（黑屏或只有光标）。  

---

#### 1. 修改显卡驱动模式  

由于 Bookworm 默认的 KMS 驱动对部分非标准显示器或转接头兼容性较差，需要切换为兼容性更好的 FKMS (Firmware KMS) 模式。  

在终端中执行以下命令编辑配置文件：  

```bash
sudo nano /boot/firmware/config.txt
```

找到行 `dtoverlay=vc4-kms-v3d`，将其修改为：  

```ini
dtoverlay=vc4-fkms-v3d
```

#### 2. 强制指定 HDMI 分辨率  

在同一个配置文件 (`/boot/firmware/config.txt`) 的末尾，添加以下内容。这将强制树莓派忽略错误的显示器反馈，直接输出标准信号。  

*请根据你的实际显示器修改 `hdmi_mode` 的值（常见值：82=1080P 60Hz, 35=1280x1024, 4=720P）。*

```ini
# --- 强制显示设置开始 ---

# 强制 HDMI 接口保持开启（即使未检测到显示器热插拔）
hdmi_force_hotplug=1

# 关键设置：忽略显示器发送的错误 EDID 信息
# 这能直接解决识别出 85*40120 等奇怪分辨率的问题
hdmi_ignore_edid=0xa5000080

# 设置 HDMI 信号组为 2 (DMT - 代表标准电脑显示器模式)
hdmi_group=2

# 设置分辨率模式为 82 (即 1920x1080 60Hz)
# 如果你的显示器不是 1080P，请查询 CEA/DMT 表修改此值
hdmi_mode=82

# --- 强制显示设置结束 ---
```

修改完成后，按 `Ctrl+O` 保存，`Enter` 确认，然后按 `Ctrl+X` 退出编辑器。

#### 3. 切换为 X11 显示服务  

Bookworm 默认使用 Wayland 桌面协议，该协议与 `vc4-fkms-v3d` 驱动配合时可能会导致桌面无法启动。必须手动切换回老牌的 X11 服务。  

1. 在终端输入以下命令进入配置工具：  
   ```bash
   sudo raspi-config
   ```
2. 使用方向键选择 **6 Advanced Options** (高级选项)，按回车。  
3. 选择 **A6 Wayland**，按回车。  
4. 选择 **W1 X11** (Openbox window manager with X11 backend)，按回车确认。  
5. 系统提示设置已更新，选择 **Finish** 退出。  
6. 系统会询问是否重启，选择 **Yes** 进行重启。  

---

**重启后验证：**
重启完成后，树莓派应能以指定的 1080P 分辨率正常进入图形化桌面环境。

