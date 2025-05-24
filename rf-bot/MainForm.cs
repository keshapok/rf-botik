using System.Drawing;
using System.Windows.Forms;

namespace bot
{
    public class MainForm : Form
    {
        private readonly BotEngine _bot;
        private readonly Bitmap _hpTemplate;

        public MainForm(BotEngine bot, Bitmap hpTemplate)
        {
            _bot = bot;
            _hpTemplate = hpTemplate;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Настройка формы
            Text = "RF Online Bot";
            Size = new Size(300, 150);

            // В конструктор MainForm
            var btnSettings = new Button {
                Text = "Settings",
                Location = new Point(220, 20),
                Size = new Size(100, 30)
            };
            btnSettings.Click += (s, e) => new SettingsForm().ShowDialog();
            Controls.Add(btnSettings);
            
            // Кнопки управления
            var btnStart = new Button { Text = "Start", Location = new Point(20, 20) };
            btnStart.Click += (s, e) => _bot.Start(_hpTemplate);
            
            var btnStop = new Button { Text = "Stop", Location = new Point(120, 20) };
            btnStop.Click += (s, e) => _bot.Stop();
            
            // Добавление элементов
            Controls.Add(btnStart);
            Controls.Add(btnStop);
        }
    }
}