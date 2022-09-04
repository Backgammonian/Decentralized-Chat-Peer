# Decentralized-Chat-Peer
Peer (client) of decentralized chat application.
## Overview
This project is a client of decentralized chat application. This app works on top of [reliable UDP library](https://github.com/RevenantX/LiteNetLib) and establishes secure connections using encryption algorithms AES-256 and ECDH (WARNING: encryption protocol hasn't been thoroughly tested for vulerabilities such as MITM, replay attack etc.) Main reason of using UDP instead of TCP is to be able to perform [UDP hole punching technique](https://bford.info/pub/net/p2pnat) so users that are inside different local networks can establish direct connect via Internet with help of [rendevous server](https://github.com/Backgammonian/Decentralized-Chat-Tracker-Console). After connection is done peers are able to send text messages and images to each other. Also this app can be hidden in system tray.
## Libraries used in this project:
* [LiteNetLib](https://github.com/RevenantX/LiteNetLib)
* [Newtonsoft.Json](https://www.newtonsoft.com/json)
* [Crc32.NET](https://github.com/force-net/Crc32.NET)
* [Meziantou.Framework.WPF - Thread-safe observable collection](https://github.com/meziantou/Meziantou.Framework)
* [SystemTrayApp.WPF](https://github.com/fujieda/SystemTrayApp.WPF/)
* [Microsoft.Tookit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit)
* [Microsoft.Xaml.Behaviours.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf)
* [NGif](https://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET)
* [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common/)
## Demonstration:
![demo](demo.jpeg)
![animated-demo](animated-demo.gif)
