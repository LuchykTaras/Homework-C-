using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        string filePath = @"D:\WordSearchApp\bicycle.txt";
        string searchWord = "bicycle";

        string childExePath = @"..\Child\bin\Debug\net10.0\Child.exe";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = childExePath,
            Arguments = $"\"{filePath}\" \"{searchWord}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)!;
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(output);
    }
}