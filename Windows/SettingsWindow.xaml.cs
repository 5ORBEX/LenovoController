using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
// using LenovoController.Resources;

namespace LenovoController
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private readonly App application;
        private bool darkMode;

        public bool DarkMode
        {
            get { return darkMode; }
            set
            {
                darkMode = value;
                OnPropertyChanged(nameof(DarkMode));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsWindow(IntPtr handle, App app)
        {
            InitializeComponent();
            DataContext = this;

            application = app;
            DarkMode = application.Settings.DarkMode;
            darkTheme.IsChecked = application.Settings.DarkMode;
            autoRun.IsChecked = app.CheckAutoStart();
            showOnStartup.IsChecked = app.Settings.ShowOnStartup;
            ChangeLanguage();

            applyPressed = false;
            var helper = new WindowInteropHelper(this);
            helper.Owner = handle;
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void ChangeLanguage()
        {
            switch (application.Settings.Culture)
            {
                case "RU":
                    langList.ItemsSource = new string[] { "Русский", "Украинский", "Английский" };
                    langList.ToolTip = "Выбор языка программы";
                    langList.SelectedIndex = 0;

                    showOnStartup.Content = "Отображать при запуске";
                    showOnStartup.ToolTip = "Отображать основное окно при запуске приложения автоматически";

                    darkTheme.Content = "Темная тема";
                    darkTheme.ToolTip = "Изменить графическую тему программы";

                    autoRun.Content = "Автозагрузка";
                    autoRun.ToolTip = "Запуск программы при загрузке Windows";

                    this.Title = " Настройки";
                    about.Text = "О программе";
                    authors.Text = "Авторы:";
                    btnApply.Content = "ПРИМЕНИТЬ";
                    btnCancel.Content = "ОТМЕНА";
                    break;

                case "UA":
                    langList.ItemsSource = new string[] { "Російська", "Українська", "Англійська" };
                    langList.ToolTip = "Вибір мови програми";
                    langList.SelectedIndex = 1;

                    showOnStartup.Content = "Відображати при запуску";
                    showOnStartup.ToolTip = "Відображати основне вікно при запуску програми автоматично";

                    darkTheme.Content = "Темна тема";
                    darkTheme.ToolTip = "Змінити графічну тему програми";

                    autoRun.Content = "Автозапуск";
                    autoRun.ToolTip = "Запуск програми при завантаженні Windows";

                    this.Title = " Налаштування";
                    about.Text = "Про програму";
                    authors.Text = "Автори:";
                    btnApply.Content = "ПРИМІНИТИ";
                    btnCancel.Content = "ВІДМІНА";
                    break;

                default:
                    langList.ItemsSource = new string[] { "Russian", "Ukrainian", "English" };
                    langList.ToolTip = "The program language selection";
                    langList.SelectedIndex = 2;

                    showOnStartup.Content = "Show on startup";
                    showOnStartup.ToolTip = "Show main window after start automatically";

                    darkTheme.Content = "Dark theme";
                    darkTheme.ToolTip = "Switch graphic theme of the program";

                    autoRun.Content = "Startup";
                    autoRun.ToolTip = "Run program on Windows startup";

                    this.Title = " Settings";
                    about.Text = "About";
                    authors.Text = "Authors:";
                    btnApply.Content = "APPLY";
                    btnCancel.Content = "CANCEL";
                    break;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var index = langList.SelectedIndex;
            string lang = null;

            switch (index)
            {
                case 0:
                    lang = "RU";
                    break;
                case 1:
                    lang = "UA";
                    break;
                case 2:
                    lang = "EN";
                    break;
            }

            applyPressed = true;
            application.Settings.Culture = lang;
            application.Settings.DarkMode = darkTheme?.IsChecked ?? false;
            application.Settings.ShowOnStartup = showOnStartup?.IsChecked ?? false;
            DarkMode = application.Settings.DarkMode;
            ChangeLanguage();
            application.SaveSettings();
            application.SetRunOnWindowsStartUp(autoRun?.IsChecked ?? false);
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = applyPressed;
        }

        private bool applyPressed;
    }
}
