using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ProjectTools
{
    public static class Tools
    {


        public static string ToTitleCase(string senetence) 
        {
            var textInfo = new CultureInfo("en-US").TextInfo;
            return textInfo.ToTitleCase(senetence);
        }

        /// <summary>
        /// Appends the time stamp to string.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string AppendTimeStamp(string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName),
                DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                Path.GetExtension(fileName)
                );
        }

        public static char WhiteSpace => ' ';


        /// <summary>
        /// Type of file to save
        /// </summary>
        public enum TypeOfFile
        {
            Text,
            Csv
        }

        /// <summary>
        /// Convert Base64 to string.
        /// </summary>
        /// <param name="base64String">The base64 string.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Input string for {nameof(base64String)} is null or empty - base64String</exception>
        public static string Base64ToString(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException($"Input string for {nameof(base64String)} is null or empty", nameof(base64String));

            base64String = base64String.Replace('-', '+');
            base64String = base64String.Replace('_', '/');

            var byteArray = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(byteArray);
        }

        /// <summary>
        /// Saves to file.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="separteFolder">if set to <c>true</c> [separte folder].</param>
        /// <param name="typeOfFile">The type of file.</param>
        public static void SaveToFile(string content ,string fileName = "file",  bool separteFolder = false, TypeOfFile typeOfFile = TypeOfFile.Text) 
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            string fileExtentsion;
            switch (typeOfFile)
            {
                case TypeOfFile.Text:
                    fileExtentsion = ".txt";
                    break;
                case TypeOfFile.Csv:
                    fileExtentsion = ".csv";
                    break;
                default:
                    fileExtentsion = ".txt";
                    break;
            }

            if (separteFolder)
            {
                var folderPath = Path.Combine(currentDirectory, fileName);
                Directory.CreateDirectory(folderPath);
                var pathWithFileName = Path.Combine(folderPath, AppendTimeStamp(fileName + fileExtentsion));
                File.WriteAllText(pathWithFileName, content);
            }
            else 
            {
                var pathWithFileName = Path.Combine(currentDirectory, AppendTimeStamp(fileName + fileExtentsion));
                File.WriteAllText(pathWithFileName, content);
            }
        }
    }
}
