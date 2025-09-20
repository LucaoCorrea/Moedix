using System;
using System.Linq;
using Microsoft.Maui.Controls;

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
            if (Navigation.NavigationStack?.Count > 1)
                await Navigation.PopAsync();
            else
                await Shell.Current.GoToAsync("..");
        }

        private void OnToggleClicked(object sender, EventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is string targetName))
                return;

            var target = this.FindByName<StackLayout>(targetName);
            if (target == null)
                return;

            bool willShow = !target.IsVisible;

            foreach (var child in ContentList.Children.OfType<Frame>())
            {
                if (child.Content is VerticalStackLayout vs && vs.Children.Count > 1 && vs.Children[1] is StackLayout body)
                {
                    body.IsVisible = false;

                    if (vs.Children[0] is Button headerBtn)
                    {
                        if (headerBtn.Text.Contains("▲"))
                            headerBtn.Text = headerBtn.Text.Replace("▲", "▼");
                    }
                }
            }

            target.IsVisible = willShow;
            if (willShow)
            {
                if (btn.Text.Contains("▼"))
                    btn.Text = btn.Text.Replace("▼", "▲");
            }
            else
            {
                if (btn.Text.Contains("▲"))
                    btn.Text = btn.Text.Replace("▲", "▼");
            }
        }
    }
}
