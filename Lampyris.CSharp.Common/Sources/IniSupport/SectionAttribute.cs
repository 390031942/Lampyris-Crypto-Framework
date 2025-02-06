namespace Lampyris.CSharp.Common;

using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SectionAttribute : Attribute
{
    public string Name { get; set; }
}
