using Moedix.Models;

namespace Moedix
{
    public partial class ShopPage : ContentPage
    {
        public ShopPage()
        {
            InitializeComponent();
            UpdateCoins();
        }

        void UpdateCoins() => CoinsLabel.Text = $"Moedas: {PlayerData.Instance.Coins}";

        void BuyGoldenSkin(object sender, EventArgs e)
        {
            if (PlayerData.Instance.Coins >= 50)
            {
                PlayerData.Instance.Coins -= 50;
                PlayerData.Instance.OwnedSkins.Add("GoldenPig");
                PlayerData.Instance.Save();
                DisplayAlert("Compra realizada!", "Você comprou a skin dourada!", "OK");
                UpdateCoins();
            }
            else
                DisplayAlert("Moedas insuficientes", "Jogue mais para ganhar moedas!", "OK");
        }

        void BuyExtraPower(object sender, EventArgs e)
        {
            if (PlayerData.Instance.Coins >= 100)
            {
                PlayerData.Instance.Coins -= 100;
                PlayerData.Instance.Save();
                DisplayAlert("Poder desbloqueado!", "Você ganhou um voo mais forte!", "OK");
                UpdateCoins();
            }
            else
                DisplayAlert("Moedas insuficientes", "Continue jogando!", "OK");
        }
    }
}
