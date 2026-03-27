namespace PS3Sharp.Backends.TMAPI
{
    internal static class TMAPIProtocol
    {
        // packet header magic
        private static readonly byte[] Magic = { 0x30, 0x10, 0x00, 0x00, 0x00, 0x00 };

        // message types
        internal const ushort TYPE_MT = 0x4D54; // client control
        internal const ushort TYPE_TM = 0x544D; // server control response
        internal const ushort TYPE_HT = 0x4854; // client data
        internal const ushort TYPE_TH = 0x5448; // server data response

        // commands
        internal const ushort CMD_INIT = 0x0002;
        internal const ushort CMD_ACK = 0x0004;
        internal const ushort CMD_PROCESS_CONTINUE = 0x0002;
        internal const ushort CMD_PROCESS_ATTACH = 0x0102;
        internal const ushort CMD_PROCESS_ATTACH_FINALIZE = 0x0202;
        internal const ushort CMD_GET_PROCESS_LIST = 0x0005;
        internal const ushort CMD_GET_MEMORY = 0x0300;
        internal const ushort CMD_SET_MEMORY = 0x0301;

        // flags
        internal const ushort FLAG_PROCESS_CMD = 0x0020;

        // status
        internal const byte STATUS_SUCCESS = 0x80;

        internal static byte CalculateChecksum(byte[] payload)
        {
            int sum = 0x40;
            for (int i = 0; i < payload.Length; i++)
                sum += payload[i];
            return (byte)(sum & 0xFF);
        }

        internal static byte[] BuildHeader(ushort totalLength)
        {
            var header = new byte[8];
            Buffer.BlockCopy(Magic, 0, header, 0, 6);
            header[6] = (byte)(totalLength >> 8);
            header[7] = (byte)(totalLength & 0xFF);
            return header;
        }

        internal static byte[] BuildInitPacket(string lparName = "PS3_LPAR")
        {
            // MT init: 52 bytes total
            var pkt = new byte[52];
            Buffer.BlockCopy(BuildHeader(52), 0, pkt, 0, 8);

            // MT type
            pkt[8] = 0x4D; pkt[9] = 0x54;
            // version/flags: 00 00
            // target: 00 00 00 01
            pkt[15] = 0x01;
            // command group: 00 00
            // command: 00 02
            pkt[19] = 0x02;

            // LPAR name at offset 20, padded to 32 bytes
            var nameBytes = System.Text.Encoding.ASCII.GetBytes(lparName);
            Buffer.BlockCopy(nameBytes, 0, pkt, 20, Math.Min(nameBytes.Length, 31));

            return pkt;
        }

        internal static byte[] BuildAckPacket(byte[] notificationData)
        {
            // MT ack: 24 bytes total
            // echo 6 bytes from notification offset 18-23
            var pkt = new byte[24];
            Buffer.BlockCopy(BuildHeader(24), 0, pkt, 0, 8);

            // MT type
            pkt[8] = 0x4D; pkt[9] = 0x54;
            // version/flags: 00 00
            // target: 00 00 00 01
            pkt[15] = 0x01;
            // command: 00 04 (ack)
            pkt[17] = 0x04;

            // echo notification data
            if (notificationData.Length >= 24)
                Buffer.BlockCopy(notificationData, 18, pkt, 18, 6);

            return pkt;
        }

        // builds a simple HT command with no data payload (used for attach/continue)
        internal static byte[] BuildSimpleCommand(ushort command, uint sequence, ushort flags = 0x0000)
        {
            var payload = new byte[24];
            int pos = 0;

            payload[pos++] = 0x48; payload[pos++] = 0x54;
            payload[pos++] = 0x00; payload[pos++] = 0x10;
            payload[pos++] = 0x00; payload[pos++] = 0x00;
            payload[pos++] = 0x02; payload[pos++] = 0x00;

            // flags + command
            payload[pos++] = (byte)(flags >> 8);
            payload[pos++] = (byte)(flags & 0xFF);
            payload[pos++] = (byte)(command >> 8);
            payload[pos++] = (byte)(command & 0xFF);

            // sequence
            payload[pos++] = (byte)(sequence >> 24);
            payload[pos++] = (byte)(sequence >> 16);
            payload[pos++] = (byte)(sequence >> 8);
            payload[pos++] = (byte)(sequence & 0xFF);

            // payload length: 0
            pos += 4;

            // unit type PPU + flags
            payload[pos++] = 0x01; payload[pos++] = 0x01;
            payload[pos++] = 0x02; payload[pos++] = 0x00;

            return WrapWithHeaderAndChecksum(payload);
        }

        // builds GetProcessList command (has flag 0x0020)
        internal static byte[] BuildGetProcessListPacket(uint sequence)
        {
            return BuildSimpleCommand(CMD_GET_PROCESS_LIST, sequence, FLAG_PROCESS_CMD);
        }

        internal static byte[] BuildReadPacket(uint address, int length, uint sequence)
        {
            // payload: HT header(8) + status/cmd(4) + seq(4) + payloadLen(4) + unit(4) + thread(4) + addr(4) + pad(4) + len(4) = 40 bytes
            var payload = new byte[40];
            int pos = 0;

            // HT type + flags
            payload[pos++] = 0x48; payload[pos++] = 0x54; // HT
            payload[pos++] = 0x00; payload[pos++] = 0x10; // flags
            payload[pos++] = 0x00; payload[pos++] = 0x00; // reserved
            payload[pos++] = 0x02; payload[pos++] = 0x00; // target/session

            // status (0 for request) + command
            payload[pos++] = 0x00; payload[pos++] = 0x00;
            payload[pos++] = (byte)(CMD_GET_MEMORY >> 8);
            payload[pos++] = (byte)(CMD_GET_MEMORY & 0xFF);

            // sequence number (big-endian)
            payload[pos++] = (byte)(sequence >> 24);
            payload[pos++] = (byte)(sequence >> 16);
            payload[pos++] = (byte)(sequence >> 8);
            payload[pos++] = (byte)(sequence & 0xFF);

            // payload length: 16 (thread + addr + pad + readlen)
            payload[pos++] = 0x00; payload[pos++] = 0x00;
            payload[pos++] = 0x00; payload[pos++] = 0x10;

            // unit type PPU + flags
            payload[pos++] = 0x01; payload[pos++] = 0x01;
            payload[pos++] = 0x02; payload[pos++] = 0x00;

            // thread ID (0)
            pos += 4;

            // address (big-endian)
            payload[pos++] = (byte)(address >> 24);
            payload[pos++] = (byte)(address >> 16);
            payload[pos++] = (byte)(address >> 8);
            payload[pos++] = (byte)(address & 0xFF);

            // padding
            pos += 4;

            // read length (big-endian)
            payload[pos++] = (byte)(length >> 24);
            payload[pos++] = (byte)(length >> 16);
            payload[pos++] = (byte)(length >> 8);
            payload[pos++] = (byte)(length & 0xFF);

            return WrapWithHeaderAndChecksum(payload);
        }

        internal static byte[] BuildWritePacket(uint address, byte[] data, uint sequence)
        {
            // payload: HT header(8) + status/cmd(4) + seq(4) + payloadLen(4) + unit(4) + thread(4) + addr(4) + data(N) = 32+N bytes
            int payloadSize = 32 + data.Length;
            var payload = new byte[payloadSize];
            int pos = 0;

            // HT type + flags
            payload[pos++] = 0x48; payload[pos++] = 0x54;
            payload[pos++] = 0x00; payload[pos++] = 0x10;
            payload[pos++] = 0x00; payload[pos++] = 0x00;
            payload[pos++] = 0x02; payload[pos++] = 0x00;

            // status + command
            payload[pos++] = 0x00; payload[pos++] = 0x00;
            payload[pos++] = (byte)(CMD_SET_MEMORY >> 8);
            payload[pos++] = (byte)(CMD_SET_MEMORY & 0xFF);

            // sequence
            payload[pos++] = (byte)(sequence >> 24);
            payload[pos++] = (byte)(sequence >> 16);
            payload[pos++] = (byte)(sequence >> 8);
            payload[pos++] = (byte)(sequence & 0xFF);

            // payload length: thread(4) + addr(4) + data(N)
            int cmdPayloadLen = 8 + data.Length;
            payload[pos++] = (byte)(cmdPayloadLen >> 24);
            payload[pos++] = (byte)(cmdPayloadLen >> 16);
            payload[pos++] = (byte)(cmdPayloadLen >> 8);
            payload[pos++] = (byte)(cmdPayloadLen & 0xFF);

            // unit type PPU + flags
            payload[pos++] = 0x01; payload[pos++] = 0x01;
            payload[pos++] = 0x02; payload[pos++] = 0x00;

            // thread ID (0)
            pos += 4;

            // address
            payload[pos++] = (byte)(address >> 24);
            payload[pos++] = (byte)(address >> 16);
            payload[pos++] = (byte)(address >> 8);
            payload[pos++] = (byte)(address & 0xFF);

            // data
            Buffer.BlockCopy(data, 0, payload, pos, data.Length);

            return WrapWithHeaderAndChecksum(payload);
        }

        private static byte[] WrapWithHeaderAndChecksum(byte[] payload)
        {
            int totalLength = 8 + payload.Length + 1;
            var packet = new byte[totalLength];
            Buffer.BlockCopy(BuildHeader((ushort)totalLength), 0, packet, 0, 8);
            Buffer.BlockCopy(payload, 0, packet, 8, payload.Length);
            packet[totalLength - 1] = CalculateChecksum(payload);
            return packet;
        }

        internal static ushort GetPacketType(byte[] packet) => (ushort)((packet[8] << 8) | packet[9]);
        internal static ushort GetTotalLength(byte[] packet) => (ushort)((packet[6] << 8) | packet[7]);
        internal static byte GetStatus(byte[] packet) => packet[16];
        internal static ushort GetCommand(byte[] packet) => (ushort)((packet[18] << 8) | packet[19]);

        internal static byte[] ExtractReadData(byte[] response)
        {
            // response layout:
            //   header(8) + type/flags(8) + status/cmd(4) + seq(4) + payloadLen(4) +
            //   unit/flags(4) + thread(4) + padding(4) + address(4) + DATA(N) + trailing(1)
            // data starts at offset 44
            const int dataOffset = 44;
            int totalLen = GetTotalLength(response);

            if (response.Length < dataOffset + 1 || totalLen < dataOffset + 1)
                return Array.Empty<byte>();

            int dataLen = totalLen - dataOffset - 1;

            if (dataLen <= 0)
                return Array.Empty<byte>();

            var data = new byte[dataLen];
            Buffer.BlockCopy(response, dataOffset, data, 0, dataLen);
            return data;
        }
    }
}
