using Moedix.Models;

namespace Moedix
{
    public partial class ShopPage : ContentPage
    {
        public ShopPage()
        {
            InitializeComponent();
            UpdateCoins();
            LoadCustomizationOptions();
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
                LoadCustomizationOptions();
            }
            else
                DisplayAlert("Moedas insuficientes", "Jogue mais para ganhar moedas!", "OK");
        }

        void BuyExtraPower(object sender, EventArgs e)
        {
            if (PlayerData.Instance.Coins >= 100)
            {
                PlayerData.Instance.Coins -= 100;
                PlayerData.Instance.OwnedPowers.Add("ExtraPower");
                PlayerData.Instance.Save();
                DisplayAlert("Poder desbloqueado!", "Você ganhou um voo mais forte!", "OK");
                UpdateCoins();
                LoadCustomizationOptions();
            }
            else
                DisplayAlert("Moedas insuficientes", "Continue jogando!", "OK");
        }

        void LoadCustomizationOptions()
        {
            bool hasAnyCustomization = PlayerData.Instance.OwnedSkins.Any() || PlayerData.Instance.OwnedPowers.Any();
            CustomizationSection.IsVisible = hasAnyCustomization;

            SkinPicker.Items.Clear();
            SkinPicker.Items.Add("Padrão");
            foreach (var skin in PlayerData.Instance.OwnedSkins)
                SkinPicker.Items.Add(skin == "GoldenPig" ? "Skin Dourada" : skin);

            var selectedSkin = PlayerData.Instance.SelectedSkin ?? "Padrão";
            SkinPicker.SelectedItem = selectedSkin == "GoldenPig" ? "Skin Dourada" : selectedSkin;

            PowerSwitch.IsToggled = PlayerData.Instance.PowerEnabled;
        }

        void OnSkinSelected(object sender, EventArgs e)
        {
            if (SkinPicker.SelectedItem is string selectedSkin)
            {
                PlayerData.Instance.SelectedSkin = selectedSkin == "Skin Dourada" ? "GoldenPig" : selectedSkin;
                PlayerData.Instance.Save();
            }
        }

        void OnPowerToggled(object sender, ToggledEventArgs e)
        {
            if (PlayerData.Instance.OwnedPowers.Contains("ExtraPower"))
            {
                PlayerData.Instance.PowerEnabled = e.Value;
                PlayerData.Instance.Save();
            }
            else
            {
                PowerSwitch.IsToggled = false;
                DisplayAlert("Bloqueado", "Você ainda não comprou o poder extra!", "OK");
            }
        }

        private async void GoBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
