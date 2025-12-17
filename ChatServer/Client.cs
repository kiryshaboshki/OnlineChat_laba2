using ChatServer.NET.IO;
using ChatServer.Models;
using ChatServer.Services;
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
        public bool IsAuthenticated { get; set; }

        PacketReader _packetReader;
        private JsonDatabaseService _dbService;

        public Client(TcpClient client, JsonDatabaseService dbService)
        {
            try
            {
                ClientSocket = client;
                _dbService = dbService;
                _packetReader = new PacketReader(ClientSocket.GetStream());

                Console.WriteLine($"{DateTime.Now}: Новое соединение от {client.Client.RemoteEndPoint}");

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
            try
            {
                while (ClientSocket.Connected)
                {
                    var opcode = _packetReader.ReadByte();

                    switch (opcode)
                    {
                        case 0: // Регистрация
                            HandleRegistration();
                            break;
                        case 1: // Авторизация
                            HandleLogin();
                            break;
                        case 5: // Сообщение
                            if (IsAuthenticated)
                            {
                                var msg = _packetReader.ReadMessage();
                                Console.WriteLine($"[{DateTime.Now}]: {Username}: {msg}");

                                // Отправляем в простом формате
                                Program.BroadcastMessage($"<{Username}> {msg}");
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"[{UID}]: отключился");
                if (IsAuthenticated)
                {
                    Program.BroadcastDisconnect(UID.ToString());
                }
                try
                {
                    ClientSocket?.Close();
                }
                catch { }
            }
        }

        void HandleRegistration()
        {
            var username = _packetReader.ReadMessage();
            var password = _packetReader.ReadMessage();

            Console.WriteLine($"{DateTime.Now}: Попытка регистрации: {username}");

            var success = _dbService.RegisterUser(username, password);

            using (var packet = new PacketBuilder())
            {
                packet.WriteOpCode(2); // Ответ на регистрацию
                packet.WriteMessage(success ? "SUCCESS" : "FAIL");

                var bytes = packet.GetPacketBytes();
                ClientSocket.Client.Send(bytes);
            }

            if (success)
            {
                Console.WriteLine($"{DateTime.Now}: Успешная регистрация: {username}");
            }
        }

        void HandleLogin()
        {
            var username = _packetReader.ReadMessage();
            var password = _packetReader.ReadMessage();

            Console.WriteLine($"{DateTime.Now}: Попытка входа: {username}");

            var user = _dbService.AuthenticateUser(username, password);

            if (user != null)
            {
                Username = user.Username;
                UID = user.UID;
                UserModel = user;
                IsAuthenticated = true;

                Console.WriteLine($"{DateTime.Now}: Успешный вход: {Username}");

                // Добавляем в список онлайн пользователей
                Program.AddAuthenticatedClient(this);

                // Отправляем успешный ответ
                using (var packet = new PacketBuilder())
                {
                    packet.WriteOpCode(3); // Успешный вход
                    packet.WriteMessage(Username);
                    packet.WriteMessage(UID.ToString());

                    var bytes = packet.GetPacketBytes();
                    ClientSocket.Client.Send(bytes);
                }
            }
            else
            {
                Console.WriteLine($"{DateTime.Now}: Неудачная попытка входа: {username}");

                using (var packet = new PacketBuilder())
                {
                    packet.WriteOpCode(4); // Неудачный вход
                    packet.WriteMessage("Неверные учетные данные");

                    var bytes = packet.GetPacketBytes();
                    ClientSocket.Client.Send(bytes);
                }
            }
        }
    }
}