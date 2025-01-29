using CourseworkGameLib;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

Server server = new Server();
await server.ListenPlayerAsync();

class Server
{
    TcpListener listener = new TcpListener(IPAddress.Any, 8888);
    List<Player> players = new List<Player>();
    
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
    protected internal async Task SendPlayerMessageAsync(string msg) { }
    protected internal void Disconnect()
    {
        foreach (var player in players)
        {
            player.Close();
        }
        players.Clear();
        listener.Stop();
    }
}

class Player
{
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    TcpClient TcpPlayer;
    Server server;

    public Player(TcpClient tcpPlayer, Server server) { }

    public async Task ProcessAsync() { }

    protected internal void Close() { }
}