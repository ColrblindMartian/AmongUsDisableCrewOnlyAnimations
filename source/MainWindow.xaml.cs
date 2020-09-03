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
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ///\todo check version 
            ///and add page for version check?

            // init pages
            GlobalVars.pageNavigator = new PageNavigator();
            GlobalVars.pageNavigator.AddPage(EPages.eStartPage);
            GlobalVars.pageNavigator.AddPage(EPages.eSelectDirectoryPage);
            GlobalVars.pageNavigator.AddPage(EPages.eRestoreOrModifyPage);
            GlobalVars.pageNavigator.AddPage(EPages.eRestoreBackupPage);
            GlobalVars.pageNavigator.AddPage(EPages.eSelectAnimationPage);
            GlobalVars.pageNavigator.AddPage(EPages.ePatchAnimationPage);

            MainFrame.Content = GlobalVars.pageNavigator.GetCurrentPage();
        }

        // Navigation
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = GlobalVars.pageNavigator.PreviousPage();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = GlobalVars.pageNavigator.NextPage();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //\todo delete tmp files
            System.Windows.Application.Current.Shutdown();
        }
    }
}
