using System.Text.Json;

public class ProgressManager
{
    private const string ProgressFile = "progress.json";

    public Dictionary<string, DateTime> LoadProgress()
    {
        if (!File.Exists(ProgressFile))
        {
            return new Dictionary<string, DateTime>();
        }

        string json = File.ReadAllText(ProgressFile);
        return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);
    }

    public void SaveProgress(Dictionary<string, DateTime> progress)
    {
        string json = JsonSerializer.Serialize(progress, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ProgressFile, json);
    }
}
