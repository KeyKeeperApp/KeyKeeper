using System.IO;
using System.Text.Json;

namespace KeyKeeper;

public static class AppSettings
{
    private static readonly string FilePath = "settings.json";

    public static bool ExitOnRepositoryClose { get; set; } = false;

    // Сохранение в файл
    public static void Save()
    {
        var data = new { ExitOnRepositoryClose };
        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(FilePath, json);
    }

    // Загрузка из файла
    public static void Load()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data != null)
                {
                    ExitOnRepositoryClose = data.ExitOnRepositoryClose;
                }
            }
            catch { /* Если файл поврежден, просто используем значения по умолчанию */ }
        }
    }

    private class SettingsData
    {
        public bool ExitOnRepositoryClose { get; set; }
    }
}