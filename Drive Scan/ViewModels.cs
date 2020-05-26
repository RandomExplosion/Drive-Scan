using System.Security.Cryptography;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Drive_Scan
{
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public class FolderInfo
    {
        public string path;
        public string name;
        public long size;
        public string[] splitPath;
        public ObservableCollection<FolderInfo> subfolders;
        //public ObservableCollection<FolderInfo> Subfolders {    get { return subfolders; }    }

        public ObservableCollection<FileInfo> files;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_path"></param>
        public FolderInfo(long _size, string _path)
        {
            this.subfolders = new ObservableCollection<FolderInfo>();
            this.files = new ObservableCollection<FileInfo>();

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
        public void CreateSubFolder(long size, string path)
        {
            subfolders.Add(new FolderInfo(size, path));
        }

        ///<summary>
        ///Gets a child of this folder by its name
        ///<summary>
        public FolderInfo GetSubFolderByName(string _name)
        {
            // //Crafty idiot proofing
            // if (name == "con".ToUpper())
            // {
            //     throw new InvalidOperationException("con isn't a real folder name");
            // }

            // //This is super jank, basically it relies on the fact that you can't name a folder "Con" on windows
            // //Because FolderInfo is a struct not a class it is non nullable so i can't leave it un instantiated
            // //So that when we return Con we know for certain that it didn't find a file and it isn't a real folder
            // FolderInfo foundFolder = new FolderInfo(0, "Con");

            // foreach (FolderInfo folder in subfolders)
            // {
            //     if (folder.name == name)
            //     {
            //         foundFolder = folder;
            //     }
            // }

            //Find that folder
            FolderInfo foundFolder = subfolders.ToList().Find(x => x.name == name);
            
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
                    currentFolder.subfolders.Add(new FolderInfo(0, currentFolder.path + newFile.splitPath[i]));
                    continue;
                }
            
            }

            //Add the file to the folder
                currentFolder.files.Add(newFile);

            return newFile;
        }

        public void ModifySize(long newSize)
        {
            size = newSize;
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
                    currentFolder.subfolders.Add(new FolderInfo(0, currentFolder.path + updatedFolderInfo.splitPath[i]));
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
                currentFolder.subfolders.Add(updatedFolderInfo);
            }

            return updatedFolderInfo;
        }
    }

    public class FileInfo
    {
        public string name;
        public string path;
        public string[] splitPath;
        public long size;

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
}