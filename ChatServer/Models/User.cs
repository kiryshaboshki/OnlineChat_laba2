using System;

namespace ChatServer.Models
{
    public class User
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public string PasswordHash { get; set; }  
        public DateTime ConnectedTime { get; set; }
        public DateTime RegisteredDate { get; set; }

        public User()
        {
            ConnectedTime = DateTime.Now;
            RegisteredDate = DateTime.Now;
        }
    }
}