using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("=== Консольний генератор простих чисел ===");

        // 1. Зчитування нижньої межі (за замовчуванням 2)
        Console.Write("Введіть нижню межу (натисніть Enter для значення 2): ");
        string? inputStart = Console.ReadLine();
        long start = string.IsNullOrWhiteSpace(inputStart) ? 2 : long.Parse(inputStart);
        if (start < 2) start = 2; // Прості числа починаються з 2

        // 2. Зчитування верхньої межі (за замовчуванням нескінченність)
        Console.Write("Введіть верхню межу (натисніть Enter для нескінченної генерації): ");
        string? inputEnd = Console.ReadLine();
        long? end = string.IsNullOrWhiteSpace(inputEnd) ? null : long.Parse(inputEnd);

        Console.WriteLine("\n[Запуск] Генерація розпочата...");
        Console.WriteLine("[Підказка] Натисніть 'Q' або 'Esc', щоб зупинити потік.\n");

        using CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // 3. Запуск фонового потоку генерації
        Task workerTask = Task.Run(() => GeneratePrimes(start, end, token), token);

        // 4. Головний потік слухає консоль для зупинки потоку
        while (!workerTask.IsCompleted)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n[Сигнал] Отримано команду на зупинку...");
                    cts.Cancel(); // Надсилаємо сигнал скасування у фоновий потік
                    break;
                }
            }
            Thread.Sleep(100);
        }

        // Очікуємо коректного завершення потоку
        try
        {
            workerTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
        {
            // Очікуване скасування завдання
        }

        Console.WriteLine("[Завершено] Роботу потоку зупинено.");
    }

    /// <summary>
    /// Метод, який виконується у фоновому потоці.
    /// </summary>
    static void GeneratePrimes(long start, long? end, CancellationToken token)
    {
        long number = start;

        while (!token.IsCancellationRequested)
        {
            // Якщо верхню межу вказано і ми її досягли — виходим з циклу
            if (end.HasValue && number > end.Value)
            {
                break;
            }

            if (IsPrime(number))
            {
                Console.WriteLine(number);
            }

            number++;
        }
    }

    /// <summary>
    /// Перевірка чи є число простим.
    /// </summary>
    static bool IsPrime(long n)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;

        for (long i = 5; i * i <= n; i += 6)
        {
            if (n % i == 0 || n % (i + 2) == 0) return false;
        }

        return true;
    }
}