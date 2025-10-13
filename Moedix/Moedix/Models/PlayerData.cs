using System.Text.Json;

namespace Moedix.Models
{
    public class PlayerData
    {
        private static PlayerData _instance;
        public static PlayerData Instance => _instance ??= Load();

        public int Coins { get; set; } = 0;
        public int HighScore { get; set; } = 0;
        public HashSet<string> OwnedSkins { get; set; } = new();
        public HashSet<string> OwnedPowers { get; set; } = new();
        public string? SelectedSkin { get; set; }
        public bool PowerEnabled { get; set; } = false;

        private const string StorageKey = "player_data.json";
        private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, StorageKey);

        public static PlayerData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<PlayerData>(json) ?? new PlayerData();
                }
            }
            catch { }
            return new PlayerData();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }
}
