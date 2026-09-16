using System;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    // Черга для подій
    static ConcurrentQueue<string> eventQueue = new ConcurrentQueue<string>();

    // Оцінка кількості задач, які були витягнуті з черги та відправлені на обробку
    static CountdownEvent countdownEvent = new CountdownEvent(1);

    // Лічильники та прапорці
    static int eventCounter = 0;
    static int processedCounter = 0;
    static volatile bool isGenerating = true;

    // Рівні важливості подій
    static readonly string[] Severities = { "LOW", "MEDIUM", "HIGH", "CRITICAL" };
    static readonly Random random = new Random();

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== SIEM System Initialized ===\n");

        // 1. ⏱️ Генератор подій (кожні 600 мс)
        using Timer timer = new Timer(GenerateEvent, null, 0, 600);

        // 2. ⚙️ Запуск окремого фонового потоку для диспетчеризації подій з черги
        Thread processorThread = new Thread(ProcessQueue)
        {
            IsBackground = true
        };
        processorThread.Start();

        // 3. Через 5 секунд зупиняємо генерацію
        Thread.Sleep(5000);
        isGenerating = false;
        timer.Change(Timeout.Infinite, Timeout.Infinite); // Зупиняємо таймер
        Console.WriteLine("\n🚨 [Генератор зупинено] Очікуємо обробки останніх подій...\n");

        // Сигналізуємо, що нових задач від генератора більше не буде
        countdownEvent.Signal();

        // Дочікуємося обробки всіх подій у черзі
        countdownEvent.Wait();

        // Підсумковий вивід
        Console.WriteLine($"\n📊 Всього оброблено подій: {processedCounter}");
    }

    /// <summary>
    /// Метод таймера: генерує нову подібну подій кожні 600 мс.
    /// </summary>
    static void GenerateEvent(object? state)
    {
        if (!isGenerating) return;

        int currentNumber = Interlocked.Increment(ref eventCounter);
        string severity = Severities[random.Next(Severities.Length)];
        string eventMsg = $"Event #{currentNumber} [{severity}]";

        // Додаємо задачу до лічильника CountdownEvent (збільшуємо на 1)
        countdownEvent.AddCount();

        // Кладемо подію в потокобезпечну чергу
        eventQueue.Enqueue(eventMsg);
        Console.WriteLine($"[+] Згенеровано: {eventMsg}");
    }

    /// <summary>
    /// Фоновий потік-диспетчер: витягує події з черги та передає їх у ThreadPool.
    /// </summary>
    static void ProcessQueue()
    {
        while (isGenerating || !eventQueue.IsEmpty)
        {
            if (eventQueue.TryDequeue(out string? eventMsg))
            {
                // Відправляємо обробку у ThreadPool
                ThreadPool.QueueUserWorkItem(HandleEvent, eventMsg);
            }
            else
            {
                // Маленький відпочинок, щоб не завантажувати процесор, коли черга тимчасово порожня
                Thread.Sleep(50);
            }
        }
    }

    /// <summary>
    /// Метод обробки конкретної події у ThreadPool.
    /// </summary>
    static void HandleEvent(object? state)
    {
        string? eventMsg = state as string;

        Console.WriteLine($"  ⚡ [Alert] Обробляю: {eventMsg}");

        // Імітація обробки (400 мс)
        Thread.Sleep(400);

        // Інкрементуємо лічильник оброблених подій
        Interlocked.Increment(ref processedCounter);

        // Повідомляємо CountdownEvent про завершення обробки цієї задачі
        countdownEvent.Signal();
    }
}