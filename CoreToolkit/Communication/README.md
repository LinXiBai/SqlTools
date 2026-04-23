# Communication 通信模块

提供串口（Serial Port）与 TCP 客户端/服务器的通信抽象与实现，支持 Modbus 协议和帧解析。

## 目录结构

```
Communication/
├── Core/
│   ├── ISerialPort.cs           # 串口接口抽象
│   └── ITcpClient.cs            # TCP 客户端接口抽象
├── Helpers/
│   ├── AdvancedSerialPort.cs    # 高级串口实现（事件驱动、缓冲区管理）
│   ├── AdvancedTcpClient.cs     # 高级 TCP 客户端（异步、心跳、重连）
│   ├── ModbusHelper.cs          # Modbus RTU/TCP 协议辅助
│   ├── SerialFrameParser.cs     # 串口数据帧解析器
│   ├── SerialPortHelper.cs      # 串口基础封装
│   ├── SerialPortManager.cs     # 串口多实例管理
│   ├── SerialPortScanner.cs     # 串口自动扫描与探测
│   └── TcpClientHelper.cs       # TCP 基础封装
└── Models/
    ├── CommunicationResult.cs   # 通信结果（成功/数据/异常）
    ├── ProtocolFrame.cs         # 协议帧模型
    ├── SerialPortEventArgs.cs   # 串口接收事件参数
    └── TcpEventArgs.cs          # TCP 接收事件参数
```

## 核心类说明

| 类/接口 | 说明 |
|---------|------|
| `ISerialPort` | 串口接口：`Open/Close/Write/Read/ReadLine/ClearBuffers` |
| `ITcpClient` | TCP 客户端接口：`Connect/Disconnect/Send/Receive` |
| `SerialPortManager` | 管理多个命名串口实例的生命周期，支持配置持久化 |
| `SerialPortScanner` | 扫描可用串口并返回 `SerialPortInfo` 列表 |
| `ModbusHelper` | Modbus CRC 校验、数据包组装/解析 |
| `SerialFrameParser` | 基于头尾标识或固定长度的数据帧解析，解决粘包问题 |

## 使用示例

```csharp
// 串口通信
using var port = new SerialPortHelper("COM3", 9600);
port.Open();
port.WriteLine("STATUS?");
string response = port.ReadLine(timeoutMs: 1000);

// TCP 通信
var tcp = new TcpClientHelper("192.168.1.10", 502);
tcp.Connect();
var data = tcp.SendReceive(new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A }, timeoutMs: 2000);

// 帧解析
var parser = new SerialFrameParser(header: new byte[] { 0xAA, 0x55 }, lengthFieldOffset: 2);
parser.FrameReceived += (s, e) => Console.WriteLine($"收到帧: {BitConverter.ToString(e.Frame)}");
```

## 依赖

- System.IO.Ports
- System.Net.Sockets
