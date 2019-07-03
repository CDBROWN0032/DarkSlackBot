using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DarkSlackBot
{
    class SlackBot
    {
        public string _user;
        public string _directory;
        public OperatingSystem _os;

        public void Run()
        {
            GetUser();
            var directoryFolders = GetDirectoryInfo();
            var folders = ParseFolders(directoryFolders);
            var newestVersion = SelectNewestVersion(folders);
            DirectoryDrill(newestVersion);
        }



        private void GetUser()
        {
            Console.WriteLine($"Press Any Key To Begin...");
            Console.ReadKey();
            
            _user = Environment.UserName;            
        }
        
        private List<string> GetDirectoryInfo()
        {
            Console.WriteLine($"Fetching Slack Directory");
            _directory = $@"C:\Users\{_user}\AppData\Local\slack";

            var directoryFolders = Directory.GetDirectories(_directory).ToList();

            return directoryFolders;
        }

        private List<string> ParseFolders(List<string> directoryFolders)
        {
            List<string> theFolders = new List<string>();
            foreach (var item in directoryFolders)
            {
                var folderName = item.Split(@"\").Last();

                var folderCheck = folderName.Contains("app");

                if (folderCheck)
                    theFolders.Add(folderName);
            }
            theFolders.OrderBy(x => x);

            return theFolders;

        }

        private string SelectNewestVersion(List<string> directoryFolders)
        {
            Console.WriteLine($"Selecting Newest Version of Slack");
            return directoryFolders.Last();
        }

        private void DirectoryDrill(string newestVersion)
        {
            var neededFile = @"ssb-interop.js";
            var fileAddress = $@"{_directory}\{newestVersion}\resources\app.asar.unpacked\src\static\";
            var fileLocation = $@"{fileAddress}{neededFile}";

            if (File.Exists(fileLocation))
            {
                Console.WriteLine($"Creating Backup");
                CreateFileBackup(fileLocation, fileAddress);

                var darkSlackCode = GetDarkSlackCode();

                using(StreamWriter writer = File.AppendText(fileLocation))
                {
                    Console.WriteLine($"Injecting JavaScript");
                    writer.Write(darkSlackCode);
                }
                Console.WriteLine($"Reboot Slack and Enjoy Dark Theme");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Error: File Not Found =(");
                Console.ReadKey();
            }

        }

        private string GetDarkSlackCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                                          .Single(str => str.EndsWith("addThis.txt"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        private void CreateFileBackup(string fileLocation, string fileAddress)
        {
            File.Copy(fileLocation, $"{fileAddress}ssb-interop.bac", true);
        }

    }
}
