namespace Lampyris.CSharp.Common;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class AutowiredAttribute : Attribute
{
    public string Name { get; private set; }

    public AutowiredAttribute(string name = "")
    {
        this.Name = name;
    }
}