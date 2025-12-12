using ChatServer.NET.IO;
using ChatServer.Models;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        public User UserModel { get; set; }

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            try
            {
                ClientSocket = client;
                UID = Guid.NewGuid();
                _packetReader = new PacketReader(ClientSocket.GetStream());

                // Читаем код операции
                var opcode = _packetReader.ReadByte();
                if (opcode != 0)
                {
                    Console.WriteLine($"Ошибка: неверный код операции {opcode}");
                    client.Close();
                    return;
                }

                // Читаем имя пользователя
                Username = _packetReader.ReadMessage();
                if (string.IsNullOrEmpty(Username))
                {
                    Console.WriteLine("Ошибка: не удалось прочитать имя пользователя");
                    client.Close();
                    return;
                }

                // Создаем модель пользователя
                UserModel = new User
                {
                    Username = this.Username,
                    UID = this.UID
                };

                Console.WriteLine($"{DateTime.Now}: Клиент подключился с именем '{Username}'");
                Task.Run(() => Process());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания клиента: {ex.Message}");
                client.Close();
            }
        }

        void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Получено сообщение! {msg}");
                            Program.BroadcastMessage($"[{DateTime.Now}] : [{Username}]: {msg}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID.ToString()}]: отключился");
                    Program.BroadcastDisconnect(UID.ToString());
                    try
                    {
                        ClientSocket?.Close();
                    }
                    catch { }
                    break;
                }
            }
        }
    }
}