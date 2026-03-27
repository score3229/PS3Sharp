namespace PS3Sharp.Backends.MAPI
{
    internal static class MAPIProtocol
    {
        internal const int DEFAULT_PORT = 7887;
        internal const string LINE_ENDING = "\r\n";

        // response codes
        internal const int OK = 200;
        internal const int DATA_OPEN = 150;
        internal const int DATA_CLOSE = 226;

        // commands
        internal static string GetAllPid() => "PROCESS GETALLPID";
        internal static string GetProcessName(uint pid) => $"PROCESS GETNAME {pid}";
        internal static string MemoryGet(uint pid, ulong address, int size) => $"MEMORY GET {pid} {address:X16} {size}";
        internal static string MemorySet(uint pid, ulong address) => $"MEMORY SET {pid} {address:X16}";
        internal static string Pasv() => "PASV";
        internal static string Disconnect() => "DISCONNECT";
        internal static string TypeBinary() => "TYPE I";

        internal static (int code, string message) ParseResponse(string response)
        {
            if (string.IsNullOrEmpty(response) || response.Length < 3)
                return (-1, response ?? "");

            string trimmed = response.TrimEnd('\r', '\n');
            int spaceIndex = trimmed.IndexOf(' ');

            if (spaceIndex > 0 && int.TryParse(trimmed.Substring(0, spaceIndex), out int code))
                return (code, trimmed.Substring(spaceIndex + 1));

            if (int.TryParse(trimmed, out int codeOnly))
                return (codeOnly, "");

            return (-1, trimmed);
        }

        internal static uint[] ParsePidList(string message)
        {
            // format: "pid1|pid2|pid3|"
            var parts = message.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var pids = new List<uint>();

            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed == "0")
                    continue;

                if (uint.TryParse(trimmed, out uint pid))
                    pids.Add(pid);
            }

            return pids.ToArray();
        }

        // parse PASV response to extract data connection ip:port
        // format varies: "227 Entering Passive Mode (h1,h2,h3,h4,p1,p2)"
        internal static (string ip, int port)? ParsePasv(string message)
        {
            int start = message.IndexOf('(');
            int end = message.IndexOf(')');

            string numStr;
            if (start >= 0 && end > start)
                numStr = message.Substring(start + 1, end - start - 1);
            else
            {
                // fallback: find comma-separated numbers
                start = -1;
                for (int i = 0; i < message.Length; i++)
                {
                    if (char.IsDigit(message[i])) { start = i; break; }
                }
                if (start < 0) return null;
                numStr = message.Substring(start).TrimEnd('\r', '\n', ' ', '.');
            }

            var parts = numStr.Split(',');
            if (parts.Length < 6) return null;

            string ip = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
            if (int.TryParse(parts[4], out int p1) && int.TryParse(parts[5], out int p2))
                return (ip, (p1 << 8) + p2);

            return null;
        }
    }
}
