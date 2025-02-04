namespace Lampyris.CSharp.Common;

using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class IniConfigAttribute : Attribute
{
    public string Section { get; }
    public string Key { get; }
    public string DefaultValue { get; }

    public IniConfigAttribute(string section, string key, string defaultValue = "")
    {
        Section = section;
        Key = key;
        DefaultValue = defaultValue;
    }
}