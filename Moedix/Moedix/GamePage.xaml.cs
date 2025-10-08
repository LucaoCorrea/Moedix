using Microsoft.Maui.Controls.Shapes;

namespace Moedix;

public partial class GamePage : ContentPage
{
    double gravity = 1.5;
    double jumpForce = -20;
    double pigY = 300;
    double velocityY = 0;
    double pipeSpeed = 5;
    double gapSize = 200;

    const double PigX = 100;
    const double PigSize = 50;
    const double GroundY = 650;

    int score = 0;
    bool isGameOver = false;
    bool isStarted = false;

    List<Rectangle> obstacles = new();
    IDispatcherTimer timer;
    Random rnd = new();

    public GamePage()
    {
        InitializeComponent();
    }

    // Início do jogo com dificuldade
    void StartGame(double gravityVal, double speed, double gap)
    {
        DifficultyMenu.IsVisible = false;
        gravity = gravityVal;
        pipeSpeed = speed;
        gapSize = gap;

        isStarted = true;
        StartGameLoop();

        GameLayout.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                if (!isGameOver)
                    velocityY = jumpForce;
            })
        });
    }

    void StartEasy(object sender, EventArgs e) => StartGame(1.2, 5, 220);
    void StartMedium(object sender, EventArgs e) => StartGame(1.6, 7, 200);
    void StartHard(object sender, EventArgs e) => StartGame(2.2, 9, 180);

    void StartGameLoop()
    {
        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(8.33); // ~120 FPS
        timer.Tick += (s, e) => UpdateGame();
        timer.Start();
    }

    void UpdateGame()
    {
        if (isGameOver) return;

        velocityY += gravity;
        pigY += velocityY;
        AbsoluteLayout.SetLayoutBounds(Pig, new Rect(PigX, pigY, PigSize, PigSize));

        // Gera obstáculos
        if (obstacles.Count == 0 || obstacles.Last().X < 400)
            CreateObstacle();

        // Move obstáculos e verifica colisão
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            var rect = obstacles[i];
            var bounds = AbsoluteLayout.GetLayoutBounds(rect);
            bounds.X -= pipeSpeed;

            if (bounds.X + bounds.Width < 0)
            {
                GameLayout.Children.Remove(rect);
                obstacles.RemoveAt(i);
                continue;
            }

            AbsoluteLayout.SetLayoutBounds(rect, bounds);

            // Verificando colisão com os obstáculos
            if (CheckCollision(bounds))
            {
                EndGame();
                return;
            }

            // Ganha ponto ao passar o obstáculo
            if (!isGameOver && bounds.X + bounds.Width < PigX && !rect.ClassId?.StartsWith("counted") == true)
            {
                rect.ClassId = "counted";
                score++;
                ScoreLabel.Text = $"Pontos: {score}";
            }
        }

        // Verificando colisão com o topo ou chão da tela
        if (pigY + PigSize >= GroundY || pigY < 0)
        {
            EndGame();
        }
    }

    // Verificação de colisão com os obstáculos
    bool CheckCollision(Rect obstacle)
    {
        var pigRect = new Rect(PigX, pigY, PigSize, PigSize);

        // Verificando colisão com o obstáculo superior
        if (pigRect.IntersectsWith(obstacle)) return true;

        // Obtendo o "bottomY" do obstáculo (posição do obstáculo inferior)
        var bottomY = obstacle.Y + gapSize;

        // Ajustando o valor de bottomY para fazer a colisão acontecer mais abaixo
        double offset = 30; // Ajuste esse valor para o quanto você quiser que a colisão ocorra mais embaixo
        bottomY += offset;

        // Verificando a colisão com a parte inferior do obstáculo (ajustado para ser mais embaixo)
        if (pigY + PigSize >= bottomY && pigY + PigSize <= bottomY + (800 - bottomY))
        {
            return true; // O porquinho colidiu com o obstáculo inferior
        }

        return false; // Nenhuma colisão
    }


    // Criando obstáculos
    void CreateObstacle()
    {
        double topHeight = rnd.Next(100, 400); // Altura aleatória para o topo
        double bottomY = topHeight + gapSize;  // Posição da base dos obstáculos

        var topPipe = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb("#808080")), // Cor do topo
            WidthRequest = 60,
            HeightRequest = topHeight
        };

        var bottomPipe = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb("#808080")), // Cor da base
            WidthRequest = 60,
            HeightRequest = 800 - bottomY
        };

        // Definindo posições iniciais dos obstáculos
        // Aqui, usamos AbsoluteLayout.SetLayoutBounds corretamente:
        AbsoluteLayout.SetLayoutBounds(topPipe, new Rect(800, 0, 60, topHeight));
        AbsoluteLayout.SetLayoutBounds(bottomPipe, new Rect(800, bottomY, 60, 800 - bottomY));

        // Adicionando obstáculos ao layout
        GameLayout.Children.Add(topPipe);
        GameLayout.Children.Add(bottomPipe);
        obstacles.Add(topPipe);
        obstacles.Add(bottomPipe);
    }

    void EndGame()
    {
        isGameOver = true;
        timer.Stop();
        GameOverLabel.IsVisible = true;
        RestartButton.IsVisible = true;
    }

    void RestartGame(object sender, EventArgs e)
    {
        Navigation.PushAsync(new GamePage());
    }
}
