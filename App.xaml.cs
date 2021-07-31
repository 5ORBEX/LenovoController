using LenovoController.Providers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LenovoController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public Settings Settings { get; private set; }

        public static App Instance { get; private set; }

        private NotifyIcon notifyIcon;
        private MainWindow mainWindow;

        public void LoadSettings()
        {
            try
            {
                if (File.Exists("settings.ini"))
                {
                    var json = File.ReadAllText("settings.ini");
                    Settings newSettings = JsonConvert.DeserializeObject<Settings>(json);
                    if (newSettings != null)
                    {
                        Settings = newSettings;
                    }
                    else
                    {
                        Settings = new Settings();
                    }
                }
                else
                {
                    Settings = new Settings();
                }
            }
            catch
            {
                Settings = new Settings();
            }
        }

        public void SaveSettings()
        {
            if (Settings != null)
            {
                var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText("settings.ini", json);
            }
        }

        public bool CheckAutoStart()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                if (key != null)
                {
                    var names = key.GetValueNames();
                    return names.Any(name => name.Equals("LenovoController"));
                }

                return false;
            }
        }

        public void SetRunOnWindowsStartUp(bool autoStart)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (autoStart)
                {
                    Assembly currentAssembly = Assembly.GetExecutingAssembly();
                    key.SetValue("LenovoController", currentAssembly.Location);
                }
                else
                {
                    key.DeleteValue("LenovoController", false);
                }
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var errorText = e.Exception.ToString();
            Trace.TraceError(errorText);
            System.Windows.MessageBox.Show(errorText, "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Instance = this;

            var process = Process.GetProcessesByName("LenovoController");
            if (process.Count() > 1)
            {
                switch (Settings.Culture)
                {
                    case "RU":
                        System.Windows.MessageBox.Show("Другая копия приложения уже запущена. Закройте ее и попробуйте снова.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "UA":
                        System.Windows.MessageBox.Show("Інша копія програми уже запущена. Закрийте її та спробуйте ще раз.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        System.Windows.MessageBox.Show("Another copy of the application is already running. Close it and try again.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }

                Shutdown(-1);
            }

            LoadSettings();

            string exitText = "Exit";
            switch (Settings.Culture)
            {
                case "RU":
                    exitText = "Выход";
                    break;
                case "UA":
                    exitText = "Вихід";
                    break;
            }

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.FromHandle(LenovoController.Properties.Resources.LC.Handle);
            notifyIcon.Visible = true;
            notifyIcon.Text = "Lenovo Controller";
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Lenovo Controller", OnAppRun),
                new MenuItem(exitText, OnExitClick)
            });

            CreateMainDialog();
        }

        private void CreateMainDialog()
        {
            if (DriverProvider.ErrorShown)
            {
                return;
            }

            if (mainWindow == null)
            {
                mainWindow = new MainWindow(this);
            }

            if (!Settings.ShowOnStartup)
            {
                mainWindow.Hide();
            }
            else
            {
                mainWindow.ShowDialog();
            }
        }

        private void OnAppRun(object sender, EventArgs e)
        {
            if (DriverProvider.ErrorShown)
            {
                return;
            }

            if (mainWindow == null)
            {
                mainWindow = new MainWindow(this);
                mainWindow.ShowDialog();
            }
            else
            {
                if (mainWindow.Visibility != Visibility.Visible)
                {
                    mainWindow.ShowDialog();
                }
                else
                {
                    mainWindow.Activate();
                }
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            if (DriverProvider.ErrorShown)
            {
                return;
            }

            Shutdown(0);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon = null;
        }
    }
}