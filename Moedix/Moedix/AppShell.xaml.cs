namespace Moedix
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CreditsPage), typeof(CreditsPage));
            Routing.RegisterRoute(nameof(ContentPageView), typeof(ContentPageView));
        }
    }

}
