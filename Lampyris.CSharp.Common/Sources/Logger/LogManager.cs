namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public class LogService
{
    private readonly List<ILogger> m_LoggerList = new List<ILogger>();
    
    // 以下是格式化输出日志的时候，Log类型的缩写
    private const string InfoString    = "INFO";
    private const string WarningString = "WARN";
    private const string ErrorString   = "ERROR";

    public void AddLogger(ILogger logger)
    {
        m_LoggerList.Add(logger);
    }

    public void LogInfo(string message)
    {
        Log(InfoString, message);
    }

    public void LogWarning(string message)
    {
        Log(WarningString, message);
    }

    public void LogError(string message)
    {
        Log(ErrorString, message);
    }

    private void Log(string level, string message)
    {
        string formattedMessage = FormatMessage(level, message);
        foreach (var logger in m_LoggerList)
        {
            logger.Log(formattedMessage);
        }
    }

    private string FormatMessage(string level, string message)
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
