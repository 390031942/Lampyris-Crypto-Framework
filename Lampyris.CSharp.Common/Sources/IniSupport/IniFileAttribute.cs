namespace Lampyris.CSharp.Common;

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class IniFileAttribute : Attribute
{
    public string FileName { get; }
    public IniFileAttribute(string fileName)
    {
        FileName = fileName;
    }

    public IniFileAttribute()
    {
        FileName = "common_setting";
    }
}