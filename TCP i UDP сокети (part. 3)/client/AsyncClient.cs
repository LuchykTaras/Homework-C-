using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpGameClient
{
    static void Main(string[] args)
    {
        // Створюємо UDP клієнт
        UdpClient client = new UdpClient();
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);

        // Формуємо повідомлення у форматі, де частини розділені крапкою з комою (;):
        // Наприклад: Ім'я гравця ; Дія ; Координати/Позиція
        string message = "Player1;move;10,20,30";

        byte[] data = Encoding.UTF8.GetBytes(message);

        // Відправляємо дані на сервер
        client.Send(data, data.Length, serverEndPoint);
        Console.WriteLine($"[UDP Client] Надіслано на сервер: {message}");

        // Закриваємо клієнт
        client.Close();
    }
}