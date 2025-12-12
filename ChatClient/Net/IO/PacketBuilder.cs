using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    public class PacketBuilder : IDisposable
    {
        private readonly MemoryStream _ms;
        private bool _isDisposed = false;

        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            if (_isDisposed) throw new ObjectDisposedException("PacketBuilder");
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg)
        {
            if (_isDisposed) throw new ObjectDisposedException("PacketBuilder");

            if (string.IsNullOrEmpty(msg))
            {
                _ms.Write(BitConverter.GetBytes(0));
                return;
            }

            byte[] msgBytes = Encoding.Unicode.GetBytes(msg);
            int byteLength = msgBytes.Length;
            _ms.Write(BitConverter.GetBytes(byteLength));
            _ms.Write(msgBytes);
        }

        public byte[] GetPacketBytes()
        {
            if (_isDisposed) throw new ObjectDisposedException("PacketBuilder");
            return _ms.ToArray();
        }

        public void Dispose()
        {
            _ms?.Dispose();
            _isDisposed = true;
        }
    }
}
