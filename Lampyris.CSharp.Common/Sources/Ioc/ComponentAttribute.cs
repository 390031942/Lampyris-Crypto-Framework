namespace Lampyris.CSharp.Common;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ComponentAttribute:Attribute
{
    public string  name => m_Name;
    private string m_Name;

    public ComponentAttribute(string name = "")
    {
        m_Name = name;
    } 
}