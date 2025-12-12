using ChatServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ChatServer.Services
{
    public class JsonDatabaseService
    {
        private readonly string _filePath = "users.json";
        private List<User> _users;

        public JsonDatabaseService()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                    Console.WriteLine($"Загружено {_users.Count} пользователей из базы данных.");
                }
                else
                {
                    _users = new List<User>();
                    Console.WriteLine("Файл базы данных не найден, создан новый список.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке пользователей: {ex.Message}");
                _users = new List<User>();
            }
        }

        public void SaveUser(User user)
        {
            try
            {
                // Удаляем старую запись если есть
                _users.RemoveAll(u => u.UID == user.UID);

                // Добавляем новую
                _users.Add(user);

                // Сохраняем в файл
                SaveToFile();

                Console.WriteLine($"Пользователь '{user.Username}' сохранен в базу данных.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении пользователя: {ex.Message}");
            }
        }

        public void RemoveUser(Guid uid)
        {
            try
            {
                var user = _users.FirstOrDefault(u => u.UID == uid);
                if (user != null)
                {
                    _users.Remove(user);
                    SaveToFile();
                    Console.WriteLine($"Пользователь '{user.Username}' удален из базы данных.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в файл: {ex.Message}");
            }
        }

        public List<User> GetAllUsers()
        {
            return new List<User>(_users);
        }
    }
}