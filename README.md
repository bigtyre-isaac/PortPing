# PortPing

A small console tool for checking whether a TCP port is reachable. It repeatedly tries to open a connection and prints the result with the time it took.

## Usage

```
portping host:port
```

Or with explicit flags:

```
portping --host example.com --port 443
```

### Options

- `-h`, `--host` — hostname or IP  
- `-p`, `--port` — port number  
- `-t`, `--timeout` — timeout in seconds (default 2)  
- `-i`, `--interval` — seconds between checks (default 2)

Example:

```
portping 192.168.1.10:3306 -t 1 -i 5
```

## What it does

- Tries to connect using `TcpClient`
- Waits for success or timeout
- Prints **Success** or **Failed** with the duration
- Keeps going until you press **Ctrl+C**

Simple port polling with minimal fuss.
