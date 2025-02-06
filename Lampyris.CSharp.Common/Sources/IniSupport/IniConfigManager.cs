namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class IniConfigManager
{
    private static Dictionary<Type, object> ms_Type2ConfigObjectMap = new();

    // 获取配置对象
    public static T GetConfig<T>() where T : new()
    {
        var type = typeof(T);

        if (ms_Type2ConfigObjectMap.ContainsKey(type))
        {
            return (T)ms_Type2ConfigObjectMap[type];
        }

        // 获取类型上的 [IniFile] 属性
        var iniFileAttribute = type.GetCustomAttribute<IniFileAttribute>();
        if (iniFileAttribute == null)
        {
            Logger.LogWarning($"Class {type.Name} doesn't have an [IniFile] attribute.");
            return default(T);
        }

        string fileName = iniFileAttribute.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Logger.LogWarning($"[IniFile] attribute must specify a valid file name.");
            return default(T);
        }

        // 如果文件不存在，创建默认文件
        if (!File.Exists(fileName))
        {
            SaveConfig(new T());
        }

        // 加载 INI 文件
        var config = new T();
        var iniData = File.ReadAllLines(fileName);
        var currentSection = string.Empty;
        var sectionData = new Dictionary<string, Dictionary<string, string>>();

        foreach (var line in iniData)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
            {
                continue; // 忽略空行和注释
            }

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Trim('[', ']');
                if (!sectionData.ContainsKey(currentSection))
                {
                    sectionData[currentSection] = new Dictionary<string, string>();
                }
            }
            else if (!string.IsNullOrEmpty(currentSection))
            {
                var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    sectionData[currentSection][key] = value;
                }
            }
        }

        // 映射到对象
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            var sectionAttribute = member.GetCustomAttribute<SectionAttribute>();
            if (sectionAttribute == null) continue;

            var sectionName = sectionAttribute.Name;
            if (sectionData.TryGetValue(type.Name, out var section) && section.TryGetValue(sectionName, out var value))
            {
                if (member is FieldInfo field)
                {
                    field.SetValue(config, Convert.ChangeType(value, field.FieldType));
                }
                else if (member is PropertyInfo property && property.CanWrite)
                {
                    property.SetValue(config, Convert.ChangeType(value, property.PropertyType));
                }
            }
        }

        ms_Type2ConfigObjectMap[type] = config;
        return config;
    }

    // 保存配置对象到 INI 文件
    public static void SaveConfig<T>(T config) where T : new()
    {
        var type = typeof(T);
        var iniFileAttribute = type.GetCustomAttribute<IniFileAttribute>();
        if (iniFileAttribute == null)
        {
            throw new InvalidOperationException($"Class {type.Name} must have an [IniFile] attribute.");
        }

        string fileName = iniFileAttribute.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException($"[IniFile] attribute must specify a valid file name.");
        }

        var iniData = new Dictionary<string, Dictionary<string, string>>();

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            var sectionAttribute = member.GetCustomAttribute<SectionAttribute>();
            if (sectionAttribute == null) continue;

            var sectionName = sectionAttribute.Name;
            var value = member is FieldInfo field
                ? field.GetValue(config)?.ToString()
                : member is PropertyInfo property ? property.GetValue(config)?.ToString() : null;

            if (!iniData.ContainsKey(type.Name))
            {
                iniData[type.Name] = new Dictionary<string, string>();
            }

            iniData[type.Name][sectionName] = value ?? string.Empty;
        }

        using (var writer = new StreamWriter(fileName))
        {
            foreach (var section in iniData)
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
}
