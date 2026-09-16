using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class AsyncClient
{
    static ManualResetEvent connectDone = new ManualResetEvent(false);
    static ManualResetEvent sendDone = new ManualResetEvent(false);
    static ManualResetEvent receiveDone = new ManualResetEvent(false);

    static void Main(string[] args)
    {
        string clientNumber = args.Length > 0 ? args[0] : "0";
        string messageToSend = "Клієнт " + clientNumber;

        TcpClient client = new TcpClient();

        Console.WriteLine("[Client " + clientNumber + "] Підключення до сервера...");

        client.BeginConnect("127.0.0.1", 8888, OnConnect, client);
        connectDone.WaitOne();

        NetworkStream stream = client.GetStream();

        byte[] data = Encoding.UTF8.GetBytes(messageToSend);
        Console.WriteLine("[Client " + clientNumber + "] Надсилання: " + messageToSend);
        stream.BeginWrite(data, 0, data.Length, OnSend, stream);
        sendDone.WaitOne();

        byte[] buffer = new byte[1024];
        var state = new ReceiveState { Stream = stream, Buffer = buffer };
        stream.BeginRead(buffer, 0, buffer.Length, OnReceive, state);
        receiveDone.WaitOne();

        client.Close();

        Console.WriteLine("[Client " + clientNumber + "] Завершено. Натисніть Enter...");
        Console.ReadLine();
    }

    static void OnConnect(IAsyncResult ar)
    {
        TcpClient client = (TcpClient)ar.AsyncState;
        client.EndConnect(ar);
        Console.WriteLine("[Client] Підключено до сервера.");
        connectDone.Set();
    }

    static void OnSend(IAsyncResult ar)
    {
        NetworkStream stream = (NetworkStream)ar.AsyncState;
        stream.EndWrite(ar);
        sendDone.Set();
    }

    static void OnReceive(IAsyncResult ar)
    {
        var state = (ReceiveState)ar.AsyncState;
        int bytesRead = state.Stream.EndRead(ar);

        if (bytesRead > 0)
        {
            string response = Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
            Console.WriteLine("[Client] Відповідь сервера: " + response);
        }

        receiveDone.Set();
    }

    class ReceiveState
    {
        public NetworkStream Stream;
        public byte[] Buffer;
    }
}