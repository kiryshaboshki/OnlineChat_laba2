using System;

namespace ChatClient.Net
{
    public static class ServerManager
    {
        private static Server _serverInstance;

        public static Server Instance
        {
            get
            {
                if (_serverInstance == null)
                {
                    _serverInstance = new Server();
                }
                return _serverInstance;
            }
        }

        public static bool IsConnected => _serverInstance != null && _serverInstance.IsConnected;

        public static string CurrentUsername { get; set; }
        public static string CurrentUID { get; set; }

        public static void Reset()
        {
            _serverInstance = null;
            CurrentUsername = null;
            CurrentUID = null;
        }

        public static void SetUserData(string username, string uid)
        {
            CurrentUsername = username;
            CurrentUID = uid;
        }
    }
}