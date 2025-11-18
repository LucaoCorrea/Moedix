using Moedix.Models;
using System.Linq; 

namespace Moedix
{
    public partial class ContentPageView : ContentPage
    {
        private Dictionary<string, int> contentCosts = new Dictionary<string, int>
        {
            { "Conteudo1", 0 },   
            { "Conteudo2", 50 },
            { "Conteudo3", 100 },
            { "Conteudo4", 150 },
            { "Conteudo5", 200 },
            { "Conteudo6", 250 },
            { "Conteudo7", 300 },
            { "Conteudo8", 350 },
            { "Conteudo9", 400 },
            { "Conteudo10", 450 },
        };

        public ContentPageView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Preferences.Set("Conteudo1_Unlocked", true);

            UpdateCoinLabel();
            InitializeLockStates();
        }

        void UpdateCoinLabel()
        {
            int playerCoins = PlayerData.Instance.Coins;
            PlayerCoinLabel.Text = $"Moedas: {playerCoins} 💰";
        }

        void InitializeLockStates()
        {
            foreach (var item in ContentList.Children)
            {
                if (item is Frame frame && frame.Content is VerticalStackLayout vsl)
                {
                    var headerButton = vsl.Children.OfType<Button>().FirstOrDefault(b => b.CommandParameter != null);
                    if (headerButton == null) continue;

                    string contentId = headerButton.CommandParameter.ToString();
                    bool isUnlocked = Preferences.Get($"{contentId}_Unlocked", false);

                    var lockPanel = this.FindByName($"LockPanel{contentId.Replace("Conteudo", "")}") as VerticalStackLayout;
                    var contentBody = this.FindByName($"{contentId}Body") as StackLayout;

                    if (lockPanel != null)
                    {
                        lockPanel.IsVisible = !isUnlocked;
                    }
                    if (contentBody != null)
                    {
                        contentBody.IsVisible = false;
                    }

                    if (headerButton.Text.Contains("▲"))
                    {
                        headerButton.Text = headerButton.Text.Replace("▲", "▼");
                    }
                }
            }
        }

        private void OnHeaderClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string contentId = button.CommandParameter.ToString();

            bool isUnlocked = Preferences.Get($"{contentId}_Unlocked", false);

            if (isUnlocked)
            {
                var contentBody = this.FindByName($"{contentId}Body") as StackLayout;

                if (contentBody != null)
                {
                    contentBody.IsVisible = !contentBody.IsVisible;

                    button.Text = button.Text.Contains("▼")
                        ? button.Text.Replace("▼", "▲")
                        : button.Text.Replace("▲", "▼");
                }
            }
            else
            {
                DisplayAlert("Bloqueado", "Você precisa desbloquear este conteúdo primeiro!", "OK");
            }
        }

        private async void OnUnlockClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string contentId = button.CommandParameter.ToString();

            int cost = contentCosts[contentId];
            int playerCoins = PlayerData.Instance.Coins;

            if (playerCoins >= cost)
            {
                bool buy = await DisplayAlert("Confirmar Compra", $"Deseja desbloquear este item por {cost} moedas?", "Comprar", "Cancelar");

                if (buy)
                {
                    playerCoins -= cost;
                    PlayerData.Instance.Coins = playerCoins;
                    PlayerData.Instance.Save();

                    Preferences.Set($"{contentId}_Unlocked", true);

                    UpdateCoinLabel();

                    var lockPanel = (VerticalStackLayout)button.Parent;
                    lockPanel.IsVisible = false;

                    var contentBody = this.FindByName($"{contentId}Body") as StackLayout;
                    if (contentBody != null)
                    {
                        contentBody.IsVisible = true;
                    }

                    var parentVsl = (VerticalStackLayout)lockPanel.Parent;
                    var headerButton = parentVsl.Children.OfType<Button>().FirstOrDefault(b =>
                        b.CommandParameter != null && b.CommandParameter.ToString() == contentId);

                    if (headerButton != null && headerButton.Text.Contains("▼"))
                    {
                        headerButton.Text = headerButton.Text.Replace("▼", "▲");
                    }

                    await DisplayAlert("Sucesso!", "Conteúdo desbloqueado!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Moedas Insuficientes",
                    $"Você precisa de {cost} moedas para desbloquear. Você tem {playerCoins}.", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnVideoButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string url)
            {
                try
                {
                    await Launcher.Default.OpenAsync(url);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", "Não foi possível abrir o vídeo: " + ex.Message, "OK");
                }
            }
        }
    }
}
