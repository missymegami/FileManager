using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class Program
    {
        public class FileDetails
        {
            public string FileName { get; set; }
            public string FileHash { get; set; }
        }

        //checks that user entered filepath exists
        public static bool TryGetFullPath(string path, out string result)
        {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool status = false;

            try
            {
                var testPath = Directory.GetFiles(path);// Path.GetFullPath(path);
                status = true;
            }
            catch (ArgumentException) { Console.WriteLine("Path invalid, please re-enter the path."); }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { Console.WriteLine("Path name is too long, please re-enter the path."); }
            catch (DirectoryNotFoundException) { Console.WriteLine("Path could not be found, please re-enter the path."); }

            return status;
        }

        public static bool IsValidPath(string path)
        {
            string result;
            return TryGetFullPath(path, out result);
        }

        static void Main(string[] args)
        {
            string path = "";
            double totalSize = 0;

            //welcome screen
            Console.SetWindowSize(130, 50);
            Console.WriteLine("\n\n    oooooooooooo  o8o  oooo               ooo        ooooo                                                               ");
            Console.WriteLine("    `888'     `8  `\"'  `888               `88.       .888'                                                               ");
            Console.WriteLine("     888         oooo   888   .ooooo.      888b     d'888   .oooo.   ooo. .oo.    .oooo.    .oooooooo  .ooooo.  oooo d8b ");
            Console.WriteLine("     888oooo8    `888   888  d88' `88b     8 Y88. .P  888  `P  )88b  `888P\"Y88b  `P  )88b  888' `88b  d88' `88b `888\"\"8P ");
            Console.WriteLine("     888    \"     888   888  888ooo888     8  `888'   888   .oP\"888   888   888   .oP\"888  888   888  888ooo888  888     ");
            Console.WriteLine("     888          888   888  888    .o     8    Y     888  d8(  888   888   888  d8(  888  `88bod8P'  888    .o  888     ");
            Console.WriteLine("    o888o        o888o o888o `Y8bod8P'    o8o        o888o `Y888\"\"8o o888o o888o `Y888\"\"8o `8oooooo.  `Y8bod8P' d888b    ");
            Console.WriteLine("                                                                                           d\"     YD                     ");
            Console.WriteLine("                                                                                           \"Y88888P'                     \n\n");

            //prompt for user input to get folder path, prompt to re-enter if invalid.
            while (!IsValidPath(path))
            {
                Console.WriteLine("\nPlease enter the filepath of the directory you would like to scan in a format such as" + @" ""C:\Users\Melissa\Desktop""");
                path = Console.ReadLine();
                if (!IsValidPath(path))
                {
                    Console.WriteLine("Path invalid, please try again.");
                    path = "";
                }                    
            }

            var fileList = Directory.GetFiles(path);
            int numFiles = fileList.Length;
            Console.WriteLine("\n\n--------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Folder " + path + " contains " + numFiles + " files");

            List<FileDetails> fileDetailsList = new List<FileDetails>();
            List<string> duplicateFilenames = new List<string>();

            //get file details including hash code for each file in the folder
            foreach (var file in fileList)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    fileDetailsList.Add(new FileDetails()
                    {
                        FileName = file,
                        FileHash = BitConverter.ToString(SHA1.Create().ComputeHash(fs)),
                    });
                }
            }

            //group by hash code to find duplicates
            var similarList = fileDetailsList.GroupBy(f => f.FileHash)
                .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });

            //ignore the first file of each hash code, identifying the rest as duplicate files
            duplicateFilenames.AddRange(similarList.SelectMany(f => f.Files.Skip(1)).ToList());
            Console.WriteLine("This folder contains " + duplicateFilenames.Count + " duplicate files");

            //list all duplicate files and their filesizes
            if (duplicateFilenames.Count > 0)
            {
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------- ");
                var sortedDuplicateFilenames = from item in duplicateFilenames
                           orderby new FileInfo(item).Length descending
                           select item;

                foreach (string fileName in sortedDuplicateFilenames)
                {
                    Console.WriteLine(fileName);                    
                    FileInfo fi = new FileInfo(fileName);
                    totalSize += fi.Length;
                    Console.WriteLine("File Size: {0} bytes", (fi.Length).ToString());
                }
            }
            else
            {
                Console.WriteLine("No Duplicates Found.");
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------- ");

            //show total amount of disc space taken up by duplicate files
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Total space used -  {0}mb", Math.Round((totalSize / 1000000), 6).ToString());

            //exit the program
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to exit the program.");
            Console.ReadKey();
        }
    }
}
