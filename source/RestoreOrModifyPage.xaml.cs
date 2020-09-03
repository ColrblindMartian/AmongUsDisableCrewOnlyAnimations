using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für RestoreOrModifyPage.xaml
    /// </summary>
    public partial class RestoreOrModifyPage : Page
    {
        public RestoreOrModifyPage()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex == 0)
            {
                GlobalVars.removeModChoice = true;
                GlobalVars.pageNavigator.AddPage(EPages.eRestoreBackupPage);
            }
            else
            {
                // remove unnecessary page
                GlobalVars.pageNavigator.RemovePage(EPages.eRestoreBackupPage);
            }
        }
    }
}
