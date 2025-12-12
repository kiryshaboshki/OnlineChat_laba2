using System;
using System.Windows;
using ChatClient.MVVM.ViewModel;

namespace ChatClient
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Подписываемся на события
            _viewModel.LoginSuccess += OnLoginSuccess;
            _viewModel.RegisterSuccess += OnRegisterSuccess;

            // Обработчики изменения паролей
            LoginPassword.PasswordChanged += (s, e) => _viewModel.LoginPassword = LoginPassword.Password;
            RegisterPassword.PasswordChanged += (s, e) => _viewModel.RegisterPassword = RegisterPassword.Password;
            RegisterConfirmPassword.PasswordChanged += (s, e) => _viewModel.RegisterConfirmPassword = RegisterConfirmPassword.Password;
        }

        private void OnLoginSuccess(string username, string password)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Вход выполнен: {username}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Открываем главное окно чата
                OpenMainWindow(username);
            });
        }

        private void OnRegisterSuccess(string username, string password)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Пользователь {username} зарегистрирован", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Переключаем на вкладку входа
                var tabControl = FindName("TabControl") as System.Windows.Controls.TabControl;
                if (tabControl != null)
                {
                    tabControl.SelectedIndex = 0;
                }

                // Очищаем поля регистрации
                _viewModel.RegisterUsername = "";
                _viewModel.RegisterPassword = "";
                _viewModel.RegisterConfirmPassword = "";
            });
        }

        private void OpenMainWindow(string username)
        {
            try
            {
                var mainWindow = new MVVM.View.MainWindow();
                var mainViewModel = mainWindow.DataContext as MVVM.ViewModel.MainViewModel;

                if (mainViewModel != null)
                {
                    mainViewModel.SetUsername(username);
                }

                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия главного окна: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик смены вкладок для очистки ошибок
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _viewModel.ClearErrors();
        }
    }
}