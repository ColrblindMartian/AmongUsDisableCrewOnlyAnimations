using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AmongUsHardcorePatch
{
    public static class GlobalVars
    {
        public static string gameInstallationPath;
        public static bool gameInstallationFound = false;
        public static string sharedassetsFileLocation;
        public static string backupFileName = @"\sharedassets0_hardcoreMod_backup.assets";
        public static string tmpFile;

        public static bool removeModChoice = true;

        public static bool patchingFinished = false;
        public static bool patchingFailed = true;

        public static Dictionary<string, bool> lstSelectedAnimations;

        //public static List<Page> lstPages;
        public static PageNavigator pageNavigator;
        //public static EPages pages;
        //public static int pageIterator;

        public static string modInstalledString = "HardcoreModAdded"; // exactly 16 bytes
        public static bool modAlreadyInstalled;
    }

    public static class EObjectKeys
    {
        public static string gunfire = "gunfire";
        public static string WeaponFiringBottom = "WeaponFiringBottom";
        public static string WeaponFiringTop = "WeaponFiringTop";
        public static string ScanBACK = "ScanBACK";
        public static string ScanFront = "ScanFront";
        public static string HatchOpening = "HatchOpening";
        public static string ShieldOn = "ShieldOn";
        public static string ShieldOff = "ShieldOff";
        public static string shieldborder_off = "shieldborder_off";
        public static string Particles = "Particles";
    }

    /// <summary>
    /// All available pages,
    /// The value also defines the order
    /// </summary>
    public enum EPages
    {
        eMinInvalidPage =       0x01,   // 0000 0001

        // add Version Page             
        eStartPage =            0x02,   // 0000 0010
        eSelectDirectoryPage =  0x04,   // 0000 0100
        eRestoreOrModifyPage =  0x08,   // 0000 1000
        eRestoreBackupPage =    0x10,   // 0001 0000
        eSelectAnimationPage =  0x20,   // 0010 0000
        ePatchAnimationPage =   0x40,   // 0100 0000
                                        
        eMaxInvalidPage =       0x80    // 1000 0000
    }

    public class PageNavigator
    {
        public PageNavigator()
        {
            currentPageIndex = 0;
            pages = 0;
            firstPage = EPages.eMaxInvalidPage;
            lastPage =  EPages.eMinInvalidPage;
            allowOnlyFinished = false;
        }

        /// <summary>
        /// gets filled with EPages values
        /// </summary>
        private EPages pages;
        /// <summary>
        /// points to the index of the current page
        /// </summary>
        private EPages currentPageIndex;
        private EPages firstPage;
        private EPages lastPage;
        private Page currentPage;

        public bool allowOnlyFinished;

        public void AddPage(EPages page)
        {
            pages |= page;

            if (page > lastPage)  lastPage =  page;
            if (page < firstPage) firstPage = page;
        }

        public void RemovePage(EPages page)
        {
            EPages pagesBefore = pages;
            pages &= ~page;

            // page to remove was never included
            if (pages == pagesBefore)
                return;

            // find new max and min pages
            // I have no idea why I support removing pages...
            firstPage = EPages.eMaxInvalidPage;
            lastPage =  EPages.eMinInvalidPage;
            int iterator = (int)EPages.eMinInvalidPage;
            for(; ; )
            {
                iterator <<= 1;
                if(iterator >= (int)EPages.eMaxInvalidPage)
                {
                    // done
                    break;
                }

                if((iterator & (int)pages) == iterator)
                {
                    if (iterator > (int)lastPage)  lastPage =  (EPages)iterator;
                    if (iterator < (int)firstPage) firstPage = (EPages)iterator;
                }
            }

        }

        private void CreatePage(EPages pageIndex)
        {
            switch( pageIndex )
            {
                case EPages.eStartPage:             currentPage = new StartPage();              break;
                case EPages.eSelectDirectoryPage:   currentPage = new SelectDirectoryPage();    break;
                case EPages.eRestoreOrModifyPage:   currentPage = new RestoreOrModifyPage();    break;
                case EPages.eRestoreBackupPage:     currentPage = new RestoreBackupPage();      break;
                case EPages.eSelectAnimationPage:   currentPage = new SelectAnimationsPage();   break;
                case EPages.ePatchAnimationPage:    currentPage = new PatchAnimationsPage();    break;

                case EPages.eMinInvalidPage:
                case EPages.eMaxInvalidPage:
                    throw new Exception("CreatePage(): invalid Page index");
            }
            currentPageIndex = pageIndex;
        }

        public Page NextPage()
        {
            int currentIndex = (int)currentPageIndex;
            
            for(; ; )
            {
                currentIndex <<= 1;

                // no page after current one?
                if (allowOnlyFinished || currentIndex >= (int)EPages.eMaxInvalidPage)
                {
                    // close window
                    System.Windows.Application.Current.Shutdown();
                    return currentPage;
                }

                // check if next index is added in pages
                if (((int)pages & currentIndex) == currentIndex)
                {
                    currentPageIndex = (EPages)currentIndex;
                    CreatePage(currentPageIndex);
                    UpdateButtonVisibility();
                    return currentPage;
                }
            } 
        }

        public Page PreviousPage()
        {
            int currentIndex = (int)currentPageIndex;

            for (; ; )
            {
                //currentPageIndex--;
                currentIndex >>= 1;

                // no page before current one?
                if (currentIndex <= (int)EPages.eMinInvalidPage)
                {
                    return currentPage;
                }

                // check if next index is added as page
                if (((int)pages & currentIndex) == currentIndex)
                {
                    currentPageIndex = (EPages)currentIndex;
                    UpdateButtonVisibility();
                    CreatePage(currentPageIndex);
                    return currentPage;
                }
            }
        }

        public Page GetCurrentPage()
        {
            if (currentPage == null)
            {
                if (currentPageIndex == 0 || currentPageIndex == EPages.eMaxInvalidPage || currentPageIndex == EPages.eMinInvalidPage)
                    currentPageIndex = firstPage;
                CreatePage(currentPageIndex);
                UpdateButtonVisibility();
            }
                
            return currentPage;
        }

        public void UpdateButtonVisibility()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

                // first reset everything to default
                mainWindow.btnBack.IsEnabled = true;
                mainWindow.btnBack.Content = "< Back";
                mainWindow.btnNext.IsEnabled = true;
                mainWindow.btnNext.Content = "Next >";
                mainWindow.btnCancel.IsEnabled = true;

                if(allowOnlyFinished)
                {
                    mainWindow.btnNext.Content = "Finish";
                    mainWindow.btnBack.IsEnabled = false;
                    mainWindow.btnCancel.IsEnabled = false;
                }


                // on first Page we don't want the Back button to be pressed
                if (currentPageIndex == firstPage)
                    mainWindow.btnBack.IsEnabled = false;

                // on last page we want to disable the Finish Button 

                // if the page is on LastPage rename the Next Button
                if (currentPageIndex == lastPage)
                {
                    mainWindow.btnNext.Content = "Finish";
                    // we are done there is no need to cancel
                    mainWindow.btnCancel.IsEnabled = false;
                }

                switch (currentPageIndex)
                {
                    case EPages.eStartPage: break;
                    case EPages.eSelectDirectoryPage:
                        if (!GlobalVars.gameInstallationFound)
                        {
                            mainWindow.btnNext.IsEnabled = false;
                        }
                        break;
                    case EPages.eRestoreOrModifyPage: break;
                    case EPages.eRestoreBackupPage: break;
                    case EPages.eSelectAnimationPage:
                        mainWindow.btnNext.Content = "Patch";
                        // check if at least one checkbox is pressed
                        bool bAtLeastOneChecked = false;
                        if (GlobalVars.lstSelectedAnimations != null)
                        {
                            foreach (bool value in GlobalVars.lstSelectedAnimations.Values)
                            {
                                bAtLeastOneChecked |= value;
                            }
                        }
                        mainWindow.btnNext.IsEnabled = bAtLeastOneChecked;

                        break;
                    case EPages.ePatchAnimationPage:
                        // The damage is done, there is no turning back now
                        mainWindow.btnBack.IsEnabled = false;
                        // wait for patching to be done
                        mainWindow.btnNext.IsEnabled = false;
                        mainWindow.btnCancel.IsEnabled = false;

                        if (GlobalVars.patchingFinished)
                        {
                            if (GlobalVars.patchingFailed)
                                mainWindow.btnCancel.IsEnabled = true;
                            else
                                mainWindow.btnNext.IsEnabled = true;
                        }

                        break;
                }
            }));
        }
    }


    public class Animation
    {
        public Animation(string name, int offsetToFrameStart, int frameCount)
        {
            this.Name = Encoding.ASCII.GetBytes(name);
            this.OffsetToFrameStart = offsetToFrameStart;
            this.FrameCount = frameCount;
            for (int i = 0; i < nMaxNumberOfOccurences; i++)
            {
                found[i] = false;
                ffBlockFound[i] = false;
                ffBlockCount[i] = 0;
                position[i] = 0;
                ffBlockEndPosition[i] = 0;
                iSearchIndex[i] = 0;

                firstFrameId[i] = 0;
                lastFrameId[i] = 0;
            }
        }
        //public string Name { get; }
        /// <summary>
        /// number of occurences that can be found in the hex file
        /// </summary>
        public const uint nMaxNumberOfOccurences = 2;
        /// <summary>
        /// this constant defines the offset between the end of the long FFFF... block and
        /// the first frame id. Can be used to check if the found name in the hex file is really a animation block
        /// </summary>
        public const uint nOffsetBytesFFToFirstFrameId = 68;
        /// <summary>
        /// not shure how to name this variable correctly... this number defines how many Bytes have to be set to 255 directly after another
        /// to be counted as a block ( between the name of the animation and the first frame id therer is always a big block where each byte has the value 255)
        /// </summary>
        public const uint nNumberOfFBytesForValidFFBlock = 20;
        public int[] ffBlockCount = new int[nMaxNumberOfOccurences];

        public byte[] Name;
        public int OffsetToFrameStart { get; }
        public int FrameCount { get; }
        public Int64[] firstFrameId = new Int64[nMaxNumberOfOccurences];  // == long
        public Int64[] lastFrameId = new Int64[nMaxNumberOfOccurences];
        public long[]  firstFramePos = new long[nMaxNumberOfOccurences];
        /// <summary>
        /// determines if the name was found in the prefab file
        /// </summary>
        public bool[] found = new bool[nMaxNumberOfOccurences];
        public bool[] ffBlockFound = new bool[nMaxNumberOfOccurences];
        public long[] ffBlockEndPosition = new long[nMaxNumberOfOccurences];
        /// <summary>
        /// contains the position(s) in the file where the start of the Name string was found
        /// check if found is true before using this position
        /// </summary>
        public long[] position = new long[nMaxNumberOfOccurences];
        public int[] iSearchIndex = new int[nMaxNumberOfOccurences];
    }

    public class GameObject
    {
        public GameObject(string strName, int addZerosAfterName, int nOffsetBytesToComponent)
        {
            // add number of 0s after the name to make shure the right one is found
            // e.g. the result could return true on "ParticlesRight" with the name "Particles" and we don't want that
            // Yes there can still be a false positive if string ends with Particles like "SnowParticles" but there is only one occurence
            // and we can filter this one out by just using the position offsets between the occurences like we do with
            // the shieldborder_off so I don't care
            this.ZerosAfterName = addZerosAfterName;
            byte[] name = new byte[strName.Length + addZerosAfterName];


            this.Name = new byte[strName.Length + addZerosAfterName];
            for (int i = 0; i < strName.Length; i++)
            {
                // save name
                this.Name[i] = Encoding.ASCII.GetBytes(strName)[i];
            }
            for (int i = 0; i < addZerosAfterName; i++)
            {
                // append 0s
                this.Name[i + strName.Length] = 0;
            }

            OffsetBytesToComponent = nOffsetBytesToComponent;

            for (int i = 0; i < nMaxNumberOfOccurences; i++)
            {
                found[i] = false;
                position[i] = 0;
                iSearchIndex[i] = 0;
            }
        }

        public byte[] Name;
        public int ZerosAfterName { get; }

        /// <summary>
        /// should be a negative number, because interestingly the name property in the GameObject is at the end.
        /// This defines the number of bytes to get from the name position to the component position
        /// that should be disabled/unlinked from this GameObject
        /// </summary>
        public int OffsetBytesToComponent { get; }

        public const uint nMaxNumberOfOccurences = 3;
        /// <summary>
        /// the next occurence with the same name has to be found within 100 bytes for the GameObject
        /// to be valid. Thats how we find the correct one, because the next GameObject with one ID above has interestingly
        /// the same name, but is not in use anymore.
        /// To be on the save site we set both to valid
        /// </summary>
        public const uint nOffsetToNextOccurenceForValidResult = 100;

        public bool[] found = new bool[nMaxNumberOfOccurences];
        public long[] position = new long[nMaxNumberOfOccurences];

        public int[] iSearchIndex = new int[nMaxNumberOfOccurences];
    }

    public static class ExceptionHandling
    {
        public static void ShowException(string message, bool bShutdown = false)
        {
            MessageBox.Show(message, "Error occured");
            if (bShutdown)
                CloseApplication();
        }
        
        public static void CloseApplication()
        {
            //\todo delete tmp file
            
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Application.Current.Shutdown();
            }));
        }
    }

    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

    }
}
