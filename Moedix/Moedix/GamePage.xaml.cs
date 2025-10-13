using Microsoft.Maui.Controls.Shapes;
using Moedix.Models;
using System;
using System.Timers;

namespace Moedix
{
    public partial class GamePage : ContentPage
    {
        double gravity = 0.6;
        double jumpStrength = -12;
        double pigVelocity = 0;
        double gameSpeed = 4;
        double speedIncrease = 0.001;
        int score = 0;
        bool gameRunning = false;

        System.Timers.Timer gameTimer;

        BoxView topPillar, bottomPillar;
        Ellipse coin;
        double pillarGap = 150;
        double pillarWidth = 60;

        Random random = new();

        public GamePage()
        {
            InitializeComponent();
        }

        void OnPageLoaded(object sender, NavigatedToEventArgs e)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            GameLayout.GestureRecognizers.Add(tap);

            ApplyPlayerCustomizations();
            ResetGame();
        }

        void ApplyPlayerCustomizations()
        {
            var player = PlayerData.Instance;

            string selectedSkin = player.SelectedSkin ?? "Padrão";

            if (selectedSkin == "GoldenPig")
            {
                Pig.Fill = new RadialGradientBrush
                {                    GradientStops =
            {
                new GradientStop(Color.FromArgb("#FFD700"), 0.2f),
                new GradientStop(Color.FromArgb("#B8860B"), 1f)
            }
                };
            }
            else
            {
                Pig.Fill = new SolidColorBrush(Colors.Pink);
            }

            if (player.OwnedPowers.Contains("ExtraPower") && player.PowerEnabled)
            {
                jumpStrength = -10; 
            }
            else
            {
                jumpStrength = -12; 
            }
        }

        void ResetGame()
        {
            gameRunning = false;
            score = 0;
            ScoreLabel.Text = "Pontos: 0";
            GameOverScreen.IsVisible = false;
            RemoveObjects();
            AbsoluteLayout.SetLayoutBounds(Pig, new Rect(100, 300, 50, 50));
        }

        void StartGame()
        {
            RemoveObjects();
            score = 0;
            gameSpeed = 4;
            pigVelocity = 0;
            gameRunning = true;
            GameOverScreen.IsVisible = false;
            CreatePillars();
            CreateCoin();

            gameTimer?.Stop();
            gameTimer = new System.Timers.Timer(20);
            gameTimer.Elapsed += GameLoop;
            gameTimer.AutoReset = true;
            gameTimer.Start();
        }

        void RemoveObjects()
        {
            if (topPillar != null) GameLayout.Children.Remove(topPillar);
            if (bottomPillar != null) GameLayout.Children.Remove(bottomPillar);
            if (coin != null) GameLayout.Children.Remove(coin);
        }

        void GameLoop(object sender, ElapsedEventArgs e)
        {
            if (!gameRunning) return;

            gameSpeed += speedIncrease;
            pigVelocity += gravity;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var pigRect = AbsoluteLayout.GetLayoutBounds(Pig);
                double newY = pigRect.Y + pigVelocity;
                AbsoluteLayout.SetLayoutBounds(Pig, new Rect(pigRect.X, newY, pigRect.Width, pigRect.Height));

                MoveObjects();

                double groundY = GameLayout.Height - Ground.HeightRequest;
                if (newY + pigRect.Height >= groundY || newY <= 0 || CheckCollision())
                {
                    EndGame();
                    return;
                }

                CheckCoinCollection();
            });
        }

        void CreatePillars()
        {
            double canvasHeight = Math.Max(GameLayout.Height, 800);
            double topHeight = random.Next(80, (int)(canvasHeight * 0.45));
            double bottomY = topHeight + pillarGap;
            double bottomHeight = Math.Max(100, canvasHeight - bottomY - Ground.HeightRequest);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RemoveObjects();
                topPillar = new BoxView { Color = Colors.SaddleBrown, WidthRequest = pillarWidth, HeightRequest = topHeight };
                bottomPillar = new BoxView { Color = Colors.SaddleBrown, WidthRequest = pillarWidth, HeightRequest = bottomHeight };

                double startX = GameLayout.Width + 50;
                AbsoluteLayout.SetLayoutBounds(topPillar, new Rect(startX, 0, pillarWidth, topHeight));
                AbsoluteLayout.SetLayoutBounds(bottomPillar, new Rect(startX, bottomY, pillarWidth, bottomHeight));

                GameLayout.Children.Add(topPillar);
                GameLayout.Children.Add(bottomPillar);
            });
        }

        void CreateCoin()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (topPillar == null || bottomPillar == null) return;

                int coinValue = random.Next(1, 11);

                double baseSize = 20;
                double coinSize = baseSize + (coinValue * 3);

                Color coinColor = coinValue <= 3 ? Colors.Peru :
                                  coinValue <= 7 ? Colors.Silver :
                                  Colors.Gold;

                coin = new Ellipse
                {
                    Fill = coinColor,
                    Stroke = Colors.DarkGoldenrod,
                    StrokeThickness = 2,
                    WidthRequest = coinSize,
                    HeightRequest = coinSize,
                    BindingContext = coinValue 
                };

                var topBounds = AbsoluteLayout.GetLayoutBounds(topPillar);
                var bottomBounds = AbsoluteLayout.GetLayoutBounds(bottomPillar);

                double startX = GameLayout.Width + 100;

                double gapMiddle = topBounds.Height + (bottomBounds.Y - topBounds.Height) / 2;
                double offset = random.Next(-30, 30);
                double coinY = gapMiddle + offset;
                coinY = Math.Clamp(coinY, 50, GameLayout.Height - Ground.HeightRequest - 60);

                AbsoluteLayout.SetLayoutBounds(coin, new Rect(startX, coinY, coinSize, coinSize));
                GameLayout.Children.Add(coin);
            });
        }

        void MoveObjects()
        {
            MovePillar(topPillar);
            MovePillar(bottomPillar);
            MoveCoin();

            var topRect = AbsoluteLayout.GetLayoutBounds(topPillar);
            if (topRect.X + topRect.Width < 0)
            {
                score++;
                ScoreLabel.Text = $"Pontos: {score}";
                CreatePillars();
                if (random.Next(0, 3) == 1) CreateCoin();
            }
        }

        void MovePillar(BoxView pillar)
        {
            if (pillar == null) return;
            var rect = AbsoluteLayout.GetLayoutBounds(pillar);
            rect.X -= gameSpeed;
            AbsoluteLayout.SetLayoutBounds(pillar, rect);
        }

        void MoveCoin()
        {
            if (coin == null) return;
            var rect = AbsoluteLayout.GetLayoutBounds(coin);
            rect.X -= gameSpeed;
            AbsoluteLayout.SetLayoutBounds(coin, rect);
            if (rect.X + rect.Width < 0)
            {
                GameLayout.Children.Remove(coin);
                coin = null;
            }
        }

        void CheckCoinCollection()
        {
            if (coin == null) return;
            var pigRect = AbsoluteLayout.GetLayoutBounds(Pig);
            var coinRect = AbsoluteLayout.GetLayoutBounds(coin);

            bool collected = pigRect.IntersectsWith(coinRect);

            if (collected)
            {
                int coinValue = (int)coin.BindingContext;

                GameLayout.Children.Remove(coin);
                coin = null;

                PlayerData.Instance.Coins += coinValue;
                PlayerData.Instance.Save();

                // 🔸 Texto flutuante com valor
                ShowFloatingText($"+{coinValue}", pigRect.X + 20, pigRect.Y - 30);
            }
        }

        async void ShowFloatingText(string text, double x, double y)
        {
            var label = new Label
            {
                Text = text,
                TextColor = Colors.Yellow,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center
            };

            AbsoluteLayout.SetLayoutBounds(label, new Rect(x, y, 60, 30));
            GameLayout.Children.Add(label);

            await label.TranslateTo(0, -50, 800, Easing.SinOut);
            await label.FadeTo(0, 400);
            GameLayout.Children.Remove(label);
        }

        bool CheckCollision()
        {
            if (topPillar == null || bottomPillar == null) return false;
            var pig = AbsoluteLayout.GetLayoutBounds(Pig);
            var top = AbsoluteLayout.GetLayoutBounds(topPillar);
            var bottom = AbsoluteLayout.GetLayoutBounds(bottomPillar);

            return (pig.IntersectsWith(top) || pig.IntersectsWith(bottom));
        }

        void EndGame()
        {
            gameRunning = false;
            gameTimer?.Stop();

            if (score > PlayerData.Instance.HighScore)
                PlayerData.Instance.HighScore = score;

            PlayerData.Instance.Save();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GameLayout.Children.Remove(GameOverScreen);
                GameLayout.Children.Add(GameOverScreen);

                GameOverScreen.IsVisible = true;
                FinalScoreLabel.Text = $"Pontos: {score}\nRecorde: {PlayerData.Instance.HighScore}\nMoedas: {PlayerData.Instance.Coins}";
            });
        }

        void OnTapped(object sender, EventArgs e)
        {
            if (!gameRunning)
            {
                StartGame();
                return;
            }

            pigVelocity = jumpStrength;
        }

        void RestartGame(object sender, EventArgs e)
        {
            ResetGame();
            StartGame();
        }

        async void GoBackToMainPage(object sender, EventArgs e)
        {
            gameTimer?.Stop();
            await Navigation.PopAsync();
        }
    }
}
