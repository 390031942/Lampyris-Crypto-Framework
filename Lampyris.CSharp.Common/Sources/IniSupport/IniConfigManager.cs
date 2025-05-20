namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class IniConfigManager
{
    /// <summary>
    /// 从 INI 文件加载配置到对象
    /// </summary>
    /// <typeparam name="T">目标对象的类型</typeparam>
    /// <returns>加载后的对象</returns>
    public static T Load<T>() where T : new()
    {
        // 获取类上的 IniFileAttribute
        var type = typeof(T);
        var iniFileAttribute = type.GetCustomAttribute<IniFileAttribute>();
        if (iniFileAttribute == null)
        {
            throw new InvalidOperationException($"Class {type.Name} does not have IniFileAttribute.");
        }

        string fileName = "cfgs/" + iniFileAttribute.FileName;
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"INI file '{fileName}' not found.");
        }

        // 读取 INI 文件内容
        var iniData = File.ReadAllLines(fileName);

        // 创建目标对象
        var instance = new T();

        // 遍历类的字段和属性
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            var iniFieldAttribute = member.GetCustomAttribute<IniFieldAttribute>();
            if (iniFieldAttribute == null) continue;

            string section = iniFieldAttribute.Section;
            string name = string.IsNullOrEmpty(iniFieldAttribute.Name) ? member.Name : iniFieldAttribute.Name;

            // 查找 INI 文件中的对应值
            string value = GetIniValue(iniData, section, name);
            if (value == null) continue;

            // 设置值到对象的字段或属性
            if (member is PropertyInfo property && property.CanWrite)
            {
                property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(instance, Convert.ChangeType(value, field.FieldType));
            }
        }

        return instance;
    }

    /// <summary>
    /// 保存对象到 INI 文件
    /// </summary>
    /// <typeparam name="T">目标对象的类型</typeparam>
    /// <param name="obj">要保存的对象</param>
    public static void Save<T>(T obj)
    {
        // 获取类上的 IniFileAttribute
        var type = typeof(T);
        var iniFileAttribute = type.GetCustomAttribute<IniFileAttribute>();
        if (iniFileAttribute == null)
        {
            throw new InvalidOperationException($"Class {type.Name} does not have IniFileAttribute.");
        }

        string fileName = "cfgs" + iniFileAttribute.FileName;

        // 用于存储 INI 文件内容
        var iniData = new Dictionary<string, List<(string Key, string Value, string Desc)>>();

        // 遍历类的字段和属性
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            var iniFieldAttribute = member.GetCustomAttribute<IniFieldAttribute>();
            if (iniFieldAttribute == null) continue;

            string section = iniFieldAttribute.Section;
            string name = iniFieldAttribute.Name;
            string desc = iniFieldAttribute.Desc; // 获取描述信息
            string value = null;

            // 获取字段或属性的值
            if (member is PropertyInfo property && property.CanRead)
            {
                value = property.GetValue(obj)?.ToString();
            }
            else if (member is FieldInfo field)
            {
                value = field.GetValue(obj)?.ToString();
            }

            if (value == null) continue;

            // 添加到 INI 数据结构
            if (!iniData.ContainsKey(section))
            {
                iniData[section] = new List<(string Key, string Value, string Desc)>();
            }
            iniData[section].Add((name, value, desc));
        }

        // 写入 INI 文件
        using (var writer = new StreamWriter(fileName))
        {
            foreach (var section in iniData)
            {
                writer.WriteLine($"[{section.Key}]"); // 写入节名
                foreach (var (key, value, desc) in section.Value)
                {
                    if (!string.IsNullOrEmpty(desc))
                    {
                        writer.WriteLine($"; {desc}"); // 写入注释
                    }
                    writer.WriteLine($"{key}={value}"); // 写入键值对
                }
                writer.WriteLine(); // 空行分隔
            }
        }
    }

    /// <summary>
    /// 从 INI 数据中获取值
    /// </summary>
    private static string GetIniValue(string[] iniData, string section, string key)
    {
        bool inSection = string.IsNullOrEmpty(section);

        foreach (var line in iniData)
        {
            var trimmedLine = line.Trim();

            // 跳过空行和注释
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                continue;

            // 检查是否是一个新的 section
            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                inSection = trimmedLine.Trim('[', ']') == section;
                continue;
            }

            // 如果在目标 section 中，查找 key
            if (inSection && trimmedLine.Contains("="))
            {
                var parts = trimmedLine.Split(new[] { '=' }, 2);
                if (parts[0].Trim() == key)
                {
                    return parts[1].Trim();
                }
            }
        }

        return null;
    }
}
