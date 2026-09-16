using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class AsyncServer
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
        listener.Start();
        Console.WriteLine("[Server] Запущено на 127.0.0.1:8888");

        listener.BeginAcceptTcpClient(OnAccept, listener);

        Console.WriteLine("Натисніть Enter для завершення роботи сервера...");
        Console.ReadLine();
    }

    static void OnAccept(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        TcpClient client;

        try
        {
            client = listener.EndAcceptTcpClient(ar);
        }
        catch (ObjectDisposedException)
        {
            return; // listener зупинено
        }

        Console.WriteLine("[Server] Новий клієнт підключився: " + client.Client.RemoteEndPoint);

        // Одразу реєструємо наступний BeginAccept, щоб приймати інших клієнтів
        listener.BeginAcceptTcpClient(OnAccept, listener);

        // Починаємо асинхронне читання від щойно підключеного клієнта
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        var state = new ClientState
        {
            Client = client,
            Stream = stream,
            Buffer = buffer
        };

        stream.BeginRead(buffer, 0, buffer.Length, OnRead, state);
    }

    static void OnRead(IAsyncResult ar)
    {
        var state = (ClientState)ar.AsyncState;
        NetworkStream stream = state.Stream;

        int bytesRead;
        try
        {
            bytesRead = stream.EndRead(ar);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Server] Помилка читання: " + ex.Message);
            state.Client.Close();
            return;
        }

        if (bytesRead == 0)
        {
            // Клієнт закрив з'єднання
            Console.WriteLine("[Server] Клієнт відключився: " + state.Client.Client.RemoteEndPoint);
            state.Client.Close();
            return;
        }

        string message = Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
        Console.WriteLine("[Server] Прийнято повідомлення: " + message);

        string response = "[Server] Отримано: " + message;
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);

        stream.BeginWrite(responseBytes, 0, responseBytes.Length, OnWrite, state);
    }

    static void OnWrite(IAsyncResult ar)
    {
        var state = (ClientState)ar.AsyncState;
        try
        {
            state.Stream.EndWrite(ar);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Server] Помилка запису: " + ex.Message);
            state.Client.Close();
            return;
        }

        // Продовжуємо слухати наступні повідомлення від цього ж клієнта
        state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, OnRead, state);
    }

    class ClientState
    {
        public TcpClient Client;
        public NetworkStream Stream;
        public byte[] Buffer;
    }
}