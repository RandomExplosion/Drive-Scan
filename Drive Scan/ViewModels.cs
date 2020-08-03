using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using LiveCharts;

namespace Drive_Scan
{
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public class FolderInfo : FileInfo
    {
        public ObservableCollection<FileInfo> children { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_path"></param>
        public FolderInfo(long _size, string _path) : base(_size, _path)
        {
            this.children = new ObservableCollection<FileInfo>();

            this.path = _path;
            this.size = _size;
            this.splitPath = path.Split("\\");
            this.name = this.splitPath.Last();
        }

        /// <summary>
        /// Not implimented
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        // public void CreateSubFolder(long size, string path)
        // {
        //     children.Add(new FolderInfo(size, path));
        // }

        ///<summary>
        ///Gets a child of this folder by its name
        ///<summary>
        public FolderInfo GetSubFolderByName(string _name)
        {
        
            //Find that folder
            FolderInfo foundFolder = null;
            foreach (FileInfo child in children)
            {
                if(child.name == _name)
                {
                    if(child.GetType().GetProperty("children") != null)
                    {
                        foundFolder = child as FolderInfo;
                    }
                }
            }
            
            return foundFolder;
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
                
                //If the folder has already been created then go to that and move on to the next part of the path
                if (currentFolder.GetSubFolderByName(newFile.splitPath[i]) != null)
                {
                    currentFolder = currentFolder.GetSubFolderByName(newFile.splitPath[i]);
                    continue;
                }
                else
                {
                    //Create the folder
                    currentFolder.children.Add(new FolderInfo(0, $"{currentFolder.path}\\{newFile.splitPath[i]}"));
                    continue;
                }
            
            }

            //Add the file to the folder
                currentFolder.children.Add(newFile);

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
                if (currentFolder.GetSubFolderByName(updatedFolderInfo.splitPath[i]) != null)
                {
                    currentFolder = currentFolder.GetSubFolderByName(updatedFolderInfo.splitPath[i]);
                    continue;
                }
                else
                {
                    //Create the folder
                    currentFolder.children.Add(new FolderInfo(0, currentFolder.path + updatedFolderInfo.splitPath[i]));
                    continue;
                }
            }

            // if (currentFolder.subfolders.Contains())
            // {
                
            // }

            FolderInfo existingFolder = currentFolder.GetSubFolderByName(updatedFolderInfo.name);

            //Update the folder's size if it isn't there
            if (existingFolder != null)
            {
                existingFolder.size = updatedFolderInfo.size;
            }
            else    //Add the folder if it isn't there for some reason
            {
                currentFolder.children.Add(updatedFolderInfo);
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