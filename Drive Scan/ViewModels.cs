using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Drive_Scan
{
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public struct FolderInfo
    {
        public string path {  get; set;  }
        public string name {  get; set;  }
        public long size  {  get; set;  }
        public string[] splitPath  {  get; set;  }
        public ObservableCollection<FolderInfo> subfolders  {  get; set;  }
        //public ObservableCollection<FolderInfo> Subfolders {    get { return subfolders; }    }

        public ObservableCollection<FileInfo> files  {  get; set;  }

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
        public FolderInfo GetSubFolderByPath(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateSubFolder(long size, string path)
        {
            subfolders.Add(new FolderInfo(size, path));
        }

        ///<summary>
        ///Gets a child of this folder by its name
        ///<summary>
        public FolderInfo GetSubFolderByName(string name)
        {
            //Crafty idiot proofing
            if (name == "con".ToUpper())
            {
                throw new InvalidOperationException("con isn't a real folder name");
            }

            //This is super jank, basically it relies on the fact that you can't name a folder "Con" on windows
            //Because FolderInfo is a struct not a class it is non nullable so i can't leave it un instantiated
            //So that when we return Con we know for certain that it didn't find a file and it isn't a real folder
            FolderInfo foundFolder = new FolderInfo(0, "Con");

            foreach (FolderInfo folder in subfolders)
            {
                if (folder.name == name)
                {
                    foundFolder = folder;
                }
            }
            
            return foundFolder;
        }

        public FileInfo GetFileByPath(string path)
        {
            throw new NotImplementedException();
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

            //For every chunk of the path except the last entry (filename)
            for (int i = 0; i < newFile.splitPath.Length-1; i++)
            {
                
                //Loop through all the existing folders to see if the one we're looking for already exists
                for (int j = 0; j <= currentFolder.subfolders.Count; j++)
                {
                    //Check if this is the right folder
                    if (currentFolder.subfolders.Count > 0 && currentFolder.subfolders[j].name == newFile.splitPath[i])
                    {
                        //If so set this as the current folder and break out of the loop, moving to the next part of the path
                        currentFolder = currentFolder.subfolders[j];
                        break;
                    }
                    //If this is the last folder and we didn't find a folder with the right name and this isn't the right folder already, create the folder
                    else if (j == currentFolder.subfolders.Count && newFile.splitPath[newFile.splitPath.Length-2] != currentFolder.name)
                    {
                        /* Add the needed folder by combining the currentFolder path and the folder we were looking for
                        Then set it as the current folder, then move on to the next part of the path */
                        currentFolder.subfolders.Add(new FolderInfo(0, $"{currentFolder.path}\\{newFile.splitPath[i]}"));
                        currentFolder = currentFolder.subfolders.Last();
                        break;
                    }
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

            //For every chunk of the path
            for (int i = 0; i < updatedFolderInfo.splitPath.Length; i++)
            {
                
                //Loop through all the existing folders to see if the one we're looking for already exists
                for (int j = 0; j <= currentFolder.subfolders.Count; j++)
                {
                    //Check if this is the right folder
                    if (currentFolder.subfolders.Count > 0 && currentFolder.subfolders[j].name == updatedFolderInfo.splitPath[i])
                    {
                        //If this is the last chunk of the target path (folderInfo.name)
                        if (i == updatedFolderInfo.splitPath.Length-1)
                        {
                            //Update the folder and return it
                            currentFolder.subfolders[j].ModifySize(updatedFolderInfo.size);
                            return currentFolder.subfolders[j];
                        }
                        //If so set this as the current folder and break out of the loop, moving to the next part of the path
                        currentFolder = currentFolder.subfolders[j];
                        break;
                    }
                    //If this is the last folder and we didn't find a folder with the right name and this isn't the right folder already, create the folder
                    else if (j == currentFolder.subfolders.Count && updatedFolderInfo.splitPath[updatedFolderInfo.splitPath.Length-1] != currentFolder.name)
                    {
                        //If this is the last chunk of the target path (folderInfo.name)
                        if (i == updatedFolderInfo.splitPath.Length-1)
                        {
                            //Update the folder and return it
                            currentFolder.subfolders[j].ModifySize(updatedFolderInfo.size);
                            return currentFolder.subfolders[j];
                        }
                        else 
                        {
                            /* Add the needed folder by combining the currentFolder path and the folder we were looking for
                            Then set it as the current folder, then move on to the next part of the path (set the new folder as currentfolder because our final destination is somewhere in this folder) */
                            currentFolder.subfolders.Add(new FolderInfo(0, $"{currentFolder.path}\\{updatedFolderInfo.splitPath[i]}"));
                            currentFolder = currentFolder.subfolders.Last();
                        }
                        
                        break;
                    }
                }

                Console.WriteLine("This shouldn't be happening");
            }

            // if (currentFolder.subfolders.Contains())
            // {
                
            // }

            // //Add the file to the folder
            //     currentFolder.subfolders.Add(folderInfo);

            return updatedFolderInfo;
        }
    }

    public struct FileInfo
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