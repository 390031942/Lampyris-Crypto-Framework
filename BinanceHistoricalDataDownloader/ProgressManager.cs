using System.Text.Json;

public class ProgressManager
{
    private string ProgressFile;

    public ProgressManager(string subprocessName)
    {
        ProgressFile = $"progress_{subprocessName}.json";
    }

    public Dictionary<string, DateTime> LoadProgress()
    {
        if (!File.Exists(ProgressFile))
        {
            if(File.Exists("progress.json"))
            {
                return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(File.ReadAllText("progress.json"));
            }
            return new Dictionary<string, DateTime>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(File.ReadAllText(ProgressFile));
    }

    public void SaveProgress(Dictionary<string, DateTime> progress)
    {
        string json = JsonSerializer.Serialize(progress, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ProgressFile, json);
    }
}
