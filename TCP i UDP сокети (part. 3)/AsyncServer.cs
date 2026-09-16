using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class AsyncServer
{
    static void Main()
    {
        UdpClient listener = new UdpClient(9000);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 0);

        Console.WriteLine("[UDP Game] Сервер запущено...");

        while (true)
        {
            byte[] bytes = listener.Receive(ref groupEP);
            string message = Encoding.UTF8.GetString(bytes);

            // Припускаємо, що дані розділені крапкою з комою (наприклад: "Player1;move;10,20,30")
            string[] parts = message.Split(';');

            if (parts.Length >= 3)
            {
                Console.WriteLine($"[UDP Game] player {parts[0]} moved to ({parts[2]})");
            }
        }
    }
}