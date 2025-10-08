namespace Moedix.Models;

public class PlayerData
{
    public int Coins { get; set; } = 0;
    public int BestScore { get; set; } = 0;
    public List<string> OwnedSkins { get; set; } = new();

    private static PlayerData _instance;
    public static PlayerData Instance => _instance ??= new PlayerData();
}
