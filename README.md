# PS3Sharp

PS3Sharp is a cross-platform .NET library for reading and writing memory on **PlayStation 3 (PS3)** consoles and the **RPCS3** emulator. It supports **TMAPI** (DEX), **PS3MAPI** (CEX/DEX via webMAN), **CCAPI** (CEX/DEX), and **RPCS3** — with no native DLL dependencies for cross-platform backends.

**v2.0** is a complete rewrite. PS3Lib and all native dependencies have been dropped. The TMAPI protocol was reverse-engineered from scratch, making PS3Sharp the first library to offer cross-platform TMAPI support — it runs on **Windows, Linux, and macOS** without Target Manager or any Sony tooling installed.

---

## Features

- Cross-platform: Windows, Linux, macOS
- Direct memory access for PS3 consoles and RPCS3 emulator
- Read and write all primitive types: byte, int16, int32, int64, float, double, string, bool
- Built-in `Vector3` and `Vector4` types with `ReadVector3` / `WriteVector3` / `ReadVector4` / `WriteVector4`
- Generic `ReadEnum<T>` / `WriteEnum<T>` with automatic endian conversion
- Generic `ReadStruct<T>` / `WriteStruct<T>` with automatic big-endian field swapping
- Pointer chasing with `GetPointer`
- Multiple backend support: TMAPI, PS3MAPI, CCAPI, RPCS3
- No native DLL dependencies for TMAPI, PS3MAPI, and RPCS3
- Automatic chunking for large memory transfers
- `IDisposable` on all backends for proper resource cleanup
- TMAPI seamlessly uses Target Manager DLL when installed, direct TCP otherwise

---

## Platform Notes

| Backend | x64 | x86 | Notes |
|---------|-----|-----|-------|
| **RPCS3** | **x64 only** | No | Cross-platform, RPCS3 is a 64-bit process |
| **TMAPI** | Yes | Yes | Cross-platform, pure C#. Hooks into Target Manager DLL if installed on Windows |
| **PS3MAPI** | Yes | Yes | Cross-platform, pure C# |
| **CCAPI** | No | **x86 only** | Windows only, requires CCAPI.dll installed |

If your project needs both CCAPI and RPCS3, you will need separate builds for each.

---

## Installation

Clone the repository and include the project in your solution, or reference the built DLL directly.

```
git clone https://github.com/score3229/PS3Sharp.git
```

---

## Usage

### RPCS3 (emulator)

Attaches to the RPCS3 emulator process for direct memory access. Requires **x64** build. Platform-specific process memory APIs are handled automatically. On Linux/macOS, run with `sudo`.

```csharp
using PS3Sharp;
using PS3Sharp.Types;

using var rpcs3 = new PS3Client("rpcs3");

if (rpcs3.Connect())
{
    int value = rpcs3.ReadInt32(0xC0000000);
    Console.WriteLine($"Value: 0x{value:X}");
    rpcs3.Disconnect();
}
```

### TMAPI (DEX)

Connects directly to the PS3 debug port over TCP. No Target Manager or ProDG installation required. Works on Windows, Linux, and macOS. On Windows with Target Manager installed, it automatically hooks into the existing connection so it works alongside ProDG.

```csharp
using var ps3 = new PS3Client(BackendType.TMAPI, "10.0.0.4");

if (ps3.Connect())
{
    ps3.WriteInt32(0x00B00000, 0x1337);
    int value = ps3.ReadInt32(0x00B00000);
    Console.WriteLine($"Value: 0x{value:X}");

    ps3.Disconnect();
}
```

### PS3MAPI (CEX/DEX, webMAN)

Connects to the ps3mapi server included in webMAN-MOD. Requires ps3mapi TCP server enabled on the PS3 (port 7887).

```csharp
using var ps3 = new PS3Client(BackendType.MAPI, "10.0.0.4");

if (ps3.Connect()) // auto-attaches to game process
{
    ps3.WriteFloat(0x00B00000, 13.37f);
    ps3.Disconnect();
}
```

### CCAPI (CEX/DEX)

Connects via the CCAPI DLL. Windows only, requires CCAPI installed and your project built as **x86**.

```csharp
using var ps3 = new PS3Client(BackendType.CCAPI, "10.0.0.4");

if (ps3.Connect()) // auto-attaches to game process
{
    ps3.WriteInt32(0x00B00000, 0x1337);
    ps3.Disconnect();
}
```

### Vectors

```csharp
// read/write 3D position
Vector3 pos = ps3.ReadVector3(0x00B00000);
ps3.WriteVector3(0x00B00000, new Vector3(100f, 50f, 200f));

// read/write position + heading (quaternion, color with alpha, etc.)
Vector4 orientation = ps3.ReadVector4(0x00B00000);
ps3.WriteVector4(0x00B00000, new Vector4(100f, 50f, 200f, 3.14f));
```

### Structs and Enums

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct PlayerData
{
    public int Health;
    public float PosX;
    public float PosY;
    public byte Team;
}

// fields are automatically big-endian swapped for PS3
var player = ps3.ReadStruct<PlayerData>(0x00B00000);
ps3.WriteStruct(0x00B00000, player);

// enums use the underlying type with proper endian conversion
enum Weapon : int { Pistol = 0, Rifle = 1, Shotgun = 2 }
ps3.WriteEnum(0x00B00000, Weapon.Rifle);
Weapon w = ps3.ReadEnum<Weapon>(0x00B00000);
```

---

## Backends

| Backend | Target | Port | Platform | Status |
|---------|--------|------|----------|--------|
| **RPCS3** | RPCS3 emulator | N/A | Windows, Linux, macOS (x64) | Fully tested |
| **TMAPI** | PS3 DEX | 1000 | Windows, Linux, macOS | Fully tested |
| **PS3MAPI** | PS3 CEX/DEX (webMAN) | 7887 | Windows, Linux, macOS | Fully tested |
| **CCAPI** | PS3 CEX/DEX | 1979 | Windows only (x86) | Fully tested |

---

## What changed in v2.0

- **Dropped PS3Lib.dll** — no more native DLL dependency or ILRepack build step
- **Cross-platform TMAPI** — reverse-engineered the SN Systems debug protocol from Wireshark captures. Believed to be the first open-source cross-platform TMAPI implementation
- **Seamless Target Manager support** — on Windows with ProDG installed, TMAPI automatically routes through the Target Manager DLL so it works alongside the debugger
- **PS3MAPI support** — FTP-like text protocol for webMAN-MOD CEX/DEX consoles
- **CCAPI support** — dynamically loads CCAPI.dll when installed on Windows (x86 only)
- **RPCS3 cross-platform** — Windows (`ReadProcessMemory`), Linux (`pread`/`pwrite`), macOS (`mach_vm_read/write`) with dynamic base address discovery
- **Shared base class** — all typed read/write methods implemented once in `PS3BackendBase`, eliminating duplication
- **New API** — `Vector3`, `Vector4`, `ReadEnum<T>`, `WriteEnum<T>`, `ReadStruct<T>`, `WriteStruct<T>` with automatic big-endian field swapping
- **Automatic chunking** — large reads/writes are split into safe-sized packets transparently
- **Proper IDisposable** — all backends and the client properly clean up resources

---

## Breaking changes from v1.x

- `PS3Type.TMAPI` / `PS3Type.CCAPI` constructors replaced with `BackendType.TMAPI` / `BackendType.CCAPI` + IP address
- PS3Lib is no longer bundled or required

---

## Testing

Unit tests are included (35 tests). Integration tests require hardware (PS3 or RPCS3).

---

## Contributing

Contributions are welcome! Help with reverse engineering the CCAPI network protocol (custom encrypted, port 1979) would enable cross-platform CCAPI support without the Windows DLL.

---

## License

[MIT License](LICENSE)

---

## Contact

Created and maintained by Mack Core.
Feel free to reach out via GitHub or open an issue.
