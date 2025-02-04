namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public class LogManager:Singleton<LogManager>
{
    private readonly List<ILogger> m_LoggerList = new List<ILogger>();
    
    // 以下是格式化输出日志的时候，Log类型的缩写
    private const string c_INFO    = "INFO";
    private const string c_WARNING = "WARN";
    private const string c_ERROR   = "ERROR";

    public void AddLogger(ILogger logger)
    {
        m_LoggerList.Add(logger);
    }

    public void LogInfo(string message)
    {
        Log(c_INFO, message);
    }

    public void LogWarning(string message)
    {
        Log(c_WARNING, message);
    }

    public void LogError(string message)
    {
        Log(c_ERROR, message);
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
