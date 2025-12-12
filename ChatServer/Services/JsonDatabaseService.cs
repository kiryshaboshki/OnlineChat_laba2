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
                    Console.WriteLine($"Загружено {_users.Count} пользователей.");
                }
                else
                {
                    _users = new List<User>();
                    SaveToFile(); // Создаем пустой файл
                    Console.WriteLine("Создан новый файл базы данных.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке пользователей: {ex.Message}");
                _users = new List<User>();
            }
        }

        public bool RegisterUser(string username, string password)
        {
            try
            {
                // Проверяем, существует ли пользователь
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Пользователь '{username}' уже существует.");
                    return false;
                }

                // Создаем нового пользователя
                var user = new User
                {
                    Username = username,
                    UID = Guid.NewGuid(),
                    PasswordHash = PasswordHasher.HashPassword(password), // Хэшируем пароль
                    ConnectedTime = DateTime.Now,
                    RegisteredDate = DateTime.Now
                };

                _users.Add(user);
                SaveToFile();

                Console.WriteLine($"Зарегистрирован новый пользователь: {username}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            try
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    user.ConnectedTime = DateTime.Now;
                    SaveToFile();
                    Console.WriteLine($"Пользователь {username} успешно аутентифицирован.");
                    return user;
                }

                Console.WriteLine($"Неверные учетные данные для пользователя {username}.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return null;
            }
        }


        public bool UserExists(string username)
        {
            return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void SaveUser(User user)
        {
            try
            {
                _users.RemoveAll(u => u.UID == user.UID);
                _users.Add(user);
                SaveToFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения пользователя: {ex.Message}");
            }
        }

        public void RemoveUser(Guid uid)
        {
            try
            {
                _users.RemoveAll(u => u.UID == uid);
                SaveToFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления пользователя: {ex.Message}");
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения файла: {ex.Message}");
            }
        }
    }
}