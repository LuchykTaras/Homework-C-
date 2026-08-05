using System;
using System.Threading;

namespace BusStopSimulation
{
    class Program
    {
        // Конфігурація моделі
        private const string BUS_NUMBER = "175";
        private const int BUS_CAPACITY = 25;       // Максимальна місткість автобуса
        private const int TOTAL_BUSES = 6;         // Фіксована кількість автобусів на день
        private const int INTERVAL_MS = 2000;      // Інтервал між прибуттям автобусів / пасажирів (мс)

        // Спільні ресурси
        private static int peopleAtStop = 0;       // Поточна кількість людей на зупинці
        private static readonly object lockObj = new object(); // Об'єкт для блокування (критична секція)

        // Події синхронізації
        private static AutoResetEvent busArrivedEvent = new AutoResetEvent(false); // Сповіщає про прибуття автобуса
        private static bool isDayFinished = false; // Прапорець завершення дня

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"=== СИМУЛЯЦІЯ РОБОТИ КІНЦЕВОЇ ЗУПИНКИ (Маршрут №{BUS_NUMBER}) ===");
            Console.WriteLine($"Місткість автобуса: {BUS_CAPACITY} чол. Заплановано рейсів: {TOTAL_BUSES}\n");

            // Запускаємо потік пасажиропотоку (люди прибувають на зупинку)
            Thread passengerThread = new Thread(GeneratePassengers);
            // Запускаємо потік руху автобусів
            Thread busThread = new Thread(ManageBuses);

            passengerThread.Start();
            busThread.Start();

            // Чекаємо завершення потоку автобусів
            busThread.Join();

            // Завершуємо день для пасажирів
            isDayFinished = true;
            passengerThread.Join();

            Console.WriteLine("\n=== РАБОЧИЙ ДЕНЬ ЗАКІНЧЕНО ===");
            Console.WriteLine($"Людей, що залишилися на зупинці: {peopleAtStop}");
        }

        /// <summary>
        /// Потік 1: Постійне прибуття випадкової кількості людей на зупинку.
        /// </summary>
        private static void GeneratePassengers()
        {
            Random random = new Random();

            while (!isDayFinished)
            {
                Thread.Sleep(random.Next(800, 1500)); // Людина/група приходить раз у 0.8-1.5 сек

                int newPeople = random.Next(1, 10); // Від 1 до 9 людей

                lock (lockObj)
                {
                    peopleAtStop += newPeople;
                    Console.WriteLine($"[ПАСАЖИРИ] Прибуло {newPeople} чол. На зупинці всього: {peopleAtStop} чол.");
                }
            }
        }

        /// <summary>
        /// Потік 2: Рух і посадка автобусів за графіком.
        /// </summary>
        private static void ManageBuses()
        {
            for (int i = 1; i <= TOTAL_BUSES; i++)
            {
                // Затримка до прибуття наступного автобуса
                Thread.Sleep(INTERVAL_MS);

                Console.WriteLine($"\n------------------------------------------------");
                Console.WriteLine($"[АВТОБУС №{BUS_NUMBER}] Автобус #{i} під'їхав на кінцеву зупинку.");

                // Сигналізуємо про подвю: Автобус на зупинці
                busArrivedEvent.Set();

                // Обробка посадки пасажирів
                BoardPassengers(i);
            }
        }

        /// <summary>
        /// Логіка посадки людей в автобус з урахуванням місткості.
        /// </summary>
        private static void BoardPassengers(int busIndex)
        {
            lock (lockObj)
            {
                if (peopleAtStop == 0)
                {
                    Console.WriteLine($"[ПОСАДКА] На зупинці немає людей. Автобус #{busIndex} поїхав порожнім (0/{BUS_CAPACITY}).");
                }
                else
                {
                    // Розраховуємо, скільки людей може сісти
                    int passengersToBoard = Math.Min(peopleAtStop, BUS_CAPACITY);

                    peopleAtStop -= passengersToBoard;

                    Console.WriteLine($"[ПОСАДКА] В автобус #{busIndex} зайшло: {passengersToBoard} чол.");
                    Console.WriteLine($"[ПОСАДКА] Заповненість: {passengersToBoard}/{BUS_CAPACITY}. На зупинці залишилось: {peopleAtStop} чол.");
                }
            }
        }
    }
}