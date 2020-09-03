using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace AmongUsHardcorePatch
{
    /// <summary>
    /// Interaktionslogik für PatchAnimationsPage.xaml
    /// </summary>
    public partial class PatchAnimationsPage : Page
    {
        Dictionary<string, Animation> lstAnimations;
        Dictionary<string, GameObject> lstGameObjects;

        public PatchAnimationsPage()
        {
            InitializeComponent();


            // initialize animations to be found in file
            lstAnimations = new Dictionary<string, Animation>();
            if(GlobalVars.lstSelectedAnimations[EObjectKeys.gunfire])
                lstAnimations.Add("gunfire", new Animation(EObjectKeys.gunfire, 0xdcc, 45)); // only 1 occurence in file
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.WeaponFiringBottom])
                lstAnimations.Add(EObjectKeys.WeaponFiringBottom, new Animation(EObjectKeys.WeaponFiringBottom, 0xa58, 13));
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.WeaponFiringTop])
                lstAnimations.Add(EObjectKeys.WeaponFiringTop, new Animation(EObjectKeys.WeaponFiringTop, 0xa38, 12));
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.ScanBACK])
                lstAnimations.Add(EObjectKeys.ScanBACK, new Animation(EObjectKeys.ScanBACK, 0x2320, 240));
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.ScanFront])
                lstAnimations.Add(EObjectKeys.ScanFront, new Animation(EObjectKeys.ScanFront, 0x2324, 240)); // this string can be found two times! so we need some kind of extra check
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.HatchOpening])
                lstAnimations.Add(EObjectKeys.HatchOpening, new Animation(EObjectKeys.HatchOpening, 0xc9a, 34));
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.ShieldOn])
                lstAnimations.Add(EObjectKeys.ShieldOn, new Animation(EObjectKeys.ShieldOn, 0xb10, 20)); // frame have to be overwritten with ShieldOff frame
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.ShieldOff])
                lstAnimations.Add(EObjectKeys.ShieldOff, new Animation(EObjectKeys.ShieldOff, 0x900, 1));


            // the shieldborder_off is a special case
            // The Id of the frame for the On and Off sprite are saved in the SpriteRenderer
            // which itself is linked to the game object. There is probably a way to get the offset between the GameObject and
            // the linked frame id, but it is way easier to just unlink the SpriteRenderer from the GameObject
            // (the SpriteRendere itself can only be found with the Id and not the name that's why it not that easy to find)

            // there are in total 3 occurences of the string "shieldborder_off" in the hex file 
            // (yes they gave the sprite and the gameObject the same name)
            // 1) The Sprite itself
            // 2) The GameObject
            // 3) Another GameObject ( interestingly identical to 2 but used for April Fools Joke?, nonetheless this one helps to find the correct occurence of the string)
            lstGameObjects = new Dictionary<string, GameObject>();
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.shieldborder_off])
                lstGameObjects.Add(EObjectKeys.shieldborder_off, new GameObject(EObjectKeys.shieldborder_off, 0, -16));
            // it's the same with the Particle at the garbage hatch.
            // the only difference is we want to unlink the ParticleSystemRenderer not the SpriteRenderer from the GameObject
            // luckily for us both components where added as the last component to the GameObject so the offset between the name
            // and the component is the same
            if (GlobalVars.lstSelectedAnimations[EObjectKeys.Particles])
                lstGameObjects.Add(EObjectKeys.Particles, new GameObject(EObjectKeys.Particles, 5, -16));

            // logging
            richtxtBoxPatchLog.Document = new FlowDocument();

            //PatchAnimations();
            Task.Run(() => PatchAnimations());
        }

        // add line to log box
        DispatcherOperation LogToTextBox(string log)
        {
            log = DateTime.Now.ToString("H:mm:ss:ff>>") + log;
            return Dispatcher.BeginInvoke(new Action(() =>
            {
                richtxtBoxPatchLog.Document.Blocks.Add(new Paragraph(new Run(log)));
                richtxtBoxPatchLog.ScrollToEnd();
            }));
        }
        DispatcherOperation LogErrorToTextBox(string log)
        {
            log = DateTime.Now.ToString("H:mm:ss:ff Error: >>") + log;
            return Dispatcher.BeginInvoke(new Action(() =>
            {
                Run run = new Run(log);
                run.Foreground = Brushes.Red;
                run.FontWeight = FontWeights.DemiBold;
                richtxtBoxPatchLog.Document.Blocks.Add(new Paragraph(run));
                richtxtBoxPatchLog.ScrollToEnd();
            }));
        }

        void PatchAnimations()
        {
            // logging
            FlowDocument flowDocument = new FlowDocument();
            DateTime start = DateTime.UtcNow;

            // if it was already installed restore the backup
            if(GlobalVars.modAlreadyInstalled)
            {
                LogToTextBox("restore backup file");
                string backupfile = System.IO.Path.GetDirectoryName(GlobalVars.sharedassetsFileLocation) + GlobalVars.backupFileName;
                if (!File.Exists(backupfile))
                {
                    LogErrorToTextBox("Mod is already installed but backupfile does not exist! Backupfile: " + backupfile);
                    goto Errorlabel;
                }
                try
                {
                    File.Delete(GlobalVars.sharedassetsFileLocation);
                    File.Copy(backupfile, GlobalVars.sharedassetsFileLocation);
                    File.Delete(backupfile);
                }
                catch(Exception e)
                {
                    LogErrorToTextBox(e.Message);
                    goto Errorlabel;
                }
                
            }

            // create temporary file
            // we want to work with the tmp file and only copy it to the standard file at the very end
            string tmpFile = System.IO.Path.GetDirectoryName(GlobalVars.sharedassetsFileLocation) +  @"\sharedassets0_tmp_" + DateTime.Now.ToString("H-mm-ss_dd-MM-yy") + ".assets";
            LogToTextBox("create temporary file with name: " + System.IO.Path.GetFileName(tmpFile));
            try
            {
                File.Copy(GlobalVars.sharedassetsFileLocation, tmpFile);
            }
            catch(Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
                //return;
            }


            // test hex modification of sharedassets0.assets
            //string path = @"C:\Games\Steam\steamapps\common\Among Us\Among Us_Data\sharedassets0.assets";

            ///\todo
            /// create backup file
            /// work on tmp file
            /// copy backup file to normal file
            /// can we change the logo?

            //\todo errorhandling needed;
            LogToTextBox("opening " + tmpFile);
            FileStream fs;
            try
            {
                fs = File.Open(tmpFile, FileMode.Open);
            }
            catch(Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
            }

            int readByte;

            while ((readByte = fs.ReadByte()) != -1)
            {
                // search for animations
                foreach (KeyValuePair<string, Animation> entry in lstAnimations)
                {
                    for (int i = 0; i < Animation.nMaxNumberOfOccurences; i++)
                    {
                        if (!entry.Value.found[i])
                        {
                            // check byte maches current name byte we are searching for
                            if (readByte == entry.Value.Name[entry.Value.iSearchIndex[i]])
                            {
                                // found -> increment to search for next character
                                entry.Value.iSearchIndex[i]++;

                                // check if full string is found
                                if (entry.Value.iSearchIndex[i] >= entry.Value.Name.Length)
                                {

                                    if (entry.Value.found[i] != true)
                                    {
                                        entry.Value.found[i] = true;
                                        entry.Value.position[i] = fs.Position - entry.Value.Name.Length; // we want the save the start of the name not the last character
                                        LogToTextBox("Found string: <" + entry.Key + "> Nr: "+(i+1).ToString() + "; Position: "+entry.Value.position[i].ToString());
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // it does not match so we don't have to search again
                                entry.Value.iSearchIndex[i] = 0;
                                // break; needed because we want to reset every index
                            }
                        }
                        // validate found results
                        else
                        {
                            // are we not within name and first frame? -> invalid search result
                            // yes we check this every time I know
                            if ((fs.Position - entry.Value.position[i]) > entry.Value.OffsetToFrameStart
                                && !entry.Value.ffBlockFound[i])

                            {
                                entry.Value.found[i] = false; // invalidate search result
                                entry.Value.iSearchIndex[i] = 0; // necessary?
                                LogToTextBox("<" + entry.Key + ">; Nr: " + (i + 1).ToString() + " is invalid");
                                continue;
                            }

                            // the string was already found -> search for the large FFFFFFF... block between the name and the first frame id
                            if (readByte == byte.MaxValue)
                            {
                                entry.Value.ffBlockCount[i]++;
                                if (entry.Value.ffBlockCount[i] >= Animation.nNumberOfFBytesForValidFFBlock)
                                    entry.Value.ffBlockFound[i] = true;
                            }
                            else
                            {
                                entry.Value.ffBlockCount[i] = 0;

                                // if block was already found it means this is the end of the block
                                if (entry.Value.ffBlockFound[i] && entry.Value.ffBlockEndPosition[i] == 0)
                                {
                                    entry.Value.ffBlockEndPosition[i] = fs.Position - 2;
                                }
                            }
                        }
                    }
                }

                // search GameObjects
                foreach (KeyValuePair<string, GameObject> entry in lstGameObjects)
                {
                    for (int i = 0; i < GameObject.nMaxNumberOfOccurences; i++)
                    {
                        if (!entry.Value.found[i])
                        {
                            if (readByte == entry.Value.Name[entry.Value.iSearchIndex[i]])
                            {
                                entry.Value.iSearchIndex[i]++;

                                // check if full string is found
                                if (entry.Value.iSearchIndex[i] >= entry.Value.Name.Length)
                                {

                                    if (entry.Value.found[i] != true)
                                    {
                                        entry.Value.found[i] = true;
                                        entry.Value.position[i] = fs.Position - entry.Value.Name.Length; // we want the save the start of the name not the last character
                                        LogToTextBox("Found string: <" + entry.Key + "> Nr: " + (i + 1).ToString() + "; Position: " + entry.Value.position[i].ToString());
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                entry.Value.iSearchIndex[i] = 0;
                            }
                        }
                    }
                }
            } // end while() reading file

            // find the correct GameObject
            foreach (KeyValuePair<string, GameObject> entry in lstGameObjects)
            {
                for (uint i = GameObject.nMaxNumberOfOccurences - 1; i > 0; i--)
                {
                    for (int k = 0; k < GameObject.nMaxNumberOfOccurences - 1; k++)
                    {
                        if (!entry.Value.found[i] || !entry.Value.found[k])
                            continue;

                        // only check distance from higher position to lower position so we don't check multiple times
                        if (i <= k)
                            continue;

                        if ((entry.Value.position[i] - entry.Value.position[k]) <= GameObject.nOffsetToNextOccurenceForValidResult)
                        {
                            // Right GameObjects found -> invalidate the other result 
                            for (int m = 0; m < GameObject.nMaxNumberOfOccurences; m++)
                            {
                                if (m != i && m != k)
                                {
                                    entry.Value.found[m] = false;
                                    LogToTextBox("<" + entry.Key + "> Nr: " + (m + 1).ToString() + " is invalid");
                                }
                            }
                            // done
                            LogToTextBox("Found correct <" + entry.Key + "> Nr: " + (i + 1).ToString());
                            LogToTextBox("Found correct <" + entry.Key + "> Nr: " + (k + 1).ToString());
                            break;
                        }
                    }
                }
            }

            // get first and last frame of the animation
            foreach (KeyValuePair<string, Animation> entry in lstAnimations)
            {
                for (int i = 0; i < Animation.nMaxNumberOfOccurences; i++)
                {
                    if (entry.Value.ffBlockFound[i] && entry.Value.found[i])
                    {
                        // calculate position of first frame
                        // there are always 68 Bytes between the FF block and the first frame 68 Bytes = 544 Bit
                        long posFirstFrame = entry.Value.ffBlockEndPosition[i] + 68 + 1;
                        entry.Value.firstFramePos[i] = posFirstFrame;

                        byte[] buffer = new byte[8];
                        fs.Position = posFirstFrame;
                        fs.Read(buffer, 0, buffer.Length);
                        entry.Value.firstFrameId[i] = BitConverter.ToInt64(buffer, 0);

                        // calculate position of last frame
                        // there are always 12 Bytes between each Frame Id entry
                        // not really shure why exactly 12 Bytes(96bit), I don't know any datatype with that size.
                        // The Ids are probably 64bit uintegers and there is maybe some extra space between the array entries. Who knows..
                        long posLastFrame = posFirstFrame + (entry.Value.FrameCount - 1) * 12;

                        fs.Position = posLastFrame;
                        fs.Read(buffer, 0, buffer.Length);
                        entry.Value.lastFrameId[i] = BitConverter.ToInt64(buffer, 0);
                        LogToTextBox("<" + entry.Key + "> Nr: " + (i + 1).ToString() +
                            "\n\u0009 Number of Frames: " + entry.Value.FrameCount.ToString() +
                            "\n\u0009 firstFrame ID: " + entry.Value.firstFrameId[i].ToString() +
                            "\n\u0009 lastFrame  ID: " + entry.Value.lastFrameId[i].ToString());
                    }
                }
            }

            // check if everything was found:
            bool bSomeNotFound = false;
            foreach (KeyValuePair<string, Animation> entry in lstAnimations)
            {
                bool bFound = false;
                for (int i = 0; i < Animation.nMaxNumberOfOccurences; i++)
                {
                    bFound |= (entry.Value.found[i] & entry.Value.ffBlockFound[i]);
                    if (bFound) break;
                }
                if(!bFound)
                {
                    bSomeNotFound = true;
                    LogErrorToTextBox("Error finding correct <" + entry.Key + "> position");
                }
            }

            foreach (KeyValuePair<string, GameObject> entry in lstGameObjects)
            {
                bool bFound = false;
                for (int i = 0; i < Animation.nMaxNumberOfOccurences; i++)
                {
                    bFound |= entry.Value.found[i];
                    if (bFound) break;
                }
                if (!bFound)
                {
                    bSomeNotFound = true;
                    LogErrorToTextBox("Error finding correct <" + entry.Key + "> position");
                }
            }


            // disable animations
            // overwrite every animation frame id with the id of the first frame
            foreach (KeyValuePair<string, Animation> entry in lstAnimations)
            {
                for (int i = 0; i < Animation.nMaxNumberOfOccurences; i++)
                { 
                    if(entry.Value.found[i] && entry.Value.ffBlockFound[i])
                    {
                        byte[] newFrameId = new byte[8]; // = 64bit per frameId

                        if(entry.Key == EObjectKeys.ScanBACK || entry.Key == EObjectKeys.ScanFront)
                        {
                            // we want to overwrite the scan animation with the last frame not the first frame!
                            newFrameId = BitConverter.GetBytes(entry.Value.lastFrameId[i]);
                        }
                        else if(entry.Key == EObjectKeys.ShieldOn || entry.Key == EObjectKeys.ShieldOff)
                        {
                            // we want to replace the shieldOn frame with the ShieldOff frame
                            if (entry.Key == EObjectKeys.ShieldOff)
                                continue;
                            newFrameId = BitConverter.GetBytes(lstAnimations[EObjectKeys.ShieldOff].firstFrameId[i]);
                        }
                        else
                        {
                            newFrameId = BitConverter.GetBytes(entry.Value.firstFrameId[i]);
                        }

                        fs.Position = entry.Value.firstFramePos[i];
                        for(int k = 0; k < entry.Value.FrameCount; k++)
                        {
                            fs.Position = entry.Value.firstFramePos[i] + k * 12; // 12 bytes between each frame id
                            try
                            {
                                fs.Write(newFrameId, 0, 8);
                            }
                            catch(Exception e)
                            {
                                ExceptionHandling.ShowException(e.Message);
                                LogErrorToTextBox(e.Message);
                                goto Errorlabel;
                            }
                        }
                    }
                }
            }

            // disable/unlink SpriteRenderer and ParticleSystemRenderer from GameObject
            foreach (KeyValuePair<string, GameObject> entry in lstGameObjects)
            {
                for (int i = 0; i < GameObject.nMaxNumberOfOccurences; i++)
                { 
                    if(entry.Value.found[i])
                    {
                        fs.Position = entry.Value.position[i] + entry.Value.OffsetBytesToComponent;
                        try
                        {
                            fs.Write(BitConverter.GetBytes((long)0), 0, 8); // overwrite with 0 bytes to remove the linked component
                        }
                        catch(Exception e)
                        {
                            ExceptionHandling.ShowException(e.Message);
                            LogErrorToTextBox(e.Message);
                            goto Errorlabel;
                        }
                    }
                }
            }

            // append the file with a string so we can check later if the mod is installed
            fs.Position = fs.Length;
            fs.Write(Encoding.ASCII.GetBytes(GlobalVars.modInstalledString), 0, 16);

            fs.Flush();
            fs.Close();

            // create backup file
            //\todo get game version
            string backupFile = System.IO.Path.GetDirectoryName(GlobalVars.sharedassetsFileLocation) + GlobalVars.backupFileName;
            LogToTextBox("creating backup file: " + backupFile);
            try
            {
                File.Copy(GlobalVars.sharedassetsFileLocation, backupFile);
            }
            catch (Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
            }
            // delete original file
            LogToTextBox("deleting original sharedassets0.assets file");
            try
            {
                File.Delete(GlobalVars.sharedassetsFileLocation);
            }
            catch(Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
            }
            // copy tmp file
            LogToTextBox("replacing file with modded file");
            try
            {
                File.Copy(tmpFile, GlobalVars.sharedassetsFileLocation);
            }
            catch (Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
            }
            // delete tmp file - no longer needed
            LogToTextBox("deleting temporary file");
            try
            {
                File.Delete(tmpFile);
            }
            catch (Exception e)
            {
                ExceptionHandling.ShowException(e.Message, false);
                LogErrorToTextBox(e.Message);
                goto Errorlabel;
            }

            DateTime end = DateTime.UtcNow;
            TimeSpan timediff = end - start;
            LogToTextBox("finished in " + timediff.TotalMilliseconds.ToString() + "ms");
            if (bSomeNotFound)
            {
                LogErrorToTextBox("not all selected animations could be disabled");
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtBlockPatchInfo.Foreground = Brushes.Red;
                    txtBlockPatchInfo.Text = "Some Animations could not be disabled. Check the log info above for more information and launch the game.\n" +
                        "To disable this mod launch the executable again and select <Remove mod>";
                }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtBlockPatchInfo.Foreground = Brushes.Green;
                    txtBlockPatchInfo.Text = "Done.\n" +
                        "To disable this mod launch the executable again and select <Remove mod>";
                }));
            }

            GlobalVars.patchingFinished = true;
            GlobalVars.patchingFailed = false;
            GlobalVars.pageNavigator.UpdateButtonVisibility();

            return;

        Errorlabel:
            Dispatcher.Invoke(new Action(() =>
            {
                txtBlockPatchInfo.Foreground = Brushes.Red;
                txtBlockPatchInfo.Text = "Error occured! \nAnimations could not be disabled";
            }));
            GlobalVars.patchingFinished = true;
            GlobalVars.patchingFailed = true;
            GlobalVars.pageNavigator.allowOnlyFinished = true;
            GlobalVars.pageNavigator.UpdateButtonVisibility();
            //\todo
            return;
        }
    }
}
