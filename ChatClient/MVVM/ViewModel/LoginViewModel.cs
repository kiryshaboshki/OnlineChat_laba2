using ChatClient.MVVM.core;
using ChatClient.Net;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ChatClient.MVVM.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private Server _server;

        private string _loginUsername;
        public string LoginUsername
        {
            get => _loginUsername;
            set { _loginUsername = value; OnPropertyChanged(); }
        }

        private string _loginPassword;
        public string LoginPassword
        {
            get => _loginPassword;
            set { _loginPassword = value; OnPropertyChanged(); }
        }

        private string _registerUsername;
        public string RegisterUsername
        {
            get => _registerUsername;
            set { _registerUsername = value; OnPropertyChanged(); }
        }

        private string _registerPassword;
        public string RegisterPassword
        {
            get => _registerPassword;
            set { _registerPassword = value; OnPropertyChanged(); }
        }

        private string _registerConfirmPassword;
        public string RegisterConfirmPassword
        {
            get => _registerConfirmPassword;
            set { _registerConfirmPassword = value; OnPropertyChanged(); }
        }

        private string _loginError;
        public string LoginError
        {
            get => _loginError;
            set { _loginError = value; OnPropertyChanged(); }
        }

        private string _registerError;
        public string RegisterError
        {
            get => _registerError;
            set { _registerError = value; OnPropertyChanged(); }
        }

        private bool _hasLoginError;
        public bool HasLoginError
        {
            get => _hasLoginError;
            set { _hasLoginError = value; OnPropertyChanged(); }
        }

        private bool _hasRegisterError;
        public bool HasRegisterError
        {
            get => _hasRegisterError;
            set { _hasRegisterError = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }

        public event Action<string, string> LoginSuccess;
        public event Action<string, string> RegisterSuccess;
        public event PropertyChangedEventHandler PropertyChanged;

        public LoginViewModel()
        {
            _server = new Server();

            LoginCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(LoginUsername))
                {
                    ShowLoginError("Введите имя пользователя");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LoginPassword))
                {
                    ShowLoginError("Введите пароль");
                    return;
                }

                try
                {
                    LoginSuccess?.Invoke(LoginUsername, LoginPassword);
                }
                catch (Exception ex)
                {
                    ShowLoginError($"Ошибка входа: {ex.Message}");
                }
            });

            RegisterCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(RegisterUsername))
                {
                    ShowRegisterError("Введите имя пользователя");
                    return;
                }

                if (string.IsNullOrWhiteSpace(RegisterPassword))
                {
                    ShowRegisterError("Введите пароль");
                    return;
                }

                if (RegisterPassword != RegisterConfirmPassword)
                {
                    ShowRegisterError("Пароли не совпадают");
                    return;
                }

                if (RegisterPassword.Length < 6)
                {
                    ShowRegisterError("Пароль должен быть не менее 6 символов");
                    return;
                }

                try
                {
                    RegisterSuccess?.Invoke(RegisterUsername, RegisterPassword);
                }
                catch (Exception ex)
                {
                    ShowRegisterError($"Ошибка регистрации: {ex.Message}");
                }
            });
        }

        private void ShowLoginError(string message)
        {
            LoginError = message;
            HasLoginError = true;
        }

        private void ShowRegisterError(string message)
        {
            RegisterError = message;
            HasRegisterError = true;
        }

        public void ClearErrors()
        {
            HasLoginError = false;
            HasRegisterError = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}