using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PS3Sharp.Backends.MAPI
{
    internal class MAPIConnection : IDisposable
    {
        private Socket? _socket;
        private bool _disposed;

        internal const int TIMEOUT_MS = 5000;

        internal bool IsConnected => _socket?.Connected ?? false;

        internal bool Connect(string ipAddress, int port = MAPIProtocol.DEFAULT_PORT)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.ReceiveTimeout = TIMEOUT_MS;
                _socket.SendTimeout = TIMEOUT_MS;
                _socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));

                // read welcome banner (220) and connected message (230)
                ReceiveLine();
                ReceiveLine();

                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }

        internal void Disconnect()
        {
            // best-effort disconnect command, don't block teardown if it fails
            try { if (_socket?.Connected == true) SendLine(MAPIProtocol.Disconnect()); }
            catch { }

            _socket?.Close();
            _socket = null;
        }

        internal (int code, string message) SendCommand(string command)
        {
            SendLine(command);
            var line = ReceiveLine();
            if (line == null)
                return (-1, "No response");
            return MAPIProtocol.ParseResponse(line);
        }

        // open PASV data connection, send command, read binary response
        internal byte[] ReadViaPassive(string command, string ipAddress)
        {
            var dataSocket = OpenPassiveConnection(ipAddress);

            SendLine(command);
            var cmdResponse = ReceiveLine();
            var (code, _) = MAPIProtocol.ParseResponse(cmdResponse ?? "");
            if (code != MAPIProtocol.DATA_OPEN && code != MAPIProtocol.OK)
                throw new InvalidOperationException($"[MAPI] - Unexpected response: {cmdResponse}");

            // read all data from the passive socket
            var data = ReadAllFromSocket(dataSocket);
            dataSocket.Close();

            // read the 226 closing response
            ReceiveLine();

            return data;
        }

        // open PASV data connection, send command, write binary data
        internal void WriteViaPassive(string command, byte[] data, string ipAddress)
        {
            var dataSocket = OpenPassiveConnection(ipAddress);

            SendLine(command);
            var cmdResponse = ReceiveLine();
            var (code, _) = MAPIProtocol.ParseResponse(cmdResponse ?? "");
            if (code != MAPIProtocol.DATA_OPEN && code != MAPIProtocol.OK)
                throw new InvalidOperationException($"[MAPI] - Unexpected response: {cmdResponse}");

            // send data on the passive socket
            dataSocket.Send(data, data.Length, SocketFlags.None);
            dataSocket.Close();

            // read the 226 closing response
            ReceiveLine();
        }

        private Socket OpenPassiveConnection(string ipAddress)
        {
            var (code, message) = SendCommand(MAPIProtocol.Pasv());
            if (code != MAPIProtocol.OK && code != 227)
                throw new InvalidOperationException($"[MAPI] - PASV failed: {code} {message}");

            var parsed = MAPIProtocol.ParsePasv(message);
            if (parsed == null)
                throw new InvalidOperationException($"[MAPI] - Failed to parse PASV response: {message}");

            var (ip, port) = parsed.Value;

            // some PS3s return 0.0.0.0, use the original IP
            if (ip == "0.0.0.0" || ip.StartsWith("127."))
                ip = ipAddress;

            var dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            dataSocket.ReceiveTimeout = TIMEOUT_MS;
            dataSocket.SendTimeout = TIMEOUT_MS;
            dataSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            return dataSocket;
        }

        private void SendLine(string line)
        {
            if (_socket == null)
                throw new InvalidOperationException("[MAPI] - Not connected.");

            byte[] data = Encoding.ASCII.GetBytes(line + MAPIProtocol.LINE_ENDING);
            _socket.Send(data, data.Length, SocketFlags.None);
        }

        private string? ReceiveLine()
        {
            if (_socket == null) return null;

            var sb = new StringBuilder();
            byte[] buf = new byte[1];

            while (_socket.Receive(buf, 1, SocketFlags.None) > 0)
            {
                sb.Append((char)buf[0]);
                if (sb.Length >= 2 && sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n')
                    break;
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        private static byte[] ReadAllFromSocket(Socket socket)
        {
            using var ms = new MemoryStream();
            byte[] buffer = new byte[4096];
            int read;

            while ((read = socket.Receive(buffer, buffer.Length, SocketFlags.None)) > 0)
                ms.Write(buffer, 0, read);

            return ms.ToArray();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }
    }
}
