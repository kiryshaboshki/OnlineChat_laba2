using ChatServer.NET.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        

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
                if (opcode != 0) // ожидаем 0 для подключения
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
                    ClientSocket.Close();
                }
            }
        }
    }
}
