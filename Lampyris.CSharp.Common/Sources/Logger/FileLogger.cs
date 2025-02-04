namespace Lampyris.CSharp.Common;

using System;
using System.IO;

public class FileLogger : ILogger
{
    private readonly string filePath;

    public FileLogger(string filePath)
    {
        this.filePath = filePath;
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log to file: {ex.Message}");
        }
    }
}