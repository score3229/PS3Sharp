using System.Net.Sockets;

namespace PS3Sharp.Backends.TMAPI
{
    internal class TMAPIConnection : IDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private uint _sequence;
        private bool _disposed;

        internal const int DEFAULT_PORT = 1000;
        internal const int TIMEOUT_MS = 5000;
        internal const int HANDSHAKE_TIMEOUT_MS = 1000;

        internal bool IsConnected => _client?.Connected ?? false;

        internal bool Connect(string ipAddress, int port = DEFAULT_PORT)
        {
            try
            {
                _client = new TcpClient();
                _client.SendTimeout = TIMEOUT_MS;
                _client.ReceiveTimeout = TIMEOUT_MS;
                _client.Connect(ipAddress, port);
                _stream = _client.GetStream();

                if (!InitHandshake())
                    return false;

                AttachProcess();
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
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }

        internal uint NextSequence() => _sequence++;

        // send a packet and receive the response
        internal byte[]? SendCommand(byte[] packet)
        {
            if (_stream == null)
                throw new InvalidOperationException("[TMAPI] - Not connected.");

            _stream.Write(packet, 0, packet.Length);
            return ReceivePacket();
        }

        private bool InitHandshake()
        {
            if (_stream == null)
                return false;

            // send MT init
            var initPacket = TMAPIProtocol.BuildInitPacket();
            _stream.Write(initPacket, 0, initPacket.Length);

            // receive TM response
            var tmResponse = ReceivePacket();
            if (tmResponse == null)
                return false;

            // receive and ack all TH notifications
            _stream.ReadTimeout = HANDSHAKE_TIMEOUT_MS;
            while (true)
            {
                try
                {
                    var notification = ReceivePacket();
                    if (notification == null)
                        break;

                    ushort pktType = TMAPIProtocol.GetPacketType(notification);
                    if (pktType == TMAPIProtocol.TYPE_TH)
                    {
                        var ack = TMAPIProtocol.BuildAckPacket(notification);
                        _stream.Write(ack, 0, ack.Length);
                    }
                }
                catch (IOException)
                {
                    break;
                }
            }

            _stream.ReadTimeout = TIMEOUT_MS;
            _sequence = 0x1000;
            return true;
        }

        private void AttachProcess()
        {
            if (_stream == null) return;

            // attach sequence: attach(x2) → continue → consume notifications → finalize, repeated twice
            for (int round = 0; round < 2; round++)
            {
                SendCommand(TMAPIProtocol.BuildSimpleCommand(TMAPIProtocol.CMD_PROCESS_ATTACH, NextSequence()));
                SendCommand(TMAPIProtocol.BuildSimpleCommand(TMAPIProtocol.CMD_PROCESS_ATTACH, NextSequence()));
                SendCommand(TMAPIProtocol.BuildSimpleCommand(TMAPIProtocol.CMD_PROCESS_CONTINUE, NextSequence()));

                // consume any thread start notifications
                DrainNotifications();

                SendCommand(TMAPIProtocol.BuildSimpleCommand(TMAPIProtocol.CMD_PROCESS_ATTACH_FINALIZE, NextSequence()));
            }

            // get process list to confirm attached state
            SendCommand(TMAPIProtocol.BuildGetProcessListPacket(NextSequence()));
        }

        // read and discard any unsolicited TH notifications
        private void DrainNotifications()
        {
            if (_stream == null) return;

            _stream.ReadTimeout = HANDSHAKE_TIMEOUT_MS;
            while (true)
            {
                try
                {
                    var pkt = ReceivePacket();
                    if (pkt == null) break;

                    // if it's a response to our command (not unsolicited), stop draining
                    ushort pktType = TMAPIProtocol.GetPacketType(pkt);
                    if (pktType == TMAPIProtocol.TYPE_TH)
                    {
                        byte status = TMAPIProtocol.GetStatus(pkt);
                        if (status == TMAPIProtocol.STATUS_SUCCESS)
                            continue; // unsolicited notification, keep draining
                    }
                }
                catch (IOException)
                {
                    break;
                }
            }
            _stream.ReadTimeout = TIMEOUT_MS;
        }

        private byte[]? ReceivePacket()
        {
            if (_stream == null)
                return null;

            // read 8-byte header
            var header = new byte[8];
            int read = ReadExact(header, 0, 8);
            if (read < 8)
                return null;

            ushort totalLength = (ushort)((header[6] << 8) | header[7]);
            int remaining = totalLength - 8;
            if (remaining <= 0)
                return header;

            var packet = new byte[totalLength];
            Buffer.BlockCopy(header, 0, packet, 0, 8);
            read = ReadExact(packet, 8, remaining);
            if (read < remaining)
                return null;

            return packet;
        }

        private int ReadExact(byte[] buffer, int offset, int count)
        {
            if (_stream == null) return 0;

            int totalRead = 0;
            while (totalRead < count)
            {
                int read = _stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0)
                    break;
                totalRead += read;
            }
            return totalRead;
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
