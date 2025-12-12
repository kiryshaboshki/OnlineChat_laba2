using ChatClient.MVVM.core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatClient.Net.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = new Server();

            // Подписываемся на события (должны быть объявлены в Server.cs)
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Команды
            ConnectToServerCommand = new RelayCommand(
                o => _server.ConnectToServer(Username),
                o => !string.IsNullOrEmpty(Username));

            SendMessageCommand = new RelayCommand(
                o =>
                {
                    _server.SendMessageToServer(Message);
                    Message = ""; // Очищаем поле сообщения после отправки
                },
                o => !string.IsNullOrEmpty(Message));
        }

        // Метод для установки имени пользователя после авторизации
        public void SetUsername(string username)
        {
            Username = username;
        }

        private void RemoveUser()
        {
            try
            {
                var uid = _server.PacketReader.ReadMessage();
                var user = Users.FirstOrDefault(x => x.UID == uid);
                if (user != null)
                {
                    Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        private void MessageReceived()
        {
            try
            {
                var msg = _server.PacketReader.ReadMessage();
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сообщения: {ex.Message}");
            }
        }

        private void UserConnected()
        {
            try
            {
                Console.WriteLine("UserConnected вызван");
                var username = _server.PacketReader.ReadMessage();
                var uid = _server.PacketReader.ReadMessage();

                Console.WriteLine($"Новый пользователь: {username}, UID: {uid}");

                var user = new UserModel
                {
                    Username = username,
                    UID = uid,
                };

                if (!Users.Any(x => x.UID == user.UID))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Users.Add(user);
                        Console.WriteLine($"Пользователь {user.Username} добавлен в список");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в UserConnected: {ex.Message}");
            }
        }
    }
}