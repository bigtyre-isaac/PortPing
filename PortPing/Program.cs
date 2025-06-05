using System.Diagnostics;
using System.Net.Sockets;

var (host, port, timeout, interval) = ParseArgs(args);


using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Stopping...");
};
Console.WriteLine($"Starting TCP port connection test");
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.White;
Console.Write($"Polling ");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write($"{host}");
Console.ForegroundColor = ConsoleColor.White;
Console.Write($" on port ");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.Write($"{port}");
Console.ForegroundColor = ConsoleColor.White;
Console.Write($" every ");
Console.ForegroundColor = ConsoleColor.White;
Console.Write($"{interval.TotalSeconds}");
Console.ForegroundColor = ConsoleColor.White;
Console.Write($" sec");
Console.WriteLine();
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.White;

while (!cts.IsCancellationRequested)
{
    var watch = Stopwatch.StartNew();

    var colour = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
    Console.Write($"Connecting to ");
        Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"{host}");
    Console.Write($":");
    Console.Write($"{port}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($" ...... ");
    Console.ForegroundColor = colour;


    var success = await TryConnectAsync(host, port, timeout);
    watch.Stop();

    var elapsed = watch.Elapsed;

    try 
    { 
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Success");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Failed");
        }
    }
    finally 
    { 
        Console.ForegroundColor = colour;
        Console.Write(" in ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write((int)elapsed.TotalMilliseconds);
        Console.ForegroundColor = colour;
        Console.WriteLine($" ms");
    }

    try
    {
        await Task.Delay(interval, cts.Token);
    }
    catch (TaskCanceledException)
    {
        break;
    }
}

static async Task<bool> TryConnectAsync(string host, int port, TimeSpan timeout)
{
    try
    {
        using var client = new TcpClient();
        var connectTask = client.ConnectAsync(host, port);
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(connectTask, timeoutTask);

        return completedTask == connectTask && client.Connected;
    }
    catch
    {
        return false;
    }
}

static (string host, int port, TimeSpan timeout, TimeSpan interval) ParseArgs(string[] args)
{
    string? host = null;
    int port = 0;
    int timeout = 2000;
    int interval = 2000;

    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i].ToLowerInvariant();

        if ((arg == "-t" || arg == "--timeout") && i + 1 < args.Length)
        {
            timeout = int.Parse(args[++i]) * 1000;
        }
        else if ((arg == "-i" || arg == "--interval") && i + 1 < args.Length)
        {
            interval = int.Parse(args[++i]) * 1000;
        }
        else if ((arg == "-p" || arg == "--port") && i + 1 < args.Length)
        {
            port = int.Parse(args[++i]);
        }
        else if ((arg == "-h" || arg == "--host") && i + 1 < args.Length)
        {
            host = args[++i];
        }
        else if (arg.Contains(':') && i == args.Length - 1)
        {
            var parts = arg.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var parsedPort))
            {
                host ??= parts[0];
                port = parsedPort;
            }
            else
            {
                throw new ArgumentException($"Invalid HOST:PORT argument: {arg}");
            }
        }
        else
        {
            throw new ArgumentException($"Unrecognized or malformed argument: {arg}");
        }
    }

    if (host == null || port == 0)
        throw new ArgumentException("Host and port must be specified.");

    return (host, port, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(interval));
}
