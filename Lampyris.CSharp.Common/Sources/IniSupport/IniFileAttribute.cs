namespace Lampyris.CSharp.Common;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class IniFileAttribute : Attribute
{
    public string FileName { get; set; }
}