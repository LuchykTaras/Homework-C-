using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class StateObject
{
    public Socket Socket = null!;
    public byte[] Buffer = new byte[1024];
}

class AsyncServer
{
    static readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);
    static readonly ManualResetEvent Stop = new ManualResetEvent(false);

    static void Main()
    {
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(EndPoint);
        listener.Listen(100);

        Console.WriteLine($"[Server] Слухаю на {EndPoint}...");
        listener.BeginAccept(AcceptCallback, listener);

        Console.WriteLine("[Server] Натисніть Ctrl+C для завершення.");
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; Stop.Set(); };
        Stop.WaitOne();
        listener.Close();
    }

    static void AcceptCallback(IAsyncResult ar)
    {
        Socket listener = (Socket)ar.AsyncState!;
        Socket handler;
        try
        {
            handler = listener.EndAccept(ar);
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        listener.BeginAccept(AcceptCallback, listener);

        Console.WriteLine($"[Server] Підключено клієнта {handler.RemoteEndPoint}");
        StateObject state = new StateObject { Socket = handler };
        handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
    }

    static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState!;
        Socket handler = state.Socket;

        int received;
        try
        {
            received = handler.EndReceive(ar);
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[Server] Помилка читання: {ex.Message}");
            handler.Close();
            return;
        }

        if (received == 0)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            return;
        }

        string message = Encoding.UTF8.GetString(state.Buffer, 0, received);
        Console.WriteLine($"[Server] Отримано від {handler.RemoteEndPoint}: {message}");

        byte[] response = Encoding.UTF8.GetBytes($"[Server] Отримано: {message}");
        handler.BeginSend(response, 0, response.Length, SocketFlags.None, SendCallback, state);
    }

    static void SendCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState!;
        Socket handler = state.Socket;
        handler.EndSend(ar);

        handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
    }
}