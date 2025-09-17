namespace Moedix
{
    public partial class ContentPageView : ContentPage
    {
        public ContentPageView()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
