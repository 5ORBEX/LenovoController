using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32.SafeHandles;

namespace LenovoController.Providers
{
    public static class DriverProvider
    {
        private static SafeFileHandle _energyDriver;

        public static bool ErrorShown { get; set; }

        public static SafeFileHandle EnergyDriver
        {
            get
            {
                if (_energyDriver == null)
                {
                    var fileHandle = Native.CreateFileW("\\\\.\\EnergyDrv", 0xC0000000,
                        3u, IntPtr.Zero, 3u, 0x80, IntPtr.Zero);
                    if (fileHandle == new IntPtr(-1))
                    {
                        if (!ErrorShown)
                        {
                            ErrorShown = true;
                            _energyDriver = null;

                            switch (App.Instance.Settings.Culture)
                            {
                                case "RU":
                                    MessageBox.Show("Драйвер Lenovo Energy Management не найден. Установите драйвер и повторите попытку.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                case "UA":
                                    MessageBox.Show("Драйвер Lenovo Energy Management не знайдено. Встановіть драйвер і спробуйте ще раз.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                default:
                                    MessageBox.Show("Lenovo Energy Management driver is not found. Install driver and try again.", "Lenovo Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                            }

                            App.Instance.Shutdown(-1);
                        }
                    }

                    _energyDriver = new SafeFileHandle(fileHandle, true);
                }

                return _energyDriver;
            }
        }
    }
}