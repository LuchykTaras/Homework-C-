using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CasinoSimulation
{
    class Program
    {
        // Семафор, який дозволяє лише 5 потокам (гравцям) одночасно бути за столом
        static readonly Semaphore tableSeats = new Semaphore(5, 5);

        // М'ютекс для синхронізації доступу до генератора чисел та списку звітів
        static readonly Mutex casinoMutex = new Mutex();

        static readonly Random rand = new Random();
        static readonly List<string> dailyReport = new List<string>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Визначаємо загальну кількість гравців на день (від 20 до 100)
            int totalPlayers = rand.Next(20, 101);
            Console.WriteLine($"=== Казино відкривається! Очікується гравців сьогодні: {totalPlayers} ===\n");

            Thread[] playerThreads = new Thread[totalPlayers];

            // Створюємо та запускаємо потоки (гравців)
            for (int i = 0; i < totalPlayers; i++)
            {
                playerThreads[i] = new Thread(PlayAtCasino);
                playerThreads[i].Name = $"Гравець{i + 1}";
                playerThreads[i].Start(i + 1); // Передаємо ID гравця
            }

            // Чекаємо, поки всі потенційні гравці зіграють (завершать свої потоки)
            foreach (Thread t in playerThreads)
            {
                t.Join();
            }

            // Формуємо та зберігаємо звіт
            string reportPath = "casino_report.txt";
            File.WriteAllLines(reportPath, dailyReport);

            Console.WriteLine($"\n=== День завершено. Казино зачиняється. ===");
            Console.WriteLine($"Всі {totalPlayers} гравців відвідали стіл. Звіт збережено у файл: {reportPath}");
        }

        // object? виправляє warning CS8622
        static void PlayAtCasino(object? playerIdObj)
        {
            if (playerIdObj == null) return;

            int playerId = (int)playerIdObj;
            // string? та оператор '??' виправляють warning CS8600
            string playerName = Thread.CurrentThread.Name ?? $"Гравець_{playerId}";

            // Використовуємо Mutex для безпечної генерації початкових даних
            casinoMutex.WaitOne();
            int initialBalance = rand.Next(500, 5001); // Початковий баланс від 500 до 5000
            // Випадкова кількість раундів, які гравець планує зіграти (щоб черга рухалась)
            int roundsToPlay = rand.Next(3, 15);
            casinoMutex.ReleaseMutex();

            // Гравець чекає, поки звільниться місце за столом
            tableSeats.WaitOne();

            casinoMutex.WaitOne();
            Console.WriteLine($"{playerName} сів за стіл. Баланс: {initialBalance}");
            casinoMutex.ReleaseMutex();

            int currentBalance = initialBalance;

            // Цикл гри (гравець грає заплановану кількість раундів або поки не закінчаться гроші)
            for (int round = 0; round < roundsToPlay; round++)
            {
                // Якщо гроші закінчилися, гравець звільняє стіл
                if (currentBalance <= 0)
                {
                    casinoMutex.WaitOne();
                    Console.WriteLine($"[{playerName}] програв усе і засмучений залишає стіл.");
                    casinoMutex.ReleaseMutex();
                    break;
                }

                casinoMutex.WaitOne();
                // Робимо ставку: від 1 до всього поточного балансу
                int betAmount = rand.Next(1, currentBalance + 1);
                int chosenNumber = rand.Next(0, 37); // Числа на рулетці 0-36
                int rouletteResult = rand.Next(0, 37); // Випадання кульки

                if (chosenNumber == rouletteResult)
                {
                    // Ставка зіграла — сума подвоюється (чистий прибуток +betAmount)
                    currentBalance += betAmount;
                }
                else
                {
                    // Ставка не зіграла — гравець втрачає поставлену суму
                    currentBalance -= betAmount;
                }
                casinoMutex.ReleaseMutex();

                // Імітація часу на обертання рулетки та роздуми гравця
                Thread.Sleep(100);
            }

            // Зберігаємо результати у загальний звіт (вимагає Mutex)
            casinoMutex.WaitOne();
            dailyReport.Add($"{playerName} [{initialBalance}] [{currentBalance}]");
            Console.WriteLine($"{playerName} звільняє стіл. Кінцевий баланс: {currentBalance}");
            casinoMutex.ReleaseMutex();

            // Звільняємо місце за столом для наступного потоку (гравця)
            tableSeats.Release();
        }
    }
}