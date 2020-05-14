using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using Config;
using ControlzEx.Theming;
using System.Collections.ObjectModel;
using Scanner;

namespace Drive_Scan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class DriveScanWindow : MetroWindow
    {

        //Singleton Pattern
        static DriveScanWindow currentWindow;

        // To create a console so we can see debug information
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        //Runs on window open
        public DriveScanWindow()
        {
            InitializeComponent();
            ThemeSwitch(ConfigHandler.readValue("theme"));
            // Show window console if debug mode is enabled
            if (Convert.ToBoolean(Convert.ToInt16(ConfigHandler.readValue("debug"))))
            {
                AllocConsole();
            }

            //Populate Drive List
            DriveList.ItemsSource = DriveInfo.GetDrives();
        }

        public void DriveList_Row_DoubleClick(object Sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("DoubleClicked on Row");
            MessageBox.Show("dOeS U wANtZ tU sCaN tHis dRiVE?");
        }

        #region Ribbon Buttons

        /// <summary> Switch current theme to specified theme and update config </summary>
        public void ThemeSwitch(dynamic Sender, RoutedEventArgs e = null)
        {
            // Get the name of the object the operation originated from 
            //                              (this will be the theme to switch to)
            string theme = Sender.GetType() != typeof(string)
                ? ((String)(Sender.GetType().GetProperty("Header")).GetValue(Sender, null)).ToLower()
                : Sender;
            
            if (theme == "dark")
            {
                ConfigHandler.updateValue("theme", "dark");
                ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue", false);
            }
            else if (theme == "light")
            {
                ConfigHandler.updateValue("theme", "light");
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Taupe", false);
            }
        }
        #endregion
    }

    #region XAML Display Converters
    /// <summary>
    /// Converts Byte Values into their simplest unit
    /// Source: https://thomasfreudenberg.com/archive/2017/01/21/presenting-byte-size-values-in-wpf/
    /// </summary>
    public class FormatSizeConverter : IValueConverter
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
            int cchBuf);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long number = System.Convert.ToInt64(value);
            StringBuilder    sb = new StringBuilder(32);
            StrFormatByteSizeW(number, sb, sb.Capacity);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public static object Convert(object value)
        {
            return new FormatSizeConverter().Convert(value, null, null, CultureInfo.CurrentCulture);
        }
    } 

    public class UsedDriveSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DriveInfo driveInfo = value as DriveInfo;
            long usedBytes = driveInfo.TotalSize - driveInfo.TotalFreeSpace;
            return FormatSizeConverter.Convert(usedBytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    #endregion

    #region XAML Data Containers
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public class FolderInfo : object
    {
        public FolderInfo(long _size, string _path)
        {
            this.subfolders = new ObservableCollection<FolderInfo>();
        }

        public string path;
        public long size;

        ObservableCollection<FolderInfo> subfolders;
        ObservableCollection<FileInfo> files;
    }

    public class FileInfo : object
    {
        public string path;
        public long size;
    }


    #endregion
}
