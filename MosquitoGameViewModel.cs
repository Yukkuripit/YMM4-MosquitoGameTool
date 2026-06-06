using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;

namespace MosquitoGame
{
    // ハイスコア保存用データ
    public class HighScoreData
    {
        public int Score { get; set; }
        public DateTime Date { get; set; }
    }

    public class MosquitoGameViewModel : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        private bool _isGameRunning;
        private int _score;
        private int _timeLeft;
        private readonly DispatcherTimer _gameTimer;
        private readonly DispatcherTimer _mosquitoTimer;

        private double _mosquitoX = 100;
        private double _mosquitoY = 100;
        private double _velocityX = 3;
        private double _velocityY = 2;
        private double _canvasWidth = 600;
        private double _canvasHeight = 400;

        private HighScoreData? _highScore;
        private readonly string _highScoreFilePath;

        private string _scoreText = "スコア: 0";
        public string ScoreText
        {
            get => _scoreText;
            private set { _scoreText = value; OnPropertyChanged(); }
        }

        private string _timeText = "残り時間: 30";
        public string TimeText
        {
            get => _timeText;
            private set { _timeText = value; OnPropertyChanged(); }
        }

        private string _highScoreText = "ハイスコア: なし";
        public string HighScoreText
        {
            get => _highScoreText;
            private set { _highScoreText = value; OnPropertyChanged(); }
        }

        private string _gameStatusText = "「ゲーム開始」ボタンを押してね！";
        public string GameStatusText
        {
            get => _gameStatusText;
            private set { _gameStatusText = value; OnPropertyChanged(); }
        }

        public double MosquitoX
        {
            get => _mosquitoX;
            set { _mosquitoX = value; OnPropertyChanged(); }
        }
        public double MosquitoY
        {
            get => _mosquitoY;
            set { _mosquitoY = value; OnPropertyChanged(); }
        }

        public ICommand StartGameCommand { get; }
        public ICommand HitMosquitoCommand { get; }

        public MosquitoGameViewModel()
        {
            _highScoreFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MosquitoHighScore.json");
            LoadHighScore();

            StartGameCommand = new RelayCommand(_ => StartGame());
            HitMosquitoCommand = new RelayCommand(_ => HitMosquito());

            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _gameTimer.Tick += GameTimer_Tick;

            _mosquitoTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _mosquitoTimer.Tick += MosquitoTimer_Tick;
        }

        private void StartGame()
        {
            if (_isGameRunning) return;

            _score = 0;
            ScoreText = "スコア: 0";
            _timeLeft = 30;
            TimeText = "残り時間: 30";
            GameStatusText = "ゲーム中！蚊をクリック！";
            _isGameRunning = true;

            var rand = new Random();
            MosquitoX = rand.Next(50, (int)_canvasWidth - 50);
            MosquitoY = rand.Next(50, (int)_canvasHeight - 50);
            _velocityX = (rand.NextDouble() * 4 + 2) * (rand.Next(0, 2) == 0 ? 1 : -1);
            _velocityY = (rand.NextDouble() * 4 + 2) * (rand.Next(0, 2) == 0 ? 1 : -1);

            _gameTimer.Start();
            _mosquitoTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_timeLeft <= 0)
            {
                EndGame();
                return;
            }

            _timeLeft--;
            TimeText = $"残り時間: {_timeLeft}";
        }

        private void MosquitoTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isGameRunning) return;

            double newX = MosquitoX + _velocityX;
            double newY = MosquitoY + _velocityY;

            if (newX < 10 || newX > _canvasWidth - 50) _velocityX = -_velocityX;
            if (newY < 10 || newY > _canvasHeight - 50) _velocityY = -_velocityY;

            MosquitoX = Math.Clamp(newX, 10, _canvasWidth - 50);
            MosquitoY = Math.Clamp(newY, 10, _canvasHeight - 50);
        }

        private void HitMosquito()
        {
            if (!_isGameRunning) return;

            _score++;
            ScoreText = $"スコア: {_score}";

            var rand = new Random();
            MosquitoX = rand.Next(10, (int)_canvasWidth - 50);
            MosquitoY = rand.Next(10, (int)_canvasHeight - 50);
            _velocityX = (rand.NextDouble() * 4 + 2) * (rand.Next(0, 2) == 0 ? 1 : -1);
            _velocityY = (rand.NextDouble() * 4 + 2) * (rand.Next(0, 2) == 0 ? 1 : -1);

            System.Media.SystemSounds.Beep.Play();
        }

        private void EndGame()
        {
            _isGameRunning = false;
            _gameTimer.Stop();
            _mosquitoTimer.Stop();

            bool isNewHighScore = (_highScore == null || _score > _highScore.Score);
            if (isNewHighScore)
            {
                _highScore = new HighScoreData { Score = _score, Date = DateTime.Now };
                SaveHighScore();
                GameStatusText = $"ゲーム終了！ 新ハイスコア: {_score} 点！";
            }
            else
            {
                GameStatusText = $"ゲーム終了！ スコア: {_score} 点。ハイスコア: {_highScore?.Score} 点";
            }
            UpdateHighScoreDisplay();
        }

        private void LoadHighScore()
        {
            if (File.Exists(_highScoreFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_highScoreFilePath);
                    _highScore = JsonSerializer.Deserialize<HighScoreData>(json);
                }
                catch { _highScore = null; }
            }
            UpdateHighScoreDisplay();
        }

        private void SaveHighScore()
        {
            var json = JsonSerializer.Serialize(_highScore, _jsonOptions);
            File.WriteAllText(_highScoreFilePath, json);
        }

        private void UpdateHighScoreDisplay()
        {
            if (_highScore != null)
                HighScoreText = $"ハイスコア: {_highScore.Score} 点 ({_highScore.Date:yyyy/MM/dd HH:mm})";
            else
                HighScoreText = "ハイスコア: なし";
        }

        public void SetCanvasSize(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // RelayCommand (変更なし)
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}