# PS3Sharp

PS3Sharp is a .NET library for reading and writing memory directly from **PlayStation 3 (PS3)** consoles and the **RPCS3** emulator. It provides easy-to-use backends to interact with PS3 memory, making it ideal for tooling, debugging, or modding projects targeting PS3 and RPCS3.

The PS3 backend is built on top of [PS3Lib](https://github.com/iMCSx/PS3Lib), a popular library for PS3 communication.

---

## Features

- Direct memory access for PS3 and RPCS3 processes
- Support for reading and writing multiple data types (ints, floats, bytes, etc.)
- Switchable backend between PS3 console (using PS3Lib) and RPCS3 emulator
- Simple API designed with extensibility in mind
- Unit tested for reliability

---

## Installation

Currently, PS3Sharp is not published as a NuGet package. To use it, clone the repository and include the project in your solution.

---

## Usage

### RPCS3

```csharp
using PS3Sharp;

var rpcs3 = new PS3Client("rpcs3"); // optional: pass custom window title

// connect to the backend
if (rpcs3.Connect())
{
    Console.WriteLine($"Connected to: {rpcs3.ActiveBackendType}");

    // read an integer from memory
    uint address = 0xC0000000;
    int value = rpcs3.ReadInt32(address);
    Console.WriteLine($"Value at {address:X}: {value}");

    // write an integer to memory
    rpcs3.WriteInt32(address, 12345);
    rpcs3.Disconnect();
}

else
    Console.WriteLine("Failed to connect.");
```

### PS3 (TMAPI / CCAPI)

```csharp
using PS3Sharp;
using PS3Sharp.Types;

var ps3 = new PS3Client(PS3Type.TMAPI); // or PS3Type.CCAPI

// connect to the backend
if (ps3.Connect())
{
    Console.WriteLine($"Connected to: {ps3.ActiveBackendType}");

    // read an integer from memory
    uint address = 0xC0000000;
    int value = ps3.ReadInt32(address);
    Console.WriteLine($"Value at {address:X}: {value}");

    // write an integer to memory
    ps3.WriteInt32(address, 12345);
    ps3.Disconnect();
}

else
    Console.WriteLine("Failed to connect.");
```

---

## Backends

- **PS3Backend**   - Connects to a physical PS3 console using PS3Lib.
- **RPCS3Backend** - Connects to the RPCS3 emulator process.

---

## Testing

Unit tests are included for both backends to verify read/write functionality and connection management.

---

## Contributing

Contributions are welcome!

---

## License

[MIT License](LICENSE)

---

## Contact

Created and maintained by Mack Core.  
Feel free to reach out via GitHub or open an issue.