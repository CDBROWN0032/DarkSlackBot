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
        public bool _isWindows;

        public void Run()
        {            
            CheckOperatingSystem();

            if(_isWindows)
            {
                RunOnWindows();
            } else
            {
                RunOnMAC();
            }            
        }

        private void RunOnWindows()
        {
            Console.WriteLine("Windows OS Detected");
            GetUser();
            var directoryFolders = GetDirectoryInfo();
            var folders = ParseFolders(directoryFolders);
            var newestVersion = SelectNewestVersion(folders);
            WindowsDirectoryDrill(newestVersion);
        }

        private void RunOnMAC()
        {
            Console.WriteLine("MAC OS Detected");
            MacDirectoryDrill();
        }

        private void GetUser()
        {            
            _user = Environment.UserName;            
        }
        
        private void CheckOperatingSystem()
        {            
            var platform = Environment.OSVersion.Platform.ToString().ToLower();
            _isWindows = platform.Contains("win");
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

        private void WindowsDirectoryDrill(string newestVersion)
        {
            var neededFile = @"ssb-interop.js";
            var fileAddress = $@"{_directory}\{newestVersion}\resources\app.asar.unpacked\src\static\";
            var file = $@"{fileAddress}{neededFile}";

            SlackBotWriter(file, fileAddress);
        }

        private void MacDirectoryDrill()
        {
            var neededFile = @"ssb-interop.js";
            var fileAddress = $@"/Applications/Slack.app/Contents/Resources/app.asar.unpacked/src/static/";
            var file = $@"{fileAddress}{neededFile}";
            SlackBotWriter(file, fileAddress);
        }

        private void SlackBotWriter(string file, string fileAddress)
        {
            if (File.Exists(file))
            {
                Console.WriteLine($"Creating Backup");
                CreateFileBackup(file, fileAddress);

                var darkSlackCode = GetDarkSlackCode();

                using (StreamWriter writer = File.AppendText(file))
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
