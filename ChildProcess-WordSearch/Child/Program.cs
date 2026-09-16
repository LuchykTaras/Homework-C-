using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Потрібно 2 аргументи: шлях до файлу та слово для пошуку.");
            return 1;
        }

        string filePath = args[0];
        string searchWord = args[1];

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Файл '{filePath}' не знайдено.");
            return 1;
        }

        string text = File.ReadAllText(filePath);

        string[] words = text.Split(
            new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '"', '(', ')', '-' },
            StringSplitOptions.RemoveEmptyEntries);

        int count = 0;
        foreach (string word in words)
        {
            if (string.Equals(word, searchWord, StringComparison.OrdinalIgnoreCase))
                count++;
        }

        Console.WriteLine($"Слово '{searchWord}' зустрічається {count} раз(ів).");
        return 0;
    }
}
