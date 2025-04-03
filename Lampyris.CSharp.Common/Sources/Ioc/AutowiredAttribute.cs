namespace Lampyris.CSharp.Common;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class AutowiredAttribute : Attribute
{
    private string name;

    public AutowiredAttribute(string name = "")
    {
        this.name = name;
    }
}