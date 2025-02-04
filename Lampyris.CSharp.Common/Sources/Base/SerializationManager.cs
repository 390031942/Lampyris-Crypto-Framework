namespace Lampyris.CSharp.Common;

using System.Text.Json;
public class SerializationManager: BehaviourSingleton<SerializationManager>
{
    private class SerializationInfo
    {
        public object serializableObject;
        public string name;
    }

    private readonly List<SerializationInfo> m_serializableInfo = new List<SerializationInfo>();

    public override void OnStart()
    {

    }
    
    // Unused
    public override void OnUpdate(float deltaTime)
    {

    }

    public void Register(object serializableObject, string specificName = "")
    {
        if (serializableObject != null)
        {
            m_serializableInfo.Add(new SerializationInfo()
            {
                serializableObject = serializableObject,
                name = string.IsNullOrEmpty(specificName) ? serializableObject.GetType().Name : specificName
            });
        }
    }

    public T TryDeserializeObjectFromFile<T>(string specificName = "")
    {
        string filePath = Path.Combine(PathUtil.SerializedDataSavePath, (string.IsNullOrEmpty(specificName) ? typeof(T).Name : specificName) + ".bin");

        if(File.Exists(filePath))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                return JsonSerializer.Deserialize<T>(stream);
            }
        }

        return default(T);
    }
    
    public override void OnDestroy()
    {
        foreach (SerializationInfo serializationInfo in m_serializableInfo)
        {
            string filePath = Path.Combine(PathUtil.SerializedDataSavePath, serializationInfo.name + ".bin");
            using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                JsonSerializer.Serialize(stream, serializationInfo.serializableObject);
            }
        }
    }
}
