using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    class PacketReader : BinaryReader
    {
        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            try
            {
                // Читаем длину сообщения в байтах
                int byteLength = ReadInt32();
                Console.WriteLine($"Длина сообщения для чтения: {byteLength} байт");

                if (byteLength <= 0 || byteLength > 100000)
                {
                    Console.WriteLine($"Некорректная длина сообщения: {byteLength}");
                    return string.Empty;
                }

                byte[] msgBuffer = new byte[byteLength];

                // Читаем все байты
                int totalRead = 0;
                while (totalRead < byteLength)
                {
                    int bytesRead = _ns.Read(msgBuffer, totalRead, byteLength - totalRead);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"Прочитано 0 байт, всего прочитано: {totalRead} из {byteLength}");
                        break;
                    }
                    totalRead += bytesRead;
                }

                if (totalRead < byteLength)
                {
                    Console.WriteLine($"Не удалось прочитать все байты: {totalRead} из {byteLength}");
                    return string.Empty;
                }

                string message = Encoding.Unicode.GetString(msgBuffer);
                Console.WriteLine($"Прочитано сообщение: '{message}'");
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения сообщения: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
