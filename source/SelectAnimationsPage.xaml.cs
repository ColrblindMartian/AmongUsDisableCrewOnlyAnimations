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
    /// Interaktionslogik für SelectAnimations.xaml
    /// </summary>
    public partial class SelectAnimationsPage : Page
    {
        public SelectAnimationsPage()
        {
            InitializeComponent();

            // init list
            GlobalVars.lstSelectedAnimations = new Dictionary<string, bool>();
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.gunfire, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.HatchOpening, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.Particles, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.ScanBACK, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.ScanFront, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.shieldborder_off, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.ShieldOff, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.ShieldOn, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.WeaponFiringBottom, false);
            GlobalVars.lstSelectedAnimations.Add(EObjectKeys.WeaponFiringTop, false);
        }

        private void chbxMouseEnter(object sender, MouseEventArgs e)
        {
            string chbxName = (sender as CheckBox).Name;

            if(chbxName == chbxScanner.Name)
            {
                ImageInfoPanel.Source = new BitmapImage(new Uri("pack://application:,,,/AmongUsHardcorePatch;component/Images/ScanAnimation.png"));
                txtBlockInfoMapOccurences.Text = "\u2022 all Maps";
            }
            else if (chbxName == chbxGunfire1.Name)
            {
                ImageInfoPanel.Source = new BitmapImage(new Uri("pack://application:,,,/AmongUsHardcorePatch;component/Images/gunfire1.png"));
                txtBlockInfoMapOccurences.Text = "\u2022 Polus";
            }
            else if (chbxName == chbxGunfire2.Name)
            {
                ImageInfoPanel.Source = new BitmapImage(new Uri("pack://application:,,,/AmongUsHardcorePatch;component/Images/gunfire2.png"));
                txtBlockInfoMapOccurences.Text = "\u2022 The Skeld";
            }
            else if (chbxName == chbxPrimeShield.Name)
            {
                ImageInfoPanel.Source = new BitmapImage(new Uri("pack://application:,,,/AmongUsHardcorePatch;component/Images/PrimeShields.png"));
                txtBlockInfoMapOccurences.Text = "\u2022 The Skeld";
            }
            else if (chbxName == chbxGarbageHatch.Name)
            {
                ImageInfoPanel.Source = new BitmapImage(new Uri("pack://application:,,,/AmongUsHardcorePatch;component/Images/garbage.png"));
                txtBlockInfoMapOccurences.Text = "\u2022 The Skeld";
            }
            else
            {
                throw new Exception(chbxName + " is not handled in if statements");
            }

            gridInfoPanel.Visibility = Visibility.Visible;
        }
        private void chbxMouseLeave(object sender, MouseEventArgs e)
        {
            gridInfoPanel.Visibility = Visibility.Hidden;
        }

        private void chbxClick(object sender, RoutedEventArgs e)
        {
            CheckBox chbxClicked = (sender as CheckBox);

            if(chbxClicked == chbxToggleAll)
            {
                chbxScanner.IsChecked = chbxToggleAll.IsChecked;
                chbxGunfire1.IsChecked = chbxToggleAll.IsChecked;
                chbxGunfire2.IsChecked = chbxToggleAll.IsChecked;
                chbxPrimeShield.IsChecked = chbxToggleAll.IsChecked;
                chbxGarbageHatch.IsChecked = chbxToggleAll.IsChecked;

                var newDictionary = new Dictionary<string, bool>(GlobalVars.lstSelectedAnimations);
                foreach (var entry in newDictionary)
                {
                    GlobalVars.lstSelectedAnimations[entry.Key] = chbxToggleAll.IsChecked.Value;
                }
            }
            else if(chbxClicked == chbxScanner)
            {
                GlobalVars.lstSelectedAnimations[EObjectKeys.ScanBACK] = chbxClicked.IsChecked.Value;
                GlobalVars.lstSelectedAnimations[EObjectKeys.ScanFront] = chbxClicked.IsChecked.Value;
            }
            else if (chbxClicked == chbxGunfire1)
            {
                GlobalVars.lstSelectedAnimations[EObjectKeys.gunfire] = chbxClicked.IsChecked.Value;
            }
            else if (chbxClicked == chbxGunfire2)
            {
                GlobalVars.lstSelectedAnimations[EObjectKeys.WeaponFiringTop] = chbxClicked.IsChecked.Value;
                GlobalVars.lstSelectedAnimations[EObjectKeys.WeaponFiringBottom] = chbxClicked.IsChecked.Value;
            }
            else if (chbxClicked == chbxPrimeShield)
            {
                GlobalVars.lstSelectedAnimations[EObjectKeys.shieldborder_off] = chbxClicked.IsChecked.Value;
                GlobalVars.lstSelectedAnimations[EObjectKeys.ShieldOn] = chbxClicked.IsChecked.Value;
                GlobalVars.lstSelectedAnimations[EObjectKeys.ShieldOff] = chbxClicked.IsChecked.Value;
            }
            else if (chbxClicked == chbxGarbageHatch)
            {
                GlobalVars.lstSelectedAnimations[EObjectKeys.Particles] = chbxClicked.IsChecked.Value;
                GlobalVars.lstSelectedAnimations[EObjectKeys.HatchOpening] = chbxClicked.IsChecked.Value;
            }
            else
            {
                throw new Exception(chbxClicked.Name + " not handled in if statements");
            }

            GlobalVars.pageNavigator.UpdateButtonVisibility();
        }
    }
}
