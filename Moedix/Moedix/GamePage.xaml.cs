using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace Moedix
{
    public partial class GamePage : ContentPage
    {
        double pigY;
        double velocity;
        double gravity = 0.5;
        bool isGameRunning = false;
        IDispatcherTimer timer;
        Random random = new();

        public GamePage()
        {
            InitializeComponent();
            pigY = 300;
            velocity = 0;
            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += OnGameTick;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapped;
            ((AbsoluteLayout)this.Content).GestureRecognizers.Add(tapGesture); 
        }

        void OnTapped(object sender, EventArgs e)
        {
            if (!isGameRunning)
            {
                StartGame();
            }
            else
            {
                velocity = -8;
            }
        }

        void StartGame()
        {
            isGameRunning = true;
            StartLabel.IsVisible = false;
            GameOverLabel.IsVisible = false;
            pigY = 300;
            velocity = 0;
            MoveColumnsToStart();
            timer.Start();
        }

        void EndGame()
        {
            isGameRunning = false;
            timer.Stop();
            GameOverLabel.IsVisible = true;
            StartLabel.IsVisible = true;
        }

        void OnGameTick(object sender, EventArgs e)
        {
            velocity += gravity;
            pigY += velocity;

            AbsoluteLayout.SetLayoutBounds(Pig, new Rect(100, pigY, 40, 40));

            MoveColumns();

            // Verifica colisões
            if (pigY > this.Height - 140 || pigY < 0 || CheckCollision())
            {
                EndGame();
            }
        }

        void MoveColumns()
        {
            var topBounds = AbsoluteLayout.GetLayoutBounds(TopColumn);
            var bottomBounds = AbsoluteLayout.GetLayoutBounds(BottomColumn);

            double newX = topBounds.X - 4;

            if (newX + topBounds.Width < 0)
            {
                newX = this.Width;
                int gapY = random.Next(150, 400);
                AbsoluteLayout.SetLayoutBounds(TopColumn, new Rect(newX, gapY - 300, 60, 200));
                AbsoluteLayout.SetLayoutBounds(BottomColumn, new Rect(newX, gapY + 150, 60, 200));
            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(TopColumn, new Rect(newX, topBounds.Y, topBounds.Width, topBounds.Height));
                AbsoluteLayout.SetLayoutBounds(BottomColumn, new Rect(newX, bottomBounds.Y, bottomBounds.Width, bottomBounds.Height));
            }
        }

        bool CheckCollision()
        {
            var pigRect = new Rect(100, pigY, 40, 40);
            var topRect = AbsoluteLayout.GetLayoutBounds(TopColumn);
            var bottomRect = AbsoluteLayout.GetLayoutBounds(BottomColumn);

            return pigRect.IntersectsWith(topRect) || pigRect.IntersectsWith(bottomRect);
        }

        void MoveColumnsToStart()
        {
            int gapY = random.Next(150, 400);
            AbsoluteLayout.SetLayoutBounds(TopColumn, new Rect(this.Width, gapY - 300, 60, 200));
            AbsoluteLayout.SetLayoutBounds(BottomColumn, new Rect(this.Width, gapY + 150, 60, 200));
        }
    }
}