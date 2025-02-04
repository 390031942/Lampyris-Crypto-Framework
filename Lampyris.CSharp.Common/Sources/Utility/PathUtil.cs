
.namespace Lampyris.Framework.Server.Common;

public static class PathUtil
{
    public static string SerializedDataSavePath
    {
        get
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),AppConfig.AppDocFolderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
