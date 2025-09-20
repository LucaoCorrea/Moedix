using System.Diagnostics;
using Microsoft.Maui.Controls;

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
            Debug.WriteLine("Exit button clicked");

#if ANDROID
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            Debug.WriteLine(activity != null ? "Activity found" : "Activity not found");
            activity?.FinishAffinity(); 
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