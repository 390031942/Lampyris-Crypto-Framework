namespace Lampyris.CSharp.Common;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ComponentAttribute:Attribute
{
    public string  Name => m_Name;
    private string m_Name;

    public string Tag => m_Tag;
    private string m_Tag;

    public ComponentAttribute(string name = "",string tag = "default")
    {
        m_Name = name;
        m_Tag = tag;
    } 
}