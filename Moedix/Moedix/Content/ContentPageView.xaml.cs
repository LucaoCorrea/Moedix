using Moedix.Models; // Necessário para o PlayerData
using System.Linq;    // Necessário para o .OfType<Button>()

namespace Moedix
{
    public partial class ContentPageView : ContentPage
    {
        // Define os custos de cada item aqui
        private Dictionary<string, int> contentCosts = new Dictionary<string, int>
        {
            { "Conteudo1", 0 },    // Grátis
            { "Conteudo2", 50 },
            { "Conteudo3", 100 },
            { "Conteudo4", 150 },
            { "Conteudo5", 200 },
            { "Conteudo6", 250 },
            { "Conteudo7", 300 }
        };

        public ContentPageView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Garante que o item 1 é grátis
            Preferences.Set("Conteudo1_Unlocked", true);

            UpdateCoinLabel();
            InitializeLockStates();
        }

        // Atualiza o placar de moedas
        void UpdateCoinLabel()
        {
            int playerCoins = PlayerData.Instance.Coins;
            PlayerCoinLabel.Text = $"Moedas: {playerCoins} 💰";
        }

        // Verifica o estado (comprado/bloqueado) de cada item ao carregar a página
        void InitializeLockStates()
        {
            foreach (var item in ContentList.Children)
            {
                if (item is Frame frame && frame.Content is VerticalStackLayout vsl)
                {
                    // Encontra o botão de header para pegar o ID do conteúdo
                    var headerButton = vsl.Children.OfType<Button>().FirstOrDefault(b => b.CommandParameter != null);
                    if (headerButton == null) continue;

                    string contentId = headerButton.CommandParameter.ToString();
                    bool isUnlocked = Preferences.Get($"{contentId}_Unlocked", false);

                    // Encontra os painéis pelo nome
                    var lockPanel = this.FindByName($"LockPanel{contentId.Replace("Conteudo", "")}") as VerticalStackLayout;
                    var contentBody = this.FindByName($"{contentId}Body") as StackLayout;

                    if (lockPanel != null)
                    {
                        lockPanel.IsVisible = !isUnlocked;
                    }
                    if (contentBody != null)
                    {
                        contentBody.IsVisible = false; // Garante que todos começam fechados
                    }

                    // Garante que o ícone do botão esteja fechado (▼)
                    if (headerButton.Text.Contains("▲"))
                    {
                        headerButton.Text = headerButton.Text.Replace("▲", "▼");
                    }
                }
            }
        }

        // Chamado ao clicar no TÍTULO de um item
        private void OnHeaderClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string contentId = button.CommandParameter.ToString(); // ex: "Conteudo1"

            bool isUnlocked = Preferences.Get($"{contentId}_Unlocked", false);

            if (isUnlocked)
            {
                // Encontra o corpo pelo nome (ex: "Conteudo1Body")
                var contentBody = this.FindByName($"{contentId}Body") as StackLayout;

                if (contentBody != null)
                {
                    // Abre ou fecha o conteúdo
                    contentBody.IsVisible = !contentBody.IsVisible;
                    // Muda o ícone do botão
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

        // Chamado ao clicar no BOTÃO DE DESBLOQUEAR
        private async void OnUnlockClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string contentId = button.CommandParameter.ToString(); // ex: "Conteudo2"

            int cost = contentCosts[contentId];
            int playerCoins = PlayerData.Instance.Coins;

            if (playerCoins >= cost)
            {
                bool buy = await DisplayAlert("Confirmar Compra", $"Deseja desbloquear este item por {cost} moedas?", "Comprar", "Cancelar");

                if (buy)
                {
                    // Paga com as moedas
                    playerCoins -= cost;
                    PlayerData.Instance.Coins = playerCoins;
                    PlayerData.Instance.Save();

                    // Salva que o item foi comprado
                    Preferences.Set($"{contentId}_Unlocked", true);

                    // Atualiza o placar de moedas
                    UpdateCoinLabel();

                    // Esconde o painel de bloqueio
                    var lockPanel = (VerticalStackLayout)button.Parent;
                    lockPanel.IsVisible = false;

                    // Encontra e MOSTRA o conteúdo
                    var contentBody = this.FindByName($"{contentId}Body") as StackLayout;
                    if (contentBody != null)
                    {
                        contentBody.IsVisible = true; // <-- A MÁGICA ACONTECE AQUI!
                    }

                    // Acha o botão "pai" (header) para mudar o ícone para aberto (▲)
                    var parentVsl = (VerticalStackLayout)lockPanel.Parent;
                    var headerButton = parentVsl.Children.OfType<Button>().FirstOrDefault(b => b.CommandParameter != null && b.CommandParameter.ToString() == contentId);
                    if (headerButton != null && headerButton.Text.Contains("▼"))
                    {
                        headerButton.Text = headerButton.Text.Replace("▼", "▲");
                    }

                    await DisplayAlert("Sucesso!", "Conteúdo desbloqueado!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Moedas Insuficientes", $"Você precisa de {cost} moedas para desbloquear. Você tem {playerCoins}.", "OK");
            }
        }

        // Botão de voltar
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}