using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using DrWPF.Windows.Data;
using LiveCharts;

namespace Drive_Scan
{
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public class FolderInfo : FileInfo
    {
        public ObservableDictionary<string, FileInfo> children { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_path"></param>
        public FolderInfo(long _size, string _path) : base(_size, _path)
        {
            this.children = new ObservableDictionary<string, FileInfo>();

            this.path = _path;
            this.size = _size;
            this.splitPath = path.Split("\\");
            this.name = this.splitPath.Last();
        }

        // public FileInfo GetFileByName(string name)
        // {
        //     return files.Where(file => file.name == name);
        // }

        /// <summary>
        /// Navigate down the tree of folders and create them if they arent there, then finally add the file when we get to the final folder
        /// </summary>
        /// <param name="Size"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public FileInfo AddFileAtPath(long Size, string Path)
        {

            FileInfo newFile = new FileInfo(Size, Path);

            FolderInfo currentFolder = this;

            //For every chunk of the path except the last entry (filename) and first entry (root)
            for (int i = 1; i < newFile.splitPath.Length-1; i++)
            {
                //If there is data on this folder already
                if (currentFolder.children.ContainsKey(newFile.splitPath[i]) && currentFolder.children[newFile.splitPath[i]].GetType().GetProperty("children") != null)
                {
                    currentFolder = currentFolder.children[newFile.splitPath[i]] as FolderInfo;
                }
            }

            //Add the file to the folder
            currentFolder.children.Add(newFile.name, newFile);

            return newFile;
        }

        public FolderInfo UpdateFolder(long Size, string Path)
        {

            FolderInfo updatedFolderInfo = new FolderInfo(Size, Path);

            FolderInfo currentFolder = this;

            //For every chunk of the path except the final (destination folder) and the first (root)
            for (int i = 1; i < updatedFolderInfo.splitPath.Length-1; i++)
            {
                
                //If the folder has already been created then go to that and move on to the next part of the path
                if (currentFolder.children.ContainsKey(updatedFolderInfo.splitPath[i]) && currentFolder.children[updatedFolderInfo.splitPath[i]].GetType().GetProperty("children") != null)
                {
                    currentFolder = currentFolder.children[updatedFolderInfo.splitPath[i]] as FolderInfo;
                    continue;
                }
                else
                {
                    //Create the folder
                    currentFolder.children.Add(updatedFolderInfo.splitPath[i], new FolderInfo(0, currentFolder.path + updatedFolderInfo.splitPath[i]));
                    continue;
                }
            }

            //If the folder already exists update its size
            if (currentFolder.children.ContainsKey(updatedFolderInfo.name) && currentFolder.children[updatedFolderInfo.name].GetType().GetProperty("children") != null)
            {
                ((FolderInfo)currentFolder.children[updatedFolderInfo.name]).size = updatedFolderInfo.size;
            }
            //Add the folder if it isn't already there
            else
            {
                currentFolder.children.Add(updatedFolderInfo.name, updatedFolderInfo);
            }

            return updatedFolderInfo;
        }
    }

    public class FileInfo
    {
        public string name { get; set; }
        public string path { get; set; }
        public string[] splitPath { get; set; }
        public long size { get; set; }

        /// <summary>
        /// ///Converts from File in Drive_Scan.Scanning to a FileInfo for use in the UI
        /// (Because File can be either a file or a folder wheras this is definitive)
        /// </summary>
        /// <param name="_size">The size of the file in bytes</param>
        /// <param name="_path">path</param>
        public FileInfo(long Size, string Path)
        {
            this.path = Path;
            this.size = Size;
            this.splitPath = path.Split("\\");
            this.name = this.splitPath.Last();
        }
    }

    public class SubfolderPieData<T> : ChartValues<T>
    {
        
    }
}