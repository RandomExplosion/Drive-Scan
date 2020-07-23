using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Collections.Generic;

namespace Drive_Scan
{
    namespace Config
    {
        public class ScanLoader
        {
            public List<Scanning.File> files = new List<Scanning.File>();

            /// <summary>
            /// Save the data in the files object to the specified path
            /// </summary>
            public void Save(string path)
            {
                // This will contain the uncompressed data that we are going to dump into the output file
                List<string> data = new List<string>();

                foreach (Scanning.File file in files)
                {
                    // We use a | to split the parameters since windows will not allow that character to appear in file names
                    //  We also convert the bools to be either "1" or "0" to save space
                    data.Add($"{file.path}|{file.size}|{(file.isFolder ? 1 : 0)}|{(file.isFirstRoot ? 1 : 0)}");
                }

                // We then use a * as the line seperator since that is another character windows will not allow and compress the resulting string
                string compressed = StringCompressor.CompressString(String.Join("*", data));

                // Create the output file    
                using (FileStream fs = File.Create(path))     
                {    
                    // Write the data to the file
                    byte[] raw = new UTF8Encoding(true).GetBytes(compressed);    
                    fs.Write(raw, 0, raw.Length);
                }    
            }

            /// <summary>
            /// Load the data contained in a file at the specified path
            /// </summary>
            public void Load(string path)
            {
                // Read the data
                byte[] raw = File.ReadAllBytes(path);
                // Re-encode the text to utf8
                string compressed = Encoding.UTF8.GetString(raw);
                // Uncompress the file and split the line delimeter
                string[] data = StringCompressor.DecompressString(compressed).Split("*");

                // Clear the file array
                files.Clear();

                // Loop through the lines and re-construct each file
                foreach (string file in data)
                {   
                    string[] chunks = file.Split("|");
                    // Convert each chunk back into the original type
                    files.Add(new Scanning.File(long.Parse(chunks[1]), chunks[0], Convert.ToBoolean(Convert.ToInt16(chunks[2])), Convert.ToBoolean(Convert.ToInt16(chunks[3]))));
                }
            }
        }


        internal static class StringCompressor
        {
            /// <summary>
            /// Compress a string
            /// </summary>
            public static string CompressString(string text)
            {
                // Convert the input text to bytes
                byte[] buffer = Encoding.UTF8.GetBytes(text);

                // Write the bytes to a gzip stream
                MemoryStream memoryStream = new MemoryStream();
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                }

                // Move the position of the memory stream back to the start
                memoryStream.Position = 0;

                // Create a byte array to hold the compressed data and write the stream to the data buffer
                byte[] compressedData = new byte[memoryStream.Length];
                memoryStream.Read(compressedData, 0, compressedData.Length);

                // Create the buffer to contain the gzip data and add an extra 4 bytes for the header
                byte[] gZipBuffer = new byte[compressedData.Length + 4];

                // Copy the data into the buffer
                Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);

                // Convert the data to base64 and return it
                return Convert.ToBase64String(gZipBuffer);
            }

            /// <summary>
            /// Decompress a string
            /// </summary>
            public static string DecompressString(string compressedText)
            {
                // Convert the input text from a base64 string to a byte array 
                byte[] gZipBuffer = Convert.FromBase64String(compressedText);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Get the length of the input data and read from the 4th byte to the end so we don't get the header as well
                    int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                    memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                    // Create the buffer to store the decompressed data
                    byte[] buffer = new byte[dataLength];

                    // Move the memory stream position back to 0
                    memoryStream.Position = 0;

                    // Decompress the memory stream and write it into the buffer
                    using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        gZipStream.Read(buffer, 0, buffer.Length);
                    }

                    // Convert it to a utf8 string and return
                    return Encoding.UTF8.GetString(buffer);
                }
            }
        }
    }
}
