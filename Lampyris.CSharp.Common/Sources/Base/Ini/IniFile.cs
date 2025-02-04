namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.IO;

public class IniFile
{
    private readonly string m_Path;
    private readonly Dictionary<string, Dictionary<string, string>> m_Data;

    public IniFile(string path)
    {
        m_Path = path;
        m_Data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Load();
    }

    private void Load()
    {
        if (!File.Exists(m_Path))
            return;

        string currentSection = null;
        foreach (var line in File.ReadAllLines(m_Path))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                if (!m_Data.ContainsKey(currentSection))
                {
                    m_Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            else if (currentSection != null)
            {
                var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    m_Data[currentSection][keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
        }
    }

    public void Save()
    {
        using (var writer = new StreamWriter(m_Path))
        {
            foreach (var section in m_Data)
            {
                writer.WriteLine($"[{section.Key}]");
                foreach (var kvp in section.Value)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
                }
                writer.WriteLine();
            }
        }
    }

    public string ReadValue(string section, string key, string defaultValue = "")
    {
        if (m_Data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public void WriteValue(string section, string key, string value)
    {
        if (!m_Data.ContainsKey(section))
        {
            m_Data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        m_Data[section][key] = value;
    }
}
