using ChatClient.Net.IO;
using System;
using System.IO;
using System.Net.Sockets;

namespace ChatClient.Net
{
    class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            try
            {
                if (_client == null)
                {
                    _client = new TcpClient();
                }

                if (!_client.Connected)
                {
                    Console.WriteLine($"Подключение к серверу как '{username}'...");
                    _client.Connect("127.0.0.1", 7891);

                    if (_client.Connected)
                    {
                        Console.WriteLine("Подключение установлено");
                        PacketReader = new PacketReader(_client.GetStream());

                        // Отправляем пакет подключения
                        using (var connectPacket = new PacketBuilder())
                        {
                            connectPacket.WriteOpCode(0); // код подключения
                            connectPacket.WriteMessage(username);

                            byte[] packetBytes = connectPacket.GetPacketBytes();
                            NetworkStream stream = _client.GetStream();
                            stream.Write(packetBytes, 0, packetBytes.Length);
                            stream.Flush();

                            Console.WriteLine("Пакет подключения отправлен");
                        }

                        // Запускаем чтение пакетов в фоне
                        ReadPackets();
                    }
                    else
                    {
                        Console.WriteLine("Не удалось подключиться");
                    }
                }
                else
                {
                    Console.WriteLine("Уже подключен к серверу");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
        }

        public void ReadPackets()
        {
            Task.Run(() =>
            {
                try
                {
                    while (_client != null && _client.Connected)
                    {
                        var opcode = PacketReader.ReadByte();
                        Console.WriteLine($"Получен opcode: {opcode}");

                        switch (opcode)
                        {
                            case 1:
                                Console.WriteLine("Событие подключения");
                                connectedEvent?.Invoke();
                                break;
                            case 5:
                                Console.WriteLine("Событие сообщения");
                                msgReceivedEvent?.Invoke();
                                break;
                            case 10:
                                Console.WriteLine("Событие отключения");
                                userDisconnectEvent?.Invoke();
                                break;
                            default:
                                Console.WriteLine($"Неизвестный opcode: {opcode}");
                                break;
                        }
                    }
                }
                catch (IOException ioex)
                {
                    Console.WriteLine($"Ошибка чтения пакетов: {ioex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Общая ошибка: {ex.Message}");
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    Console.WriteLine("Не подключен к серверу");
                    return;
                }

                using (var packetBuilder = new PacketBuilder())
                {
                    packetBuilder.WriteOpCode(5);
                    packetBuilder.WriteMessage(message);

                    byte[] packetBytes = packetBuilder.GetPacketBytes();
                    Console.WriteLine($"Отправка сообщения '{message}', размер пакета: {packetBytes.Length} байт");

                    
                    NetworkStream stream = _client.GetStream();
                    stream.Write(packetBytes, 0, packetBytes.Length);
                    stream.Flush();

                    Console.WriteLine("Сообщение отправлено успешно");
                }
            }
            catch (SocketException sex)
            {
                Console.WriteLine($"Ошибка сокета: {sex.Message}");
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"Ошибка ввода-вывода: {ioex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}