using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace AmongUsHardcorePatch
{
    /// <summary>
    /// Interaktionslogik für SelectDirectoryPage.xaml
    /// </summary>
    public partial class SelectDirectoryPage : Page
    {
        const string steamAppId = "945360";
        const string steamAppManifestFile = @"\steamapps\appmanifest_" + steamAppId + ".acf";
        const string steamLibraryFolderFile = @"\steamapps\libraryfolders.vdf";
        const string gameSubfolder = @"\steamapps\common\Among Us\";
        const string sharedassetsFile = @"Among Us_Data\sharedassets0.assets";

        public SelectDirectoryPage()
        {
            InitializeComponent();

            // yes there are a lot of if else and even some try catch in there...
            // don't be mad at me I will probably never clean this up

            // search for steam installation path
            string steamInstallPath = "";
            string gameInstallPath = "";

            const string keyInstallPath32bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
            const string keyInstallPath64bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";

            steamInstallPath = (string)Registry.GetValue(keyInstallPath64bit, "InstallPath", String.Empty);
            if(steamInstallPath == String.Empty)
                steamInstallPath = (string)Registry.GetValue(keyInstallPath32bit, "InstallPath", String.Empty);

            // found -> search for game installation folder
            if (steamInstallPath != String.Empty)
            {
                // standard folder
                if (File.Exists(steamInstallPath + steamAppManifestFile))
                {
                    if (File.Exists(steamInstallPath + gameSubfolder + sharedassetsFile))
                    {
                        gameInstallPath = steamInstallPath + gameSubfolder;
                    }
                    else
                    {
                        MessageBox.Show("Game installation path not found!\nPlease select the folder manually");
                    }
                }
                else
                {
                    // we have to look in the other installation folders
                    // these folders are saved in the libraryfolders.vdf file
                    if (File.Exists(steamInstallPath + steamLibraryFolderFile))
                    {
                        // read lines
                        string[] libraryfolderLines;
                        List<string> lstInstallationPaths = new List<string>();
                        try
                        {
                            libraryfolderLines = File.ReadAllLines(steamInstallPath + steamLibraryFolderFile);

                            // parse lines 
                            // search for "1" , "2"... that list the Installation paths
                            // file looks like this:
                            // "LibraryFolders"
                            // {
                            //    "TimeNextStatsReport"       "xxxxxxxxxx"
                            //     "ContentStatsID"        "xxxxxxxxxxxxxxxxxxx"       
                            //     "1"     "D:\\Path\\To\\SteamLibraryFolder"
                            // }

                            foreach (string line in libraryfolderLines)
                            {
                                // regex: \"[0-9]+".*"(.*)"
                                string pattern = @"\" + '"' + "[0-9]+" + '"' + ".*" + '"' + "(.*)" + '"';
                                Match match = Regex.Match(line, pattern);
                                if(match.Success && match.Groups.Count == 1)
                                {
                                    string result = match.Groups[0].Value;
                                    result.Replace(@"\\", @"\");
                                    lstInstallationPaths.Add(match.Groups[0].Value);
                                }
                            }
                            if (lstInstallationPaths.Count == 0)
                                throw new Exception("no installation path found");

                            // now look in every installation path and search for the appid
                            foreach(string path in lstInstallationPaths)
                            {
                                if(File.Exists(path + steamAppManifestFile))
                                {
                                    if (File.Exists(path + gameSubfolder + sharedassetsFile))
                                    {
                                        gameInstallPath = path + gameSubfolder;
                                        break;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Game installation path not found!\nPlease select the folder manually");
                                    }
                                    
                                }
                            }

                        }
                        catch
                        {
                            MessageBox.Show("Game installation path not found!\nPlease select the folder manually");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Game installation path not found!\nPlease select the folder manually");
                    }
                }

                if(gameInstallPath == "")
                {
                    txtBoxDirectory.Text = steamInstallPath;
                    txtBlockDirectoryInfo.Text = "the game directory could not be located automatically. Please select the location manually";
                    txtBlockDirectoryInfo.Foreground = Brushes.Red;
                    GlobalVars.gameInstallationFound = false;
                    GlobalVars.gameInstallationPath = steamInstallPath;
                }
                else
                {
                    txtBoxDirectory.Text = gameInstallPath;
                    CheckPath(gameInstallPath);
                    txtBlockDirectoryInfo.Text = "the game directory was located automatically";
                    txtBlockDirectoryInfo.Foreground = Brushes.Green;
                }
            }            
        }

        private void BtnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.Description = "Select the game Installatin Folder called \"Among Us\"";
            folderBrowserDialog.SelectedPath = GlobalVars.gameInstallationPath;

            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                CheckPath(folderBrowserDialog.SelectedPath);
            }
            GlobalVars.pageNavigator.UpdateButtonVisibility();
        }

        private void TxtBoxDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                CheckPath((sender as System.Windows.Controls.TextBox).Text);
                GlobalVars.pageNavigator.UpdateButtonVisibility();
            }
        }

        private void CheckPath(string path)
        {
            txtBoxDirectory.Text = path;

            // check if path ok
            if (File.Exists(path + @"\" + sharedassetsFile))
            {
                GlobalVars.gameInstallationFound = true;
                GlobalVars.gameInstallationPath = path;
                GlobalVars.sharedassetsFileLocation = path + @"\" + sharedassetsFile;
                txtBlockDirectoryInfo.Text = "game directory found";
                txtBlockDirectoryInfo.Foreground = Brushes.Green;

                // check if mod is already installed
                if (IsModAlreadyInstalled(GlobalVars.sharedassetsFileLocation))
                {
                    GlobalVars.modAlreadyInstalled = true;
                    // add backup pages
                    GlobalVars.pageNavigator.AddPage(EPages.eRestoreOrModifyPage);
                    GlobalVars.pageNavigator.AddPage(EPages.eRestoreBackupPage);
                }
                else
                {
                    GlobalVars.modAlreadyInstalled = false;
                    // remove backup pages
                    GlobalVars.pageNavigator.RemovePage(EPages.eRestoreOrModifyPage);
                    GlobalVars.pageNavigator.RemovePage(EPages.eRestoreBackupPage);
                }
            }
            else
            {
                GlobalVars.gameInstallationFound = false;
                txtBlockDirectoryInfo.Text = "no valid game directory selected";
                txtBlockDirectoryInfo.Foreground = Brushes.Red;
            }
        }

        private bool IsModAlreadyInstalled(string sharedassetsFile)
        {
            bool bInstalled = false;
            
            try
            {
                FileStream fs;
                fs = new FileStream(sharedassetsFile, FileMode.Open);
                fs.Position = fs.Length - GlobalVars.modInstalledString.Length;
                byte[] buffer = new byte[GlobalVars.modInstalledString.Length];
                fs.Read(buffer, 0, buffer.Length);

                if(Encoding.ASCII.GetString(buffer) == GlobalVars.modInstalledString)
                {
                    bInstalled = true;
                }
                fs.Flush();
                fs.Close();
            }
            catch
            { }
            
            return bInstalled;
        }
    }
}
