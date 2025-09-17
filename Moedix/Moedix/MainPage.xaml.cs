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
Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
Application.Current.Quit();
#endif
        }

        private async void OnCreditsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CreditsPage));
        }

        private async void OnContentClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ContentPageView));
        }


    }
}