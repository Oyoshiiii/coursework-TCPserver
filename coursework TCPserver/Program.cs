using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;


class Server
{
    static List<int> GameCodes = new List<int>() { };

    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Сервер запущен. Ожидается подключение игрока...");

        while (true)
        {
            TcpClient player = server.AcceptTcpClient();
            Console.WriteLine("Игрок подключился");
            NetworkStream stream = player.GetStream();

            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);

            int code = Convert.ToInt32(Encoding.UTF8.GetString(data, 0, bytesRead));

            if (code == 0) { NewGame(); }
            else if (code == -1) { ContGame(stream); }
            else { SaveLastAutosaveCode(code); }

            Console.WriteLine($"Игрок вышел из игры с последним сохранением: {GameCodes[GameCodes.Count - 1]}\n\n");
            player.Close();
        }
    }

    static void NewGame()
    {
        GameCodes.Clear();
        GameCodes.Add(0);
        Console.WriteLine("\t\tИгрок начал новую игру\n");
    }

    static void ContGame(NetworkStream stream)
    {
        if (GameCodes.Count > 0)
        {
            byte[] lastCode = Encoding.UTF8.GetBytes(GameCodes[GameCodes.Count - 1].ToString());
            stream.Write(lastCode, 0, lastCode.Length);
        }
        else
        {
            byte[] noLastCode = Encoding.UTF8.GetBytes("0");
            stream.Write(noLastCode, 0, noLastCode.Length);
        }
    }

    static void SaveLastAutosaveCode(int code)
    {
        GameCodes.Add(code);
    }
}