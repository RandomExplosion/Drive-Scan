using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Drive_Scan
{
    /// <summary>
    /// Stores useful info on folders
    /// </summary>
    public class FolderInfo
    {
        public FolderInfo(long _size, string _path)
        {
            this.subfolders = new ObservableCollection<FolderInfo>();
        }

        public FolderInfo GetSubFolderByPath(string path)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///Gets a child of this folder by its name
        ///<summary>
        public FolderInfo GetSubFolderByName(string name)
        {
            return (FolderInfo)subfolders.Where(folder => folder.name == name);
        }

        public FileInfo GetFileByPath(string path)
        {
            throw new NotImplementedException();
        }

        public FileInfo GetFileByName(string name)
        {
            return (FileInfo)files.Where(file => file.name == name);
        }

        public string path;
        public string name;
        public long size;

        ObservableCollection<FolderInfo> subfolders;
        ObservableCollection<FileInfo> files;
    }

    public class FileInfo
    {
        public string name;
        public string path;
        public string[] splitPath;
        public long size;

        ///<summary>
        ///Converts from File in Drive_Scan.Scanning to a FileInfo for use in the UI
        ///(Because File can be either a file or a folder wheras this is definitive)
        ///<summary>
        public FileInfo (Scanning.File scanData)
        {
            size = scanData.size;
            splitPath = scanData.path.Split();
            name = splitPath[splitPath.Length-1];
        }
    }
}