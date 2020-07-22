using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.IO;
using ControlzEx.Theming;
using System.Diagnostics;
using Drive_Scan.Config;
using System.Collections.Generic;
using MahApps.Metro.Controls.Dialogs;

namespace Drive_Scan
{
    /// <summary>
    /// Code Behind DriveScan.xaml
    /// </summary>
    public partial class DriveScanWindow : MetroWindow
    {

        //Singleton Pattern
        static DriveScanWindow currentWindow;

        //Viewmodel for scanned folders and paths
        public ObservableCollection<FolderInfo> scannedDrives;

        //The folder in the DirectoryTree that the user has selected (Contents are shown in top right)
        //Can also be changed from top right
        public FolderInfo selectedFolder; 

        static AsyncLocal<FolderInfo> _workingTree = new AsyncLocal<FolderInfo>();

        //Runs on window open
        public DriveScanWindow()
        {
            InitializeComponent();
            ThemeSwitch(ConfigHandler.readValue("theme"));
            HiddenFiles.IsChecked = Convert.ToBoolean(Convert.ToInt16(ConfigHandler.readValue("hidden")));

            currentWindow = this;

            //Populate Drive List
            DriveList.ItemsSource = DriveInfo.GetDrives();

            //Init scanned Drives list
            scannedDrives = new ObservableCollection<FolderInfo>();

            //Populate Dir Tree
            DirectoryTree.ItemsSource = scannedDrives;
        }

#region Scan File Command
        /// <summary>
        /// Start scanning the selected drive
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void ScanDrive_CommandBinding_Executed(object target, ExecutedRoutedEventArgs e)
        {
            //DriveInfo info = e.Parameter as DriveInfo;
            DriveInfo info = DriveList.SelectedItem as DriveInfo;
            ScanDrive(info.Name);
        }

        /// <summary>
        /// Checks if the conditions are right to start a drive scan (has the user selected a drive)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanDrive_CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DriveList != null)
            {
                if (DriveList.SelectedItem != null)
                {
                    e.CanExecute = true;
                }
            }
        }
#endregion

        /// <summary>
        /// Updates selectedFolder when the user double clicks on a new folder in the DirectoryTree
        /// (So that the Folder Contents View can update)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDirectoryTreeItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Get selected item from DirectoryTree
            FileInfo item = DirectoryTree.SelectedItem as FileInfo;

            //Get attributes for the selected item
            FileAttributes attr = File.GetAttributes(item.path);

            //If the selected item is a folder
            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Set the selected Folder to the selected item
                selectedFolder = DirectoryTree.SelectedItem as FolderInfo;
                
                //Update the FolderContentsView
                FolderContentsView.ItemsSource = selectedFolder.children;
            }

            //This is a file
            else
            {
                try
                {
                    //Open it in its associated application
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(item.path)
                    { 
                        UseShellExecute = true
                    };
                    p.Start();
                }
                catch (Exception)
                {
                    //User most likely attempted to open a file without an assigned application
                    //So open it with the 'how do you want to open this file' dialog
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(item.path)
                    {
                        WindowStyle = ProcessWindowStyle.Normal,
                        Verb = "openas",
                        UseShellExecute = true,
                        ErrorDialog = true
                    };
                    p.Start();
                }
            }
        }

        /// <summary>
        /// Updates selectedFolder when the user double clicks on a new folder in the FolderContentsView
        /// (So that the Folder Contents View can update)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnFolderContentsViewItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Get selected item from FolderContentsView
            FileInfo item = FolderContentsView.SelectedItem as FileInfo;

            //Get attributes for the selected item
            FileAttributes attr = File.GetAttributes(item.path);

            //If the selected item is a folder
            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Set the selected Folder to the selected item
                selectedFolder = FolderContentsView.SelectedItem as FolderInfo;
                
                //Update the FolderContentsView
                FolderContentsView.ItemsSource = selectedFolder.children;
            }

            //This is a file
            else
            {
                try
                {
                    //Open it in its associated application
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(item.path)
                    { 
                        UseShellExecute = true
                    };
                    p.Start();
                }
                catch (Exception)
                {
                    //User most likely attempted to open a file without an assigned application
                    //So open it with the 'how do you want to open this file' dialog
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(item.path)
                    {
                        WindowStyle = ProcessWindowStyle.Normal,
                        FileName = item.name,
                        Verb = "openas",
                        UseShellExecute = true,
                        ErrorDialog = true
                    };
                    p.Start();
                }
            }
        }


        /// <summary>
        /// Scans a drive with the given name (Obtainable from SystemIO.DriveInfo)
        /// </summary>
        /// <param name="DriveName"></param>
        public async void ScanDrive(string DriveName)
        {
            
            // Prompt the user for if they actually wanted to scan this drive
            MessageDialogResult res = await ((MetroWindow)(Application.Current.MainWindow)).ShowMessageAsync("Confirm", $"Are you sure you want to scan this drive? ({DriveName})", MessageDialogStyle.AffirmativeAndNegative);
            
            // If they say yes
            if (res == MessageDialogResult.Affirmative) 
            { 
                DriveInfo drive = DriveList.SelectedItem as DriveInfo;
                Console.WriteLine($"User is scanning drive: {drive.Name}{drive.VolumeLabel}");
                //Show the Bar
                ProgBar.Visibility = Visibility.Visible;
                //Scan the drive asynchronously then add the drive tree to the TreeView
                Task scanTask = Task.Run(() => {Scanning.DirectoryScanner.FindFiles(drive.Name, OnFileFound);
                
                    //Add Drive tree to ui when finished then Release Working Resources (deallocate ram from _workingTree) when scan is finished
                    Application.Current.Dispatcher.Invoke( () => 
                    {
                        scannedDrives.Add(_workingTree.Value);  //Add drive to tree
                        ProgBar.Visibility = Visibility.Hidden; //Hide Progress Bar
                    });
                
                });
            }
        }

        /// <summary>
        /// Callback for ScanDrive() (is run once for every file and folder on the drive (including the root of the folder))
        /// </summary>
        /// <param name="foundFile"></param>
        /// <param name="isFirstFile"></param>
        /// <param name="isRoot"></param>
        public void OnFileFound(Scanning.File foundFile, bool isFirstFile, bool isRoot)
        {
            //Console.WriteLine(foundFile);

            //If this is the root folder
            if (isRoot && isFirstFile)
            {
                //If this is the second time this drive's root has been retuned (has the final size)
                if (foundFile.size > 0)
                {   
                    //Update the size
                    _workingTree.Value.size = foundFile.size;
                }
                else //The size is 0 (this is the first time we have had this folder returned)
                {

                    //Initialise the working tree
                    _workingTree.Value = new FolderInfo(foundFile.size,
                    //This part is also jank. it finds the last string in an array of strings that isn't null 
                    foundFile.path.Split("\\")[foundFile.path.Split("\\").Length-2]);
                    
                }
            }
            else if (!foundFile.isFolder)
            {
                //It's a file so add it
                _workingTree.Value.AddFileAtPath(foundFile.size, foundFile.path);
            }
            else 
            {   
                //It's a folder so add it or update its size if it already exists
                _workingTree.Value.UpdateFolder(foundFile.size, foundFile.path); 
            }
        }

        #region Ribbon Buttons

        /// <summary>
        /// Update the hidden file config from the checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateHiddenFiles(object sender, RoutedEventArgs e)
        {
            ConfigHandler.updateValue("hidden", HiddenFiles.IsChecked ? "1" : "0");
        }

        /// <summary>
        /// Switch current theme to specified theme and update config
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        public void ThemeSwitch(dynamic Sender, RoutedEventArgs e = null)
        {
            //Get the name of the object the operation originated from
            //                             (this will be the theme to switch to)
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

        /// <summary>
        /// Reboot as Administrator
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        public void GetAdmin(object Sender, RoutedEventArgs e)
        {
            //Get application exe location
            string path = Process.GetCurrentProcess().MainModule.FileName;

            //Setup process info to have admin
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                //Start new instance
                Process.Start(psi);
                //Close original instance
                this.Close();
            }
            //Catch user cancelling UAC prompt
            catch (System.ComponentModel.Win32Exception) {}
        }
        #endregion
    }

}
