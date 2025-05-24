using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using bot.Core.Input;
using bot.Core.Vision;
using Microsoft.Extensions.Configuration;

namespace bot
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var config = BuildConfig();
                var gameSettings = config.GetSection("GameSettings");
                var botSettings = gameSettings.GetSection("BotSettings");

                // Инициализация
                var capturer = new GdiScreenCapturer(gameSettings["ProcessName"]);
                var detector = new OpenCvMovementDetector();
                var input = new WindowsInputSimulator();

                // Проверка разрешения
                var windowRect = capturer.GetGameWindowRect();
                if (windowRect.Width != int.Parse(gameSettings["WindowWidth"]) || 
                    windowRect.Height != int.Parse(gameSettings["WindowHeight"]))
                {
                    MessageBox.Show($"Требуется разрешение {gameSettings["WindowWidth"]}x{gameSettings["WindowHeight"]}", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Загрузка шаблона HP
                var hpTemplate = Properties.Resources.hp_bar_template;

                // Создание бота
                var bot = new BotEngine(
                    capturer,
                    detector,
                    input,
                    hpTemplate,
                    int.Parse(botSettings["AttackDelay"]),
                    int.Parse(botSettings["SearchDelay"]),
                    int.Parse(botSettings["CursorCheckDelay"]),
                    botSettings["LootKey"]);

                Application.Run(new MainForm(bot));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Critical Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }

    public class BotEngine
    {
        private readonly IScreenCapturer _capturer;
        private readonly IMovementDetector _detector;
        private readonly IInputSimulator _input;
        private readonly Bitmap _hpTemplate;
        private readonly int _attackDelay;
        private readonly int _searchDelay;
        private readonly int _cursorCheckDelay;
        private readonly string _lootKey;
        private CancellationTokenSource _cts;

        public BotEngine(
            IScreenCapturer capturer,
            IMovementDetector detector,
            IInputSimulator input,
            Bitmap hpTemplate,
            int attackDelay,
            int searchDelay,
            int cursorCheckDelay,
            string lootKey)
        {
            _capturer = capturer;
            _detector = detector;
            _input = input;
            _hpTemplate = hpTemplate;
            _attackDelay = attackDelay;
            _searchDelay = searchDelay;
            _cursorCheckDelay = cursorCheckDelay;
            _lootKey = lootKey;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => RunBotLoop(_cts.Token), _cts.Token);
        }

        public void Stop() => _cts?.Cancel();

        private async Task RunBotLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Run(() => ExecuteBotCycle(), token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Bot stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Bot error: {ex.Message}");
            }
        }

        private void ExecuteBotCycle()
        {
            try
            {
                // 1. Capture frames
                using var frame1 = _capturer.Capture();
                Thread.Sleep(_searchDelay);
                using var frame2 = _capturer.Capture();

                // 2. Find target
                var targetPos = _detector.FindLargestMovement(frame1, frame2);
                if (targetPos == null) return;

                // 3. Move and check cursor
                _input.MoveTo(targetPos.Value);
                Thread.Sleep(_cursorCheckDelay);

                if (_detector.IsCursorRed(targetPos.Value))
                {
                    _input.Click();

                    // 4. Check mob appeared
                    if (CheckMobAppeared())
                    {
                        AttackUntilDead();
                        Loot();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cycle error: {ex.Message}");
            }
        }

        private bool CheckMobAppeared()
        {
            using var currentFrame = _capturer.Capture();
            return _detector.TemplateMatch(currentFrame, _hpTemplate);
        }

        private void AttackUntilDead()
        {
            int attackCounter = 0;
            while (CheckMobAppeared() && !_cts.IsCancellationRequested)
            {
                _input.Click();
                Thread.Sleep(_attackDelay);

                if (++attackCounter % 6 == 0) _input.PressKey(Keys.F2);
                if (attackCounter % 13 == 0) _input.PressKey(Keys.F1);
            }
        }

        private void Loot()
        {
            if (!string.IsNullOrEmpty(_lootKey))
            {
                _input.PressKey((Keys)Enum.Parse(typeof(Keys), _lootKey));
                Thread.Sleep(1000);
            }
        }
    }
}