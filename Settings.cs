using System.Globalization;

namespace LenovoController
{
    public class Settings
    {
        public  bool DarkMode { get; set; }

        public string Culture { get; set; }

        public bool ShowOnStartup { get; set; }

        public Settings()
        {
            DarkMode = false;
            ShowOnStartup = true;
            Culture = "EN";

            var currentCulture = CultureInfo.CurrentCulture.ToString();
            switch (currentCulture)
            {
                case "ru":
                case "ru-BY":
                case "ru-KZ":
                case "ru-KG":
                case "ru-MD":
                case "ru-RU":
                case "ru-UA":
                    Culture = "RU";
                    break;
                case "uk":
                case "uk-UA":
                    Culture = "UA";
                    break;
            }
        }
    }
}
