using System.Reflection;
using System.Runtime.InteropServices;

namespace PS3Sharp.Backends.TMAPI
{
    // dynamically loads ps3tmapi_net.dll when Target Manager is installed
    // routes through TM's service so it works alongside ProDG
    internal class TMAPIDllConnection : IDisposable
    {
        private Assembly? _assembly;
        private Type? _api;
        private int _target;
        private uint _processId;
        private bool _disposed;

        private static readonly string[] DllPaths =
        {
            @"C:\Program Files\SN Systems\PS3\bin\ps3tmapi_net.dll",
            @"C:\Program Files (x64)\SN Systems\PS3\bin\ps3tmapi_net.dll",
            @"C:\Program Files (x86)\SN Systems\PS3\bin\ps3tmapi_net.dll",
        };

        internal bool IsConnected { get; private set; }

        internal static bool IsAvailable()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            return DllPaths.Any(File.Exists);
        }

        internal bool Connect(int targetIndex = 0)
        {
            _target = targetIndex;

            // load the dll
            string? dllPath = DllPaths.FirstOrDefault(File.Exists);
            if (dllPath == null)
                return false;

            _assembly = Assembly.LoadFrom(dllPath);
            _api = _assembly.GetType("PS3TMAPI");
            if (_api == null)
                return false;

            // init + connect
            int res = CallStatic<int>("InitTargetComms");
            if (res < 0) return false;

            res = CallStatic<int>("Connect", _target, (string?)null);
            if (res < 0) return false;

            IsConnected = true;
            return true;
        }

        internal bool AttachProcess()
        {
            if (_api == null) return false;

            // get process list
            uint[]? pids = null;
            var getListMethod = _api.GetMethod("GetProcessList",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(int), typeof(uint[]).MakeByRefType() }, null);

            if (getListMethod == null) return false;

            object?[] getListArgs = { _target, null };
            int res = (int)(getListMethod.Invoke(null, getListArgs) ?? -1);
            if (res < 0) return false;

            pids = getListArgs[1] as uint[];
            if (pids == null || pids.Length == 0) return false;

            _processId = pids[0];

            // attach + continue (UnitType.PPU = 0)
            CallStatic<int>("ProcessAttach", _target, 0 /* PPU */, _processId);
            CallStatic<int>("ProcessContinue", _target, _processId);

            return true;
        }

        internal void Disconnect()
        {
            if (IsConnected && _api != null)
            {
                try { CallStatic<int>("Disconnect", _target); } catch { }
                IsConnected = false;
            }
        }

        internal byte[] ReadMemory(uint address, int length)
        {
            if (_api == null)
                throw new InvalidOperationException("[TMAPI] - DLL not loaded.");

            byte[] buffer = new byte[length];

            var method = _api.GetMethod("ProcessGetMemory",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(int), _api.GetNestedType("UnitType")!, typeof(uint), typeof(ulong), typeof(ulong), typeof(byte[]).MakeByRefType() }, null);

            if (method == null)
                throw new InvalidOperationException("[TMAPI] - ProcessGetMemory not found in DLL.");

            // PPU = 0, thread = 0
            object unitPPU = Enum.ToObject(_api.GetNestedType("UnitType")!, 0);
            object?[] args = { _target, unitPPU, _processId, (ulong)0, (ulong)address, buffer };
            int res = (int)(method.Invoke(null, args) ?? -1);

            if (res < 0)
                throw new InvalidOperationException($"[TMAPI] - ProcessGetMemory failed at 0x{address:X}, result: {res}.");

            return (byte[])(args[5] ?? buffer);
        }

        internal void WriteMemory(uint address, byte[] data)
        {
            if (_api == null)
                throw new InvalidOperationException("[TMAPI] - DLL not loaded.");

            var method = _api.GetMethod("ProcessSetMemory",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(int), _api.GetNestedType("UnitType")!, typeof(uint), typeof(ulong), typeof(ulong), typeof(byte[]) }, null);

            if (method == null)
                throw new InvalidOperationException("[TMAPI] - ProcessSetMemory not found in DLL.");

            object unitPPU = Enum.ToObject(_api.GetNestedType("UnitType")!, 0);
            object?[] args = { _target, unitPPU, _processId, (ulong)0, (ulong)address, data };
            int res = (int)(method.Invoke(null, args) ?? -1);

            if (res < 0)
                throw new InvalidOperationException($"[TMAPI] - ProcessSetMemory failed at 0x{address:X}, result: {res}.");
        }

        private T CallStatic<T>(string methodName, params object?[] args)
        {
            if (_api == null)
                throw new InvalidOperationException("[TMAPI] - DLL not loaded.");

            var method = _api.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);

            if (method == null)
                throw new InvalidOperationException($"[TMAPI] - Method {methodName} not found.");

            return (T)(method.Invoke(null, args) ?? default(T)!);
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
