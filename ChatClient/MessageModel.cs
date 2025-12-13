using System;

namespace ChatClient.MVVM.Model
{
    public class MessageModel
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        // Для отображения в списке
        public string DisplayText => $"[{Timestamp:HH:mm}] {Username}: {Message}";
    }
}