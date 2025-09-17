using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Moedix
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
