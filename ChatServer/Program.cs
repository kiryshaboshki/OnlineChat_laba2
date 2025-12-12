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
        static List<Client> _clients;
        static List<User> _users;
        static TcpListener _listener;
        static JsonDatabaseService _dbService;

        static void Main(string[] args)
        {
            Console.Title = "Чат-Сервер";

            _clients = new List<Client>();
            _users = new List<User>();
            _dbService = new JsonDatabaseService();

            _listener = new TcpListener(IPAddress.Any, 7891);
            _listener.Start();

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║          ЧАТ-СЕРВЕР ЗАПУЩЕН          ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine($"║ Время запуска: {DateTime.Now.ToString("HH:mm:ss")}");
            Console.WriteLine($"║ Порт: 7891");
            Console.WriteLine($"║ IP-адрес: {GetLocalIPAddress()}");
            Console.WriteLine("║ Статус: Ожидание подключений...");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    var client = new Client(_listener.AcceptTcpClient());

                    // Проверяем, что клиент успешно создан
                    if (client.ClientSocket == null || string.IsNullOrEmpty(client.Username))
                        continue;

                    // Проверяем, нет ли уже такого пользователя
                    if (_clients.Any(c => c.Username == client.Username))
                    {
                        Console.WriteLine($"⚠ Пользователь '{client.Username}' уже подключен!");
                        client.ClientSocket.Close();
                        continue;
                    }

                    _clients.Add(client);

                    // Создаем пользователя
                    var user = new User
                    {
                        Username = client.Username,
                        UID = client.UID
                    };
                    _users.Add(user);

                    Console.WriteLine($" Подключен: {user.Username}");
                    Console.WriteLine($"   UID: {user.UID}");
                    Console.WriteLine($"   Всего онлайн: {_clients.Count}");
                    Console.WriteLine();

                    // Сохраняем в базу
                    _dbService.SaveUser(user);

                    // Рассылаем информацию о подключении
                    BroadcastConnection();

                    // Отправляем приветствие в чат
                    BroadcastMessage($"[СИСТЕМА] {user.Username} присоединился к чату!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Ошибка: {ex.Message}");
                }
            }
        }

        static void BroadcastConnection()
        {
            try
            {
                foreach (var client in _clients)
                {
                    foreach (var user in _users)
                    {
                        using (var packet = new PacketBuilder())
                        {
                            packet.WriteOpCode(1);
                            packet.WriteMessage(user.Username);
                            packet.WriteMessage(user.UID.ToString());

                            var bytes = packet.GetPacketBytes();
                            client.ClientSocket.Client.Send(bytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка рассылки подключений: {ex.Message}");
            }
        }

        public static void BroadcastMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            Console.WriteLine($"💬 {message}");

            using (var packet = new PacketBuilder())
            {
                packet.WriteOpCode(5);
                packet.WriteMessage(message);

                var bytes = packet.GetPacketBytes();
                var disconnected = new List<Client>();

                foreach (var client in _clients)
                {
                    try
                    {
                        if (client.ClientSocket.Connected)
                            client.ClientSocket.Client.Send(bytes);
                        else
                            disconnected.Add(client);
                    }
                    catch
                    {
                        disconnected.Add(client);
                    }
                }

                // Удаляем отключившихся
                foreach (var client in disconnected)
                {
                    RemoveClient(client.UID);
                }
            }
        }

        public static void BroadcastDisconnect(string uid)
        {
            try
            {
                var user = _users.FirstOrDefault(u => u.UID.ToString() == uid);
                if (user != null)
                {
                    Console.WriteLine($" Отключился: {user.Username}");

                    // Отправляем пакет отключения
                    using (var packet = new PacketBuilder())
                    {
                        packet.WriteOpCode(10);
                        packet.WriteMessage(uid);

                        var bytes = packet.GetPacketBytes();

                        foreach (var client in _clients.Where(c => c.UID.ToString() != uid))
                        {
                            try
                            {
                                if (client.ClientSocket.Connected)
                                    client.ClientSocket.Client.Send(bytes);
                            }
                            catch { }
                        }
                    }

                    // Сообщаем в чат
                    BroadcastMessage($"[СИСТЕМА] {user.Username} покинул чат!");

                    // Удаляем из списков
                    _users.RemoveAll(u => u.UID.ToString() == uid);
                    _clients.RemoveAll(c => c.UID.ToString() == uid);

                    // Удаляем из базы
                    _dbService.RemoveUser(Guid.Parse(uid));

                    Console.WriteLine($"   Всего онлайн: {_clients.Count}");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка отключения: {ex.Message}");
            }
        }

        static void RemoveClient(Guid uid)
        {
            BroadcastDisconnect(uid.ToString());
        }

        static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
    }
}