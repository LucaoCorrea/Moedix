using System.Diagnostics;

namespace Moedix
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmação", "Deseja realmente sair do app?", "Não", "Sim");
            if (!confirm) 

            System.Environment.Exit(0); 
        }



        private async void OnCreditsClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Exit button clicked");
            await Shell.Current.GoToAsync(nameof(CreditsPage));
        }

        private async void OnContentClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ContentPageView));
        }
    }
}