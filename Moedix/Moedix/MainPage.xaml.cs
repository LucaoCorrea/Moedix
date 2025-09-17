namespace Moedix
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
#if ANDROID
            Platform.CurrentActivity.FinishAffinity(); // erro ao clicar em sair
#elif WINDOWS
            Application.Current.Quit();
#endif
        }

        private async void OnCreditsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreditsPage());
        }
    }
}