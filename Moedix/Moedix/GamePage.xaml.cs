using Moedix.Models;
using System.Timers;
using Plugin.Maui.Audio;

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

        System.Timers.Timer? gameTimer;
        System.Timers.Timer? spriteTimer;

        Image? topPillar;
        Image? bottomPillar;
        Image? coin;
        double pillarGap = 250;
        double pillarWidth = 100;
        int spriteIndex = 1;
        Random random = new();

        readonly IAudioManager _audioManager;
        IAudioPlayer? _backgroundMusic;
        bool isMuted = false;

        public GamePage() : this(AudioManager.Current) { }

        public GamePage(IAudioManager audioManager)
        {
            InitializeComponent();
            _audioManager = audioManager;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            GameLayout?.GestureRecognizers.Add(tap);

            ApplyPlayerCustomizations();
            ResetGame();
            PlayBackgroundMusic();
        }

        void ToggleMute(object sender, EventArgs e)
        {
            isMuted = !isMuted;
            if (_backgroundMusic != null)
                _backgroundMusic.Volume = isMuted ? 0 : 0.2;

            MuteButton.Source = isMuted ? "mute.png" : "unmuted.png";
        }

        async void PlayBackgroundMusic()
        {
            if (_backgroundMusic != null)
                return;

            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("bg_music.mp3");
                _backgroundMusic = _audioManager.CreatePlayer(stream);
                _backgroundMusic.Loop = true;
                _backgroundMusic.Volume = isMuted ? 0 : 0.2;
                _backgroundMusic.Play();
            }
            catch { }
        }

        void StopMusic()
        {
            if (_backgroundMusic != null && _backgroundMusic.IsPlaying)
                _backgroundMusic.Pause();
        }

        void ResumeMusic()
        {
            if (_backgroundMusic != null && !_backgroundMusic.IsPlaying)
                _backgroundMusic.Play();
        }

        void ApplyPlayerCustomizations()
        {
            var player = PlayerData.Instance;
            jumpStrength = (player.OwnedPowers.Contains("ExtraPower") && player.PowerEnabled) ? -10 : -12;
        }

        void ResetGame()
        {
            gameRunning = false;
            score = 0;
            ScoreLabel.Text = "Pontos: 0";
            GameOverScreen.IsVisible = false;
            RemoveObjects();
            AbsoluteLayout.SetLayoutBounds(Pig, new Rect(100, 300, 70, 70));
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

            StartSpriteAnimation();
        }

        void StartSpriteAnimation()
        {
            spriteTimer?.Stop();
            spriteTimer = new System.Timers.Timer(150);
            spriteTimer.Elapsed += (s, e) =>
            {
                if (!gameRunning) return;
                spriteIndex++;
                if (spriteIndex > 4) spriteIndex = 1;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Pig.Source = $"pig_sprites0{spriteIndex}.png";
                });
            };
            spriteTimer.Start();
        }

        void RemoveObjects()
        {
            if (topPillar != null && GameLayout.Children.Contains(topPillar))
                GameLayout.Children.Remove(topPillar);
            if (bottomPillar != null && GameLayout.Children.Contains(bottomPillar))
                GameLayout.Children.Remove(bottomPillar);
            if (coin != null && GameLayout.Children.Contains(coin))
                GameLayout.Children.Remove(coin);

            topPillar = null;
            bottomPillar = null;
            coin = null;
        }

        void GameLoop(object? sender, ElapsedEventArgs e)
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

                if (newY <= 0 || newY + pigRect.Height >= GameLayout.Height || CheckCollision())
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
            double bottomHeight = Math.Max(100, canvasHeight - bottomY);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RemoveObjects();
                double startX = GameLayout.Width + 50;

                topPillar = new Image
                {
                    Source = "Pillar.png",
                    WidthRequest = pillarWidth,
                    HeightRequest = topHeight,
                    Aspect = Aspect.Fill,
                    Rotation = 180
                };

                bottomPillar = new Image
                {
                    Source = "Pillar.png",
                    WidthRequest = pillarWidth,
                    HeightRequest = bottomHeight,
                    Aspect = Aspect.Fill
                };

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
                double coinSize = 6 * coinValue;

                coin = new Image
                {
                    Source = "coin_frame_1.png",
                    WidthRequest = coinSize,
                    HeightRequest = coinSize,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    BindingContext = coinValue
                };

                var topBounds = AbsoluteLayout.GetLayoutBounds(topPillar);
                var bottomBounds = AbsoluteLayout.GetLayoutBounds(bottomPillar);
                double startX = GameLayout.Width + 100;
                double gapMiddle = topBounds.Height + (bottomBounds.Y - topBounds.Height) / 2;
                double offset = random.Next(-30, 30);
                double coinY = Math.Clamp(gapMiddle + offset, 50, GameLayout.Height - 60);

                AbsoluteLayout.SetLayoutBounds(coin, new Rect(startX, coinY, coinSize, coinSize));
                GameLayout.Children.Add(coin);

                StartCoinAnimation(coin);
            });
        }

        void StartCoinAnimation(Image coinImage)
        {
            int frame = 1;
            var coinTimer = new System.Timers.Timer(200);
            coinTimer.Elapsed += (s, e) =>
            {
                if (!gameRunning || coinImage == null)
                {
                    coinTimer.Stop();
                    return;
                }
                frame++;
                if (frame > 5) frame = 1;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    coinImage.Source = $"coin_frame_{frame}.png";
                });
            };
            coinTimer.Start();
        }

        void MoveObjects()
        {
            MovePillar(topPillar);
            MovePillar(bottomPillar);
            MoveCoin();

            if (topPillar != null)
            {
                var topRect = AbsoluteLayout.GetLayoutBounds(topPillar);
                if (topRect.X + topRect.Width < 0)
                {
                    score++;
                    ScoreLabel.Text = $"Pontos: {score}";
                    CreatePillars();
                    if (random.Next(0, 3) == 1) CreateCoin();
                }
            }
        }

        void MovePillar(VisualElement? pillar)
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
                if (GameLayout.Children.Contains(coin))
                    GameLayout.Children.Remove(coin);
                coin = null;
            }
        }

        void CheckCoinCollection()
        {
            if (coin == null) return;
            var pigRect = AbsoluteLayout.GetLayoutBounds(Pig);
            var coinRect = AbsoluteLayout.GetLayoutBounds(coin);

            if (pigRect.IntersectsWith(coinRect))
            {
                int coinValue = (int)coin.BindingContext;
                if (GameLayout.Children.Contains(coin))
                    GameLayout.Children.Remove(coin);

                coin = null;
                PlayerData.Instance.Coins += coinValue;
                PlayerData.Instance.Save();

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

            var pigRect = AbsoluteLayout.GetLayoutBounds(Pig);
            var topRect = AbsoluteLayout.GetLayoutBounds(topPillar);
            var bottomRect = AbsoluteLayout.GetLayoutBounds(bottomPillar);

            return pigRect.IntersectsWith(topRect) || pigRect.IntersectsWith(bottomRect);
        }

        void EndGame()
        {
            gameRunning = false;
            gameTimer?.Stop();
            spriteTimer?.Stop();
            StopMusic();

            if (score > PlayerData.Instance.HighScore)
                PlayerData.Instance.HighScore = score;

            PlayerData.Instance.Save();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (GameLayout.Children.Contains(GameOverScreen))
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
                ResumeMusic();
                StartGame();
                return;
            }

            pigVelocity = jumpStrength;
        }

        void RestartGame(object sender, EventArgs e)
        {
            ResumeMusic();
            ResetGame();
            StartGame();
        }

        async void GoBackToMainPage(object sender, EventArgs e)
        {
            gameTimer?.Stop();
            spriteTimer?.Stop();
            _backgroundMusic?.Stop();
            await Navigation.PopAsync();
        }
    }
}
