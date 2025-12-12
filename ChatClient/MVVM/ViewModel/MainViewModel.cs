using ChatClient.MVVM.core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatClient.Net.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public ICommand ConnectToServerCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }

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

            // Подписываемся на события
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            // Команда подключения - используется после авторизации
            ConnectToServerCommand = new RelayCommand(
                o =>
                {
                    // Показываем информационное сообщение, т.к. подключение уже установлено после авторизации
                    MessageBox.Show("Вы уже подключены к чату после авторизации.\n" +
                                  "Отправляйте сообщения в поле ниже.",
                                  "Информация",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                },
                o => !string.IsNullOrEmpty(Username));

            // Команда отправки сообщения
            SendMessageCommand = new RelayCommand(
                o =>
                {
                    if (!string.IsNullOrEmpty(Message))
                    {
                        _server.SendMessageToServer(Message);

                        // Добавляем свое сообщение сразу в историю
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Messages.Add($"Вы ({DateTime.Now:HH:mm}): {Message}");
                        });

                        Message = ""; // Очищаем поле сообщения после отправки
                    }
                },
                o => !string.IsNullOrEmpty(Message) && _server != null);
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Users.Remove(user);
                        Messages.Add($"[СИСТЕМА] {user.Username} покинул чат");
                    });
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(msg);

                    // Автоматическая прокрутка к последнему сообщению
                    if (Messages.Count > 0)
                    {
                        // Тут можно добавить логику для автоскролла в UI
                    }
                });
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
                        Messages.Add($"[СИСТЕМА] {user.Username} присоединился к чату");
                        Console.WriteLine($"Пользователь {user.Username} добавлен в список");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в UserConnected: {ex.Message}");
            }
        }

        // Метод для принудительного обновления интерфейса
        public void ForceUpdate()
        {
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(Messages));
        }
    }
}