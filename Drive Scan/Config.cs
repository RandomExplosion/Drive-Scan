using System;
using System.IO;  
using System.Text;
using System.Collections.Generic;

namespace Drive_Scan
{
    namespace Config
    {
        public static class ConfigHandler
        {
            // Store config in %AppData%\BeanCat\Drive Scan\Config.ini
            private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"BeanCat\Drive Scan\config.ini");

            /// <summary> Read the config file and return the entire file as a dictionary
            ///                      (will also create defaults if file not found) </summary>
            private static Dictionary<string, string> readConfig()
            {
                // If config file not found create default file
                if (!File.Exists(path))
                {
                    // Create parent folders
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    // Add default settings
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("theme=dark");
                        sw.WriteLine("debug=1");
                        sw.WriteLine("hidden=0");
                    }
                }

                Dictionary<string, string> data = new Dictionary<string, string>();
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Extract text before and after = sign (key value pairs)
                        int index = line.IndexOf("=");
                        data.Add(line.Substring(0, index), line.Substring(index + 1));
                    }
                }

                return data;
            }

            /// <summary> Read a value by key from the config </summary>
            public static string readValue(string key)
            {
                Dictionary<string, string> data = readConfig();
                // Ensure the key actually exists before we return it
                if (data.ContainsKey(key)) return data[key];

                // If it doesn't then we re-create the file (this resets the config to the defaults)
                File.Delete(path);
                // Now that the file is deleted readConfig will automatically re-create it with the correct keys
                return readConfig()[key];
            }

            /// <summary> Update a value by key from the config </summary>
            public static void updateValue(string key, string value)
            {
                Dictionary<string, string> data = readConfig();
                // Update value in local instance of data
                //   (this also has the cool side effect of creating it if it does not exist)
                data[key] = value;

                StringBuilder sb = new StringBuilder();
                // For each key-value pair in the data, append the valid key=value to the string builder 
                foreach (string item in data.Keys)
                {
                    sb.Append($"{item}={data[item]}{Environment.NewLine}");
                }
                // Write the updated config to the file, overwriting what was originally there
                File.WriteAllText(path, sb.ToString());
            }
        }
    }
}