using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using LenovoController.Features;

namespace LenovoController
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly RadioButton[] _alwaysOnUsbButtons;
        private readonly AlwaysOnUsbFeature _alwaysOnUsbFeature = new AlwaysOnUsbFeature();
        private readonly RadioButton[] _batteryButtons;
        private readonly BatteryFeature _batteryFeature = new BatteryFeature();
        private readonly RadioButton[] _powerModeButtons;
        private readonly PowerModeFeature _powerModeFeature = new PowerModeFeature();
        private readonly FnLockFeature _fnLockFeature = new FnLockFeature();
        private readonly OverDriveFeature _overDriveFeature = new OverDriveFeature();
        private readonly TouchpadLockFeature _touchpadLockFeature = new TouchpadLockFeature();
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

        public MainWindow(App app)
        {
            InitializeComponent();
            DataContext = this;

            application = app;
            DarkMode = application.Settings.DarkMode;
            ChangeLanguage();

            _powerModeButtons = new[] { radioQuiet, radioBalance, radioPerformance };
            _batteryButtons = new[] { radioConservation, radioNormalCharge, radioRapidCharge };
            _alwaysOnUsbButtons = new[] { radioAlwaysOnUsbOff, radioAlwaysOnUsbOnWhenSleeping, radioAlwaysOnUsbOnAlways };
            Refresh();
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private class FeatureCheck
        {
            private readonly Action _check;
            private readonly Action _disable;

            internal FeatureCheck(Action check, Action disable)
            {
                _check = check;
                _disable = disable;
            }

            internal void Check() => _check();
            internal void Disable() => _disable();
        }

        private void Refresh()
        {
            var features = new[]
            {
                new FeatureCheck(
                    () => _powerModeButtons[(int) _powerModeFeature.GetState()].IsChecked = true,
                    () => DisableControls(_powerModeButtons)),
                new FeatureCheck(
                    () => _batteryButtons[(int) _batteryFeature.GetState()].IsChecked = true,
                    () => DisableControls(_batteryButtons)),
                new FeatureCheck(
                    () => _alwaysOnUsbButtons[(int) _alwaysOnUsbFeature.GetState()].IsChecked = true,
                    () => DisableControls(_alwaysOnUsbButtons)),
                new FeatureCheck(
                    () => chkOverDrive.IsChecked = _overDriveFeature.GetState() == OverDriveState.On,
                    () => chkOverDrive.IsEnabled = false),
                new FeatureCheck(
                    () => chkTouchpadLock.IsChecked = _touchpadLockFeature.GetState() == TouchpadLockState.Off,
                    () => chkTouchpadLock.IsEnabled = false),
                new FeatureCheck(
                    () => chkFnLock.IsChecked = _fnLockFeature.GetState() == FnLockState.On,
                    () => chkFnLock.IsEnabled = false)
            };

            foreach (var feature in features)
            {
                try
                {
                    feature.Check();
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("Could not refresh feature: " + e);
                    feature.Disable();
                }
            }
        }

        private void DisableControls(Control[] buttons)
        {
            foreach (var btn in buttons)
                btn.IsEnabled = false;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void radioPowerMode_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _powerModeFeature.GetState();
            var newState = (PowerModeState)Array.IndexOf(_powerModeButtons, sender);
            if (oldState != newState)
            {
                _powerModeFeature.SetState(newState);
            }
        }

        private void radioBattery_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _batteryFeature.GetState();
            var newState = (BatteryState)Array.IndexOf(_batteryButtons, sender);
            if (oldState != newState)
            {
                _batteryFeature.SetState(newState);
            }
        }

        private void radioAlwaysOnUsb_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _alwaysOnUsbFeature.GetState();
            var newState = (AlwaysOnUsbState)Array.IndexOf(_alwaysOnUsbButtons, sender);
            if (oldState != newState)
            {
                _alwaysOnUsbFeature.SetState(newState);
            }
        }

        private void chkOverDrive_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _overDriveFeature.GetState();
            var newState = chkOverDrive.IsChecked.GetValueOrDefault(false)
                ? OverDriveState.On
                : OverDriveState.Off;
            if (oldState != newState)
            {
                _overDriveFeature.SetState(newState);
            }
        }

        private void chkTouchpadLock_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _touchpadLockFeature.GetState();
            var newState = chkTouchpadLock.IsChecked.GetValueOrDefault(false)
                ? TouchpadLockState.Off
                : TouchpadLockState.On;
            if (oldState != newState)
            {
                _touchpadLockFeature.SetState(newState);
            }
        }

        private void chkFnLock_Checked(object sender, RoutedEventArgs e)
        {
            var oldState = _fnLockFeature.GetState();
            var newState = chkFnLock.IsChecked.GetValueOrDefault(false)
                ? FnLockState.On
                : FnLockState.Off;
            if (oldState != newState)
            {
                _fnLockFeature.SetState(newState);
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            IntPtr handle = helper.EnsureHandle();

            var settingsDlg = new SettingsWindow(handle, application);
            settingsDlg.ShowDialog();
            if (settingsDlg.DialogResult != null && settingsDlg.DialogResult == true)
            {
                DarkMode = application.Settings.DarkMode;
                ChangeLanguage();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            application.Shutdown(0);
        }

        private void ChangeLanguage()
        {
            switch (application.Settings.Culture)
            {
                case "RU":
                    batteryGroup.Text = "ЗАРЯДКА БАТАРЕИ";
                    radioConservation.Content = "Сбережение";
                    radioConservation.ToolTip = "Режим, при котором батарея не будет заряжаться выше 60% для сбережения ее ресурса";
                    radioNormalCharge.Content = "Нормальная";
                    radioNormalCharge.ToolTip = "Режим, при котором батарея будет заряжаться до 100% как обычно";
                    radioRapidCharge.Content = "Быстрая";
                    radioRapidCharge.ToolTip = "Режим, при котором батарея будет заряжаться до 100% более быстро. Может вызывать повышенное нагревание блока питания";

                    powerModeGroup.Text = "РЕЖИМ РАБОТЫ";
                    radioQuiet.Content = "Тихий";
                    radioQuiet.ToolTip = "Тихий режим работы ноутбука, который ограничивает мощность процессора и видеокарты. В этом режиме система охлаждения работает относительно тихо";
                    radioBalance.Content = "Автоматический";
                    radioBalance.ToolTip = "Оптимальний режим работы ноутбука, которого хватает для большинства задач. В этом режиме мощность процессора и видеокарты регулируются автоматически";
                    radioPerformance.Content = "Производительный";
                    radioPerformance.ToolTip = "Производительный режим работы ноутбука, в котором мощность процессора и видеокарты максимальная. В этом режиме система охлаждения работает достаточно громко";

                    alwaysGroup.Text = "ALWAYS ON USB";
                    radioAlwaysOnUsbOff.Content = "Выкл.";
                    radioAlwaysOnUsbOff.ToolTip = "Опция, при включении которой напряжение на USB порты не подается";
                    radioAlwaysOnUsbOnWhenSleeping.Content = "Вкл. во время сна";
                    radioAlwaysOnUsbOnWhenSleeping.ToolTip = "Опция, при включении которой напряжение на USB порты подается в режиме сна";
                    radioAlwaysOnUsbOnAlways.Content = "Вкл. всегда";
                    radioAlwaysOnUsbOnAlways.ToolTip = "Опция, при включении которой напряжение на USB порты подается постоянно";

                    miscGroup.Text = "ДОПОЛНИТЕЛЬНЫЕ ОПЦИИ";
                    chkOverDrive.Content = "Вкл. Over Drive";
                    chkOverDrive.ToolTip = "Опция, при включении которой увеличивается скорость отклика монитора для повышения четкости быстро движущихся объектов";
                    chkFnLock.Content = "Вкл. FnLock";
                    chkFnLock.ToolTip = "Опция, при включении которой эмулируется постоянное нажатие клавиши Fn";
                    chkTouchpadLock.Content = "Вкл. сенс. панель";
                    chkTouchpadLock.ToolTip = "Опция для включения сенсорной панели";

                    btnRefresh.Content = "ОБНОВИТЬ";
                    btnRefresh.ToolTip = "Синхронизировать состояние настроек программы с настройками ноутбука";
                    btnSettings.ToolTip = "Настройки";
                    btnExit.Content = "ВЫХОД";
                    break;

                case "UA":
                    batteryGroup.Text = "ЗАРЯДКА БАТАРЕЇ";
                    radioConservation.Content = "Збереження";
                    radioConservation.ToolTip = "Режим, при якому батарея не буде заряджатися вище 60% для заощадження її ресурсу";
                    radioNormalCharge.Content = "Нормальна";
                    radioNormalCharge.ToolTip = "Режим, при якому батарея буде заряджатися до 100% як зазвичай";
                    radioRapidCharge.Content = "Швидка";
                    radioRapidCharge.ToolTip = "Режим, при якому батарея буде заряджатися до 100% швидше. Може викликати підвищене нагрівання блоку живлення";

                    powerModeGroup.Text = "РЕЖИМ РОБОТИ";
                    radioQuiet.Content = "Тихий";
                    radioQuiet.ToolTip = "Тихий режим роботи ноутбука, який обмежує потужність процесора і відеокарти. В цьому режимі система охолодження працює відносно тихо";
                    radioBalance.Content = "Автоматичний";
                    radioBalance.ToolTip = "Оптимальний режим роботи ноутбука, якого вистачає для більшості завдань. В цьому режимі потужність процесора і відеокарти регулюються автоматично";
                    radioPerformance.Content = "Продуктивний";
                    radioPerformance.ToolTip = "Продуктивний режим роботи ноутбука, в якому потужність процесора і відеокарти максимальна. В цьому режимі система охолодження працює досить голосно";

                    alwaysGroup.Text = "ALWAYS ON USB";
                    radioAlwaysOnUsbOff.Content = "Вимк.";
                    radioAlwaysOnUsbOff.ToolTip = "Опція, при увімкненні якої напруга на USB порти не подається";
                    radioAlwaysOnUsbOnWhenSleeping.Content = "Увімк. під час сну";
                    radioAlwaysOnUsbOnWhenSleeping.ToolTip = "Опція, при увімкненні якої напруга на USB порти подається в режимі сну";
                    radioAlwaysOnUsbOnAlways.Content = "Увімк. завжди";
                    radioAlwaysOnUsbOnAlways.ToolTip = "Опція, при увімкненні якої напруга на USB порти подається постійно";

                    miscGroup.Text = "ДОДАТКОВІ ОПЦІЇ";
                    chkOverDrive.Content = "Увімк. Over Drive";
                    chkOverDrive.ToolTip = "Опція, при увімкненні якої збільшується швидкість відгуку монітора для підвищення чіткості швидко рухомих об'єктів";
                    chkFnLock.Content = "Увімк. FnLock";
                    chkFnLock.ToolTip = "Опція, при увімкненні якої емулюється постійне натискання клавіші Fn";
                    chkTouchpadLock.Content = "Увімк. сенс. панель";
                    chkTouchpadLock.ToolTip = "Опція для увімкнення сенсорної панелі";

                    btnRefresh.Content = "ОНОВИТИ";
                    btnRefresh.ToolTip = "Синхронізувати стан налаштувань програми з налаштуваннями ноутбука";
                    btnSettings.ToolTip = "Налаштування";
                    btnExit.Content = "ВИХІД";
                    break;

                default:
                    batteryGroup.Text = "BATTERY CHARGE";
                    radioConservation.Content = "Conservation";
                    radioConservation.ToolTip = "A mode in which the battery will not be charged above 60% to conserve its resource";
                    radioNormalCharge.Content = "Normal";
                    radioNormalCharge.ToolTip = "A mode in which the battery will charge up to 100% as usual";
                    radioRapidCharge.Content = "Rapid Charge";
                    radioRapidCharge.ToolTip = "A mode in which the battery will charge up to 100% faster. May cause increased heating of the power supply";

                    powerModeGroup.Text = "POWER MODE";
                    radioQuiet.Content = "Quiet";
                    radioQuiet.ToolTip = "The quiet laptop mode which limits the power of the processor and video card. In this mode the cooling system is relatively quiet";
                    radioBalance.Content = "Balance";
                    radioBalance.ToolTip = "The optimal operating mode of the laptop which is sufficient for most tasks. In this mode the power of the processor and video card are automatically adjusted";
                    radioPerformance.Content = "Performance";
                    radioPerformance.ToolTip = "The productive mode of the laptop in which the power of the processor and video card is maximum. In this mode the cooling system works quite loudly";

                    alwaysGroup.Text = "ALWAYS ON USB";
                    radioAlwaysOnUsbOff.Content = "Off";
                    radioAlwaysOnUsbOff.ToolTip = "The voltage isn't supplied to the USB ports when this option is enabled";
                    radioAlwaysOnUsbOnWhenSleeping.Content = "On when sleeping";
                    radioAlwaysOnUsbOnWhenSleeping.ToolTip = "The voltage is supplied to the USB ports in sleep mode when this option is enabled";
                    radioAlwaysOnUsbOnAlways.Content = "On always";
                    radioAlwaysOnUsbOnAlways.ToolTip = "The voltage is supplied to the USB ports always when this option is enabled";

                    miscGroup.Text = "ADDITIONAL OPTIONS";
                    chkOverDrive.Content = "On Over Drive";
                    chkOverDrive.ToolTip = "An option that increases the response speed of the monitor to increase the clarity of fast moving objects when enabled";
                    chkFnLock.Content = "On FnLock";
                    chkFnLock.ToolTip = "An option that emulates the constant pressing of the Fn key when enabled";
                    chkTouchpadLock.Content = "On touchpad";
                    chkTouchpadLock.ToolTip = "An option to enable the touchpad";

                    btnRefresh.Content = "REFRESH";
                    btnRefresh.ToolTip = "Synchronize the settings of the program with the settings of the laptop";
                    btnSettings.ToolTip = "Settings";
                    btnExit.Content = "EXIT";
                    break;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}