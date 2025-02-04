namespace Lampyris.CSharp.Common;

using System.Reflection;

[AttributeUsage(AttributeTargets.Field)]
public class NamedValueAttribute : Attribute
{
    public string Name { get; private set; }

    public NamedValueAttribute(string name)
    {
        this.Name = name;
    }
}

public class EnumNameManager
{
    // 用于存储枚举值和名称的映射
    private static Dictionary<Enum, string>                   ms_NameMap      = new ();
    private static Dictionary<Type, Dictionary<string, Enum>> ms_NameValueMap = new ();
    private static Dictionary<Type, List<string>>             ms_NameListMap  = new ();

    static EnumNameManager()
    {
        // 在静态构造函数中扫描并记录所有枚举值的名称
        ScanAndRecordEnumNames();
    }

    // 扫描所有枚举类型，记录它们的NamedValueAttribute
    private static void ScanAndRecordEnumNames()
    {
        var enumTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsEnum);

        foreach (var type in enumTypes)
        {
            var nameList = ms_NameListMap[type] = new List<string>();
            ms_NameValueMap[type] = new Dictionary<string, Enum>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<NamedValueAttribute>();
                if (attribute != null)
                {
                    var enumValue = field.GetValue(null) as Enum;
                    ms_NameMap[enumValue] = attribute.Name;
                    ms_NameValueMap[type][attribute.Name] = enumValue; 
                    nameList.Add(attribute.Name);
                }
            }
        }
    }

    // 获取枚举值的名称，如果没有找到则返回null
    public static string GetName(Enum enumValue)
    {
        if (ms_NameMap.TryGetValue(enumValue, out string name))
        {
            return name;
        }

        return null;
    }

    // 获取枚举值的名称列表
    public static List<string> GetNames(Type enumType)
    {
        if (ms_NameListMap.TryGetValue(enumType, out List<string> namelist))
        {
            return namelist;
        }

        return null;
    }

    // 数据
    public static Enum GetEnum(Type enumType, string name)
    {
        if (ms_NameValueMap.TryGetValue(enumType, out var map))
        {
            if(map.TryGetValue(name, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
