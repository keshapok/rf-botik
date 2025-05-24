public class SettingsForm : Form
{
    public SettingsForm()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Элементы для настройки разрешения
        var numWidth = new NumericUpDown() {
            Value = int.Parse(config["GameSettings:WindowWidth"]),
            Minimum = 800,
            Maximum = 1920
        };
        
        // Кнопка сохранения
        var btnSave = new Button() { Text = "Save" };
        btnSave.Click += (s, e) => {
            config["GameSettings:WindowWidth"] = numWidth.Value.ToString();
            File.WriteAllText("appsettings.json", config.ToString());
            Close();
        };
        
        // Добавление элементов на форму
        Controls.AddRange(new Control[] { numWidth, btnSave });
    }
}
