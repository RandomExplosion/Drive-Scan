﻿using System.Collections.ObjectModel;
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

        //Runs on window open
        public DriveScanWindow()
        {
            InitializeComponent();
            ThemeSwitch(ConfigHandler.readValue("theme"));

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
        /// Scans a drive with the given name (Obtainable from SystemIO.DriveInfo)
        /// </summary>
        /// <param name="DriveName"></param>
        public void ScanDrive(string DriveName)
        {
            MessageBoxResult answer = MessageBox.Show("dOeS U wANtZ tU sCaN tHis dRiVE?", "DoeS U wANtZ tU sCaN tHis dRiVE?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                DriveInfo drive = DriveList.SelectedItem as DriveInfo;
                Console.WriteLine($"User is scanning drive: {drive.Name}{drive.VolumeLabel}");
                Scanning.DirectoryScanner.FindFiles(drive.Name, OnFileFound);
            }
        }

        /// <summary>
        /// Callback for ScanDrive()
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
                    scannedDrives.Where(x => x.name == foundFile.path.Split("\\")[foundFile.path.Split("\\").Length-2]).First().size = foundFile.size;
                }
                else //The size is 0 (this is the first time we have had this folder returned)
                {
                    scannedDrives.Add(new FolderInfo(foundFile.size,
                    //This part is also jank. it finds the last string in an array of strings that isn't null 
                    foundFile.path.Split("\\")[foundFile.path.Split("\\").Length-2]));
                }
            }
            else if (!foundFile.isFolder)
            {
                string rootName = foundFile.path.Split("\\").First();
                IEnumerable<FolderInfo> root = scannedDrives.Where(x => x.name == rootName);
                root.First().AddFileAtPath(foundFile.size, foundFile.path);
            }
            else 
            {
                string rootName = foundFile.path.Split("\\").First();
                IEnumerable<FolderInfo> root = scannedDrives.Where(x => x.name == rootName);
                root.First().UpdateFolder(foundFile.size, foundFile.path);
            }
        }

        #region Ribbon Buttons

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
        /// Reboot as administrator
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
