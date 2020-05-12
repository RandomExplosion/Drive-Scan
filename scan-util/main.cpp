/* NOTE: this is in a very incomplete state! I still have to write comments */

#include <vector>
#include <string>
#include <stdio.h>
#include <iostream>
#include <stdlib.h> 
#include <windows.h>
#include <stdexcept>
#include <sys/stat.h>

int EndsWith(const char *str, const char *suffix)
{
    if (!str || !suffix)
    {
        return 0;
    }
    size_t lenstr = strlen(str);
    size_t lensuffix = strlen(suffix);
    if (lensuffix >  lenstr)
    {
        return 0;
    }
    return strncmp(str + lenstr - lensuffix, suffix, lensuffix) == 0;
}

size_t getSize(char file_name[])
{
    struct stat fs = { 0 };
    if (stat((const char*)file_name, &fs) == 0)
    {
        return fs.st_size;
    }
}

bool isDir(std::string const& dir_path)
{
    DWORD const f_attrib = GetFileAttributesA(dir_path.c_str());
    return f_attrib != INVALID_FILE_ATTRIBUTES && (f_attrib & FILE_ATTRIBUTE_DIRECTORY);
}

class File
{
    public:
    size_t size;
    bool isFolder;
    std::string path;
    std::string name;
    std::vector<File> contents;
    File(std::string p, std::string n, bool i)
    {   
        path = p;
        name = n;
        isFolder = i;

        if (isFolder)
        {
            size = 0;
            const char* temp = path.append("\\").c_str();
            if (!(EndsWith(temp, "\\.\\") || EndsWith(temp, "\\..\\"))) 
            {
                contents = File::getFiles(path.append("\\").c_str());
            }
        } 
        else 
        {
            char *cstr = new char[path.length() + 1];
            strcpy(cstr, path.c_str());
            size = getSize(cstr);
        }
    }

    std::string dump()
    {
        if (isFolder)
        {
            throw std::invalid_argument( "this can only be called on a file!" );
        } 
        else 
        {   
            return ""
        }
    }

    // get a vector of the files in a folder
    static std::vector<File> getFiles(const char* path) 
    {
        std::vector<File> files;
        WIN32_FIND_DATA data;
        // append * to end of path and get first file in folder
        HANDLE hFind = FindFirstFile(( std::string(path) + "*" ).c_str(), &data);

        if (hFind != INVALID_HANDLE_VALUE)
        {
            do
            {
                std::string temp(path);
                files.insert(files.begin(), File(( std::string(path) + std::string(data.cFileName) ), data.cFileName, isDir(temp.append(data.cFileName))));
            } while (FindNextFile(hFind, &data));
            FindClose(hFind);
        }

        return files;
    }
};

class FileSystem
{
    public:
    const char* path;
    std::vector<File> files;
    FileSystem(const char* p)
    {
        path = p;
        files = File::getFiles(path);
    }

    std::string dump()
    {
        return "todo";
    }
};

/*
// overload for std::cout to be able to show the File object
std::ostream &operator<<(std::ostream &os, const File& data)
{
    if (data.name != "." && data.name != "..")
    {
        if (data.isFolder) 
        {
            std::cout << "\"" << data.name << "\":{";
            for (File file : data.contents)
            {
                std::cout << file;
            }
            std::cout << "}";
        } 
        else
        {
            std::cout << ",";
            return os << "\"" << data.name << "\":" << data.size;
        }
    }
}
*/

int main()
{
    FileSystem files("K:\\Coding\\C++\\");

    std::cout << files.dump();

    return(0);
}