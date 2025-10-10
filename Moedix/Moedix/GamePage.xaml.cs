using System;
using Microsoft.Maui.Controls;
using System.Timers;
using Microsoft.Maui.Graphics;

namespace Moedix
{
    public partial class GamePage : ContentPage
    {
        // Física
        double gravity = 0.6;
        double jumpStrength = -12;
        double pigVelocity = 0;

        // Velocidade do jogo
        double gameSpeed = 4;
        double speedIncrease = 0.0008;

        // Estado
        int score = 0;
        bool gameRunning = false;

        // Timer
        System.Timers.Timer gameTimer;

        // Pilares
        BoxView topPillar;
        BoxView bottomPillar;
        double pillarGap = 150;
        double pillarWidth = 60;

        Random random = new();

        public GamePage()
        {
            InitializeComponent();
        }

        // Assim que a página for carregada
        void OnPageLoaded(object sender, NavigatedToEventArgs e)
        {
            // Cria o gesto de toque
            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;

            // Adiciona ao layout do jogo (NÃO à página)
            GameLayout.GestureRecognizers.Add(tap);

            ShowStartScreen();
        }

        void ShowStartScreen()
        {
            gameRunning = false;
            GameOverLabel.IsVisible = false;
            RestartButton.IsVisible = false;
            BackButton.IsVisible = true;
            score = 0;
            ScoreLabel.Text = "Pontos: 0";
            AbsoluteLayout.SetLayoutBounds(Pig, new Rect(100, 300, 50, 50));
            RemovePillars();
        }

        void StartGame()
        {
            BackButton.IsVisible = false;
            RemovePillars();
            ResetPig();
            score = 0;
            ScoreLabel.Text = "Pontos: 0";
            gameSpeed = 4;
            pigVelocity = 0;
            gameRunning = true;
            GameOverLabel.IsVisible = false;
            RestartButton.IsVisible = false;
            CreatePillars();

            gameTimer?.Stop();
            gameTimer = new System.Timers.Timer(20);
            gameTimer.Elapsed += GameLoop;
            gameTimer.AutoReset = true;
            gameTimer.Start();
        }

        void ResetPig()
        {
            AbsoluteLayout.SetLayoutBounds(Pig, new Rect(100, 300, 50, 50));
            pigVelocity = 0;
        }

        void RemovePillars()
        {
            if (topPillar != null)
            {
                GameLayout.Children.Remove(topPillar);
                topPillar = null;
            }
            if (bottomPillar != null)
            {
                GameLayout.Children.Remove(bottomPillar);
                bottomPillar = null;
            }
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

                MovePillars();

                double groundY = GameLayout.Height - Ground.HeightRequest;
                if (newY + pigRect.Height >= groundY || newY <= 0 || CheckCollision())
                {
                    EndGame();
                    return;
                }
            });
        }

        void CreatePillars()
        {
            double canvasWidth = Math.Max(GameLayout.Width, 800);
            double canvasHeight = Math.Max(GameLayout.Height, 800);

            double topHeight = random.Next(80, (int)(canvasHeight * 0.45));
            double bottomY = topHeight + pillarGap;
            double bottomHeight = Math.Max(100, canvasHeight - bottomY - Ground.HeightRequest);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RemovePillars();

                topPillar = new BoxView
                {
                    Color = Colors.SaddleBrown,
                    WidthRequest = pillarWidth,
                    HeightRequest = topHeight
                };

                bottomPillar = new BoxView
                {
                    Color = Colors.SaddleBrown,
                    WidthRequest = pillarWidth,
                    HeightRequest = bottomHeight
                };

                double startX = canvasWidth + 30;

                AbsoluteLayout.SetLayoutBounds(topPillar, new Rect(startX, 0, pillarWidth, topHeight));
                AbsoluteLayout.SetLayoutBounds(bottomPillar, new Rect(startX, bottomY, pillarWidth, bottomHeight));

                GameLayout.Children.Add(topPillar);
                GameLayout.Children.Add(bottomPillar);
            });
        }

        void MovePillars()
        {
            if (topPillar == null || bottomPillar == null) return;

            var topRect = AbsoluteLayout.GetLayoutBounds(topPillar);
            var bottomRect = AbsoluteLayout.GetLayoutBounds(bottomPillar);

            topRect.X -= gameSpeed;
            bottomRect.X -= gameSpeed;

            AbsoluteLayout.SetLayoutBounds(topPillar, topRect);
            AbsoluteLayout.SetLayoutBounds(bottomPillar, bottomRect);

            if (topRect.X + topRect.Width < 0)
            {
                score++;
                ScoreLabel.Text = $"Pontos: {score}";
                CreatePillars();
            }
        }

        bool CheckCollision()
        {
            if (topPillar == null || bottomPillar == null) return false;

            var pig = AbsoluteLayout.GetLayoutBounds(Pig);
            var top = AbsoluteLayout.GetLayoutBounds(topPillar);
            var bottom = AbsoluteLayout.GetLayoutBounds(bottomPillar);

            bool hitTop = pig.X + pig.Width > top.X && pig.X < top.X + top.Width && pig.Y < top.Height;
            bool hitBottom = pig.X + pig.Width > bottom.X && pig.X < bottom.X + bottom.Width && pig.Y + pig.Height > bottom.Y;

            return hitTop || hitBottom;
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            // Aqui vai o que você quer que aconteça quando a página for carregada
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

        void EndGame()
        {
            gameRunning = false;
            try { gameTimer?.Stop(); } catch { }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GameOverLabel.IsVisible = true;
                RestartButton.IsVisible = true;
                BackButton.IsVisible = true;
            });
        }

        void RestartGame(object sender, EventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            RemovePillars();
            ResetPig();
            StartGame();
        }

        async void GoBackToMainPage(object sender, EventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            await Navigation.PopAsync();
        }
    }
}
