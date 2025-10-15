using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Graphics;

namespace Moedix.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var window = Application.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Title = "Moedix";

            var presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
            }

            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var screenWidth = displayArea.WorkArea.Width;
            var screenHeight = displayArea.WorkArea.Height;
            appWindow.MoveAndResize(new RectInt32(0, 0, screenWidth, screenHeight));

            try
            {
                string iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "logo.png");
                if (File.Exists(iconPath))
                {
                    IntPtr hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 256, 256, LR_LOADFROMFILE);
                    if (hIcon != IntPtr.Zero)
                    {
                        SendMessage(hwnd, WM_SETICON, new IntPtr(1), hIcon); 
                        SendMessage(hwnd, WM_SETICON, IntPtr.Zero, hIcon);  
                    }
                }
            }
            catch
            {
            }
        }

        private const int WM_SETICON = 0x80;
        private const int IMAGE_ICON = 1;
        private const int LR_LOADFROMFILE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, int uType,
                                               int cxDesired, int cyDesired, int fuLoad);
    }
}
