namespace Lampyris.CSharp.Common;

using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

public class IniConfigManager:BehaviourSingleton<IniConfigManager>
{
    private readonly Dictionary<string, IniFile> m_IniFiles = new Dictionary<string, IniFile>(StringComparer.OrdinalIgnoreCase);

    private void LoadConfig(string fileName, FieldInfo[] fields)
    {
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<IniConfigAttribute>();
            if (attribute != null)
            {
                if (!m_IniFiles.ContainsKey(fileName))
                {
                    m_IniFiles[fileName] = new IniFile(fileName);
                }

                string value = m_IniFiles[fileName].ReadValue(attribute.Section, attribute.Key, attribute.DefaultValue);
                var convertedValue = Convert.ChangeType(value, field.FieldType);
                field.SetValue(null, convertedValue);
            }
        }
    }

    private void SaveConfig(string fileName, FieldInfo[] fields)
    {
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<IniConfigAttribute>();
            if(attribute != null)
            {
                if (!m_IniFiles.ContainsKey(fileName))
                {
                    m_IniFiles[fileName] = new IniFile(fileName);
                }

                string? value = field.GetValue(null)?.ToString();
                if(value != null)
                {
                    m_IniFiles[fileName].WriteValue(attribute.Section, attribute.Key, value);
                }
            }
        }

        foreach (var iniFile in m_IniFiles.Values)
        {
            iniFile.Save();
        }
    }

    private void ExecutionSaveLoad(bool isSave)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            // 获取程序集中的所有类型
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                IniFileAttribute? attribute = type.GetCustomAttribute<IniFileAttribute>();
                if (attribute != null)
                {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(field => Attribute.IsDefined(field, typeof(IniConfigAttribute))).ToArray();

                    if (isSave)
                    {
                        SaveConfig(attribute.FileName,fields);
                    }
                    else
                    {
                        LoadConfig(attribute.FileName,fields);
                    }
                }
            }
        }
    }

    public override void OnStart()
    {
        base.OnStart();
        ExecutionSaveLoad(false);
    }

    public override void OnDestroy()
    {
        ExecutionSaveLoad(true);
        base.OnDestroy();
    }
}
