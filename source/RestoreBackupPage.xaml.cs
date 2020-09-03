using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace AmongUsHardcorePatch
{
    /// <summary>
    /// Interaktionslogik für RestoreBackupPage.xaml
    /// </summary>
    public partial class RestoreBackupPage : Page
    {
        public RestoreBackupPage()
        {
            InitializeComponent();

            // should mod be uninstalled? we should never come to this page otherwise, but just to be shure
            if(GlobalVars.removeModChoice)
            {
                try
                {
                    //is backupfile available?
                    string backupfile = System.IO.Path.GetDirectoryName(GlobalVars.sharedassetsFileLocation) + GlobalVars.backupFileName;
                    if (!File.Exists(backupfile))
                    {
                        throw new Exception("backup file " + backupfile + " not found");
                    }

                    File.Delete(GlobalVars.sharedassetsFileLocation);
                    File.Copy(backupfile, GlobalVars.sharedassetsFileLocation);
                    File.Delete(backupfile);
                }
                catch(Exception e)
                {
                    txtBlockInfo.Text = "Mod could not be removed:\n" + e.Message;
                    txtBlockInfo.Foreground = Brushes.Red;
                    GlobalVars.pageNavigator.allowOnlyFinished = true;
                    GlobalVars.pageNavigator.UpdateButtonVisibility();
                    return;
                }
                txtBlockInfo.Text = "Mod was succesfully removed from the game.\nHave fun playing the easy version ;)";
                txtBlockInfo.Foreground = Brushes.Green;
                GlobalVars.pageNavigator.allowOnlyFinished = true;
                GlobalVars.pageNavigator.UpdateButtonVisibility();
            }
        }
    }
}
