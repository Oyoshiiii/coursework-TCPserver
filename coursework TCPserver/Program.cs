using CourseworkGameLib;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

Server server = new Server();
await server.ListenPlayerAsync();

class Server
{
    TcpListener listener = new TcpListener(IPAddress.Any, 8888);
    protected internal List<int> GameCodes = new List<int>() { };
    List<Player> players = new List<Player>();
    protected internal void RemovePlayerConnection()
    {
        players.Clear();
    }
    protected internal async Task ListenPlayerAsync()
    {
        try
        {
            listener.Start();
            Console.WriteLine("Сервер запущен. Ожидается подключение игрока...");

            while(true)
            {
                if (players.Count == 0)
                {
                    TcpClient tcpPlayer = await listener.AcceptTcpClientAsync();
                    Player player = new Player(tcpPlayer, this);
                    Task.Run(player.ProcessAsync);
                }
                else continue;
            }
        }
        catch(Exception ex) { Console.WriteLine("\n\t\t" + ex.Message + "\n"); }
        finally
        {
            Disconnect();
        }
    }
    protected internal async Task SendPlayerMessageAsync()
    {
        foreach(var player in players)
        {
            await player.Writer.WriteLineAsync(GameCodes[GameCodes.Count - 1].ToString());
            await player.Writer.FlushAsync();
        }
    }
    protected internal void Disconnect()
    {
        foreach (var player in players)
        {
            player.Close();
        }
        listener.Stop();
    }
}

class Player
{
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    TcpClient TcpPlayer;
    Server server;

    public Player(TcpClient tcpPlayer, Server server)
    {
        TcpPlayer = tcpPlayer;
        this.server = server;

        var stream = TcpPlayer.GetStream();
        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }

    public async Task ProcessAsync()
    {
        try
        {
            string? code = await Reader.ReadLineAsync();
            if (Convert.ToInt32(code) == 0) 
            { 
                server.GameCodes.Clear(); 
                server.GameCodes.Add(Convert.ToInt32(code)); 
            }

            await server.SendPlayerMessageAsync();

            while (true)
            {
                try
                {
                    code = await Reader.ReadLineAsync();
                    Console.WriteLine($"Последний код автосохранения: {code}");
                    server.GameCodes.Add(Convert.ToInt32(code));
                }
                catch
                {
                    Console.WriteLine("Игрок вышел из игры");
                    break;
                }
            }
        }
        catch(Exception ex) { Console.WriteLine("\n\t\t" + ex.Message + "\n");}
        finally
        {
            server.RemovePlayerConnection();
        }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        TcpPlayer.Close();
    }
}