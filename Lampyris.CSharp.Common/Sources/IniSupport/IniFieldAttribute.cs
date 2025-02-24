namespace Lampyris.CSharp.Common;

using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IniFieldAttribute : Attribute
{
    public string Section { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }


    public IniFieldAttribute(string section = "",string name = "", string desc = "")
    {
        Section = section;
        Name = name;
        Desc = desc;
    }
}
