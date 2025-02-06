namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class Logger
{
    private static readonly List<ILogger> ms_LoggerList = new List<ILogger>();
    
    // 以下是格式化输出日志的时候，Log类型的缩写
    private const string InfoString    = "INFO";
    private const string WarningString = "WARN";
    private const string ErrorString   = "ERROR";

    public static void AddLogger(ILogger logger)
    {
        ms_LoggerList.Add(logger);
    }

    public static void LogInfo(string message)
    {
        Log(InfoString, message);
    }

    public static void LogWarning(string message)
    {
        Log(WarningString, message);
    }

    public static void LogError(string message)
    {
        Log(ErrorString, message);
    }

    private static void Log(string level, string message)
    {
        string formattedMessage = FormatMessage(level, message);
        foreach (var logger in ms_LoggerList)
        {
            logger.Log(formattedMessage);
        }
    }

    private static string FormatMessage(string level, string message)
    {
        var    stackFrame = new StackTrace(3, true).GetFrame(0);
        string timestamp  = DateTime.Now.ToString("HH:mm:ss");

        if (stackFrame != null)
        {
            var methodInfo = stackFrame.GetMethod();
            if (methodInfo != null && methodInfo.DeclaringType != null)
            {
                string callingClass  = methodInfo.DeclaringType.Name;
                string callingMethod = methodInfo.Name;
                
                return $"[{timestamp}][{level}][{callingClass}::{callingMethod}] {message}";
            }
        }
        return $"[{timestamp}][{level}] {message}";
    }
}
