# TCP-Chat-Client-Server
C# .NET TCP局域网聊天程序 C/S架构

使用演示：

启动服务器和客户端程序，服务器设置侦听端口号为5656，启动侦听。

输入命令netstat -ano |findstr 5656，查看端口状态，可以看到5656端口已处于LISTENING状态：

![image](https://user-images.githubusercontent.com/59370234/166692125-8a98a575-7f59-4ba0-878a-60bc8727d06d.png)

 
客户端输入服务器的IP 192.168.56.1，端口号5656，设置客户端的发送端口为19101，然后点击连接按钮：
连接成功后，会收到服务器的问好消息：

![image](https://user-images.githubusercontent.com/59370234/166692168-b3f54137-90cb-4990-b6bc-fd3c04b3881c.png)

 
服务器发来的消息带有server前缀，其他用户发来的前缀是user。
此时服务器也可以看到客户端的上线提示，并在右侧列表中显示：
 
 ![image](https://user-images.githubusercontent.com/59370234/166692200-92151319-9979-41e9-9f3e-0ad39e4bf0d0.png)

 
再次查看端口状态：
 
 ![image](https://user-images.githubusercontent.com/59370234/166692235-3e973ce1-b71c-42a0-83cf-5297d4e7ce14.png)

 
可以看到5656端口已经和19101端口建立了TCP连接。
启动Wireshark抓包，可以看到服务器和客户端之间TCP连接保活的Keep-Alive报文：

![image](https://user-images.githubusercontent.com/59370234/166692266-81c8d603-000c-4946-a3c7-6048b7e73f9b.png)

 
客户端和服务器互发消息，双方的界面均有显示，并可通过wireshark捕捉到：
客户端页面：

![image](https://user-images.githubusercontent.com/59370234/166692308-fc7efd4b-98db-4400-b118-44651a34a8c4.png)

 
服务器页面：

![image](https://user-images.githubusercontent.com/59370234/166692362-5979194d-2a15-4b76-8d15-cb9e55325e04.png)

 
Wireshark抓包结果：

![image](https://user-images.githubusercontent.com/59370234/166692397-77379a8a-0ded-4368-aebb-5eb582c654f8.png)

 
可以看到通信内容，包括头部“DIRECT”和消息内容“hello, I’m client”。
服务器向客户端发送消息，会新建一个TCP连接，发送完即释放，通过抓包可以看到连接建立SYN和释放FIN的过程：

![image](https://user-images.githubusercontent.com/59370234/166692427-c07cbeba-02c5-4da1-baa1-19a9b9bda95d.png)

 
