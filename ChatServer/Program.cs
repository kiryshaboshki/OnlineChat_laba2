using ChatServer.NET.IO;
using ChatServer.Models;
using ChatServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        static List<Client> _authenticatedClients;
        static List<Client> _pendingClients;
        static TcpListener _listener;
        static JsonDatabaseService _dbService;

        static void Main(string[] args)
        {
            InitializeServer();

            while (true)
            {
                try
                {
                    var tcpClient = _listener.AcceptTcpClient();
                    var client = new Client(tcpClient, _dbService);
                    _pendingClients.Add(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now}: Ошибка: {ex.Message}");
                }
            }
        }

        static void InitializeServer()
        {
            _authenticatedClients = new List<Client>();
            _pendingClients = new List<Client>();
            _dbService = new JsonDatabaseService();

            _listener = new TcpListener(IPAddress.Any, 7891);
            _listener.Start();

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║     ЧАТ-СЕРВЕР С АВТОРИЗАЦИЕЙ        ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine($"║ Запуск: {DateTime.Now}");
            Console.WriteLine($"║ Порт: 7891");
            Console.WriteLine("║ Ожидание подключений...");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }

        public static void AddAuthenticatedClient(Client client)
        {
            _authenticatedClients.Add(client);
            _pendingClients.Remove(client);

            Console.WriteLine($"✅ Аутентифицирован: {client.Username}");
            Console.WriteLine($"   Всего онлайн: {_authenticatedClients.Count}");
            Console.WriteLine();

            BroadcastConnection();
            BroadcastMessage($"[СИСТЕМА] {client.Username} присоединился к чату!");
        }

        static void BroadcastConnection()
        {
            try
            {
                foreach (var client in _authenticatedClients)
                {
                    foreach (var userClient in _authenticatedClients)
                    {
                        using (var packet = new PacketBuilder())
                        {
                            packet.WriteOpCode(1); // Обновление списка пользователей
                            packet.WriteMessage(userClient.Username);
                            packet.WriteMessage(userClient.UID.ToString());

                            var bytes = packet.GetPacketBytes();
                            client.ClientSocket.Client.Send(bytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка рассылки: {ex.Message}");
            }
        }

        public static void BroadcastMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            Console.WriteLine($"💬 Рассылаю: '{message}'");
            Console.WriteLine($"Клиентов онлайн: {_authenticatedClients.Count}");

            using (var packet = new PacketBuilder())
            {
                packet.WriteOpCode(5); // Сообщение
                packet.WriteMessage(message);

                var bytes = packet.GetPacketBytes();

                for (int i = 0; i < _authenticatedClients.Count; i++)
                {
                    var client = _authenticatedClients[i];
                    try
                    {
                        if (client.ClientSocket.Connected)
                        {
                            Console.WriteLine($"  → Отправляю клиенту {i}: {client.Username}");
                            client.ClientSocket.Client.Send(bytes);
                        }
                        else
                        {
                            Console.WriteLine($"  ✗ Клиент {client.Username} не подключен");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Ошибка отправки {client.Username}: {ex.Message}");
                    }
                }
            }
        }

        public static void BroadcastDisconnect(string uid)
        {
            try
            {
                var client = _authenticatedClients.FirstOrDefault(c => c.UID.ToString() == uid);
                if (client != null)
                {
                    Console.WriteLine($"🔌 Отключился: {client.Username}");

                    // Отправляем уведомление об отключении
                    using (var packet = new PacketBuilder())
                    {
                        packet.WriteOpCode(10); // Отключение
                        packet.WriteMessage(uid);

                        var bytes = packet.GetPacketBytes();

                        foreach (var otherClient in _authenticatedClients.Where(c => c.UID.ToString() != uid))
                        {
                            try
                            {
                                if (otherClient.ClientSocket.Connected)
                                    otherClient.ClientSocket.Client.Send(bytes);
                            }
                            catch { }
                        }
                    }

                    // Сообщаем в чат
                    BroadcastMessage($"[СИСТЕМА] {client.Username} покинул чат!");

                    // Удаляем из списка
                    _authenticatedClients.RemoveAll(c => c.UID.ToString() == uid);

                    Console.WriteLine($"   Осталось онлайн: {_authenticatedClients.Count}");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка отключения: {ex.Message}");
            }
        }

        static void RemoveClient(Guid uid)
        {
            BroadcastDisconnect(uid.ToString());
        }
    }
}