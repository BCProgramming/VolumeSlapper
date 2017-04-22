using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.AccessControl;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Interfaces;
using Vannatech.CoreAudio.Structures;
using VolumeLib;

namespace VolumeSlapper
{
    //quick console program slapped together to retrieve/set volume on Windows Vista and later.

    class Program
    {
        private static String HelpText = @"
VolumeSlapper Command-Line Volume Utility. v" + Assembly.GetEntryAssembly().GetName().Version.ToString() + @".
 Syntax:
 VolumeSlapper (operation) [arguments]
 
 operation: one of get, set, or status. set accepts an argument for the volume level, as well as an argument for the session.

 volumelevel: a value parsable as a floating point value between 0 and 1 to use as the volume level. used only for the set operation.

 examples:
   Retrieve current master volume:
   {0} get
   set current master volume to 50%:
   {0} set 0.5
   set Skype to 70% volume:
   {0} set Skype 0.7
   retrieve volume of Firefox
   {0} get Firefox
   View status of all volume levels
   {0} status
   

";
        static void Main(string[] args)
        {
           
            //we won't bother with fancy argument parsing here, just look at the buggers directly.
            if(args.Length==0)
            {
                ShowHelp();
                return;
            }
            else if(args.Length>0)
            {
                String sUseFile = null;
                if (args.Length > 1)
                    sUseFile = args[1];
                else
                    sUseFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BASeCamp", "VolumeSlapper", "Quick.xml");
                if (String.Compare(args[0],"save",StringComparison.OrdinalIgnoreCase)==0)
                {
                    //saves to a given filename.
                    String sTargetFile = sUseFile;
                    Console.WriteLine("Saving session volume data to " + sUseFile);
                    if (sTargetFile.StartsWith("\"")) sTargetFile = sTargetFile.Substring(1);
                    if (sTargetFile.EndsWith("\"")) sTargetFile = sTargetFile.Substring(0, sTargetFile.Length - 1);
                    if(File.Exists(sTargetFile))
                    {
                        Console.WriteLine("not overwriting existing file:\"" + sTargetFile + "\"");
                        return;
                    }
                    else
                    {
                        //save volume information to the file, if the process exists, index by the executable path. Otherwise, we index by the retrieved name.
                        XElement RootNode = new XElement("Mixer");
                        XDocument VolumeDoc = new XDocument(RootNode);
                        float MasterVolume = VolumeUtilities.GetMasterVolume();
                        VolumeDoc.Add(new XAttribute("MasterVolume",MasterVolume));
                        var VolumeData = VolumeUtilities.EnumerateApplications().ToList();
                        foreach(var appinfo in VolumeData)
                        {
                            XElement BuildNode = new XElement("Session",new XAttribute("Name",appinfo.Name),new XAttribute("Volume",appinfo.Volume));
                            RootNode.Add(BuildNode);


                        }

                        using (FileStream ftarget = new FileStream(sTargetFile, FileMode.CreateNew))
                        {
                            VolumeDoc.Save(ftarget);
                        }

                    }


                }
                else if (String.Compare(args[0], "load", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    //loads volume settings from the given file name.
                    String sLoadFile = sUseFile;
                    Console.WriteLine("Loading session volume data from " + sUseFile);
                    if (!File.Exists(sLoadFile))
                    {
                        Console.WriteLine("File not found:" +sLoadFile);
                    }
                    else
                    {

                        XDocument loadDoc = null;
                        using (FileStream fs = new FileStream(sLoadFile, FileMode.Open))
                        {
                            loadDoc = XDocument.Load(fs);
                            XElement MixerNode = loadDoc.Root;
                            float MasterVolume = 0;
                            var MasterVolumeAttrib = MixerNode.Attribute("MasterVolume");
                            if(MasterVolumeAttrib!=null)
                            {
                                MasterVolume = float.Parse(MasterVolumeAttrib.Value);
                            }
                            var ListApps = VolumeUtilities.EnumerateApplications().ToList();
                            var VolumeInfo = new Dictionary<string, VolumeUtilities.ApplicationVolumeInformation>();
                            foreach(var VolData in ListApps)
                            {
                                if(!VolumeInfo.ContainsKey(VolData.Name))
                                {
                                    VolumeInfo.Add(VolData.Name,VolData);
                                }
                            }
                            VolumeUtilities.SetMasterVolume(MasterVolume);
                            foreach(XElement LoadElement in MixerNode.Elements("Session"))
                            {
                                String sName = "";
                                float useVolume = 0;
                                XAttribute Nameattr = null, Volumeattr = null;
                                if((Nameattr = LoadElement.Attribute("Name"))!=null)
                                {
                                    sName = Nameattr.Value;
                                }
                                else if((Volumeattr = LoadElement.Attribute("Volume"))!=null)
                                {
                                    float.TryParse(Volumeattr.Value, out useVolume);
                                }
                                
                                if(VolumeInfo.ContainsKey(sName))
                                {
                                    try
                                    {
                                        VolumeInfo[sName].Volume = useVolume;
                                        Console.WriteLine("Set set volume for " + sName + " To " + useVolume);
                                    }
                                    catch(Exception exx)
                                    {
                                        Console.WriteLine("Failed Attempting to set volume for " + sName + " To " + useVolume);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Unable to set volume for " + sName + " as the Session could not be found.");
                                }

                            }



                        }


                    }
                }
                else if(String.Compare(args[0],"get",StringComparison.OrdinalIgnoreCase)==0)
                {
                    String VolumeNode = "Master";
                    if(args.Length > 1)
                    {
                        //includes the volume node (program name to get the volume of)
                        VolumeNode = args[1];
                    }
                    //get logic
                    if (VolumeNode.Equals("Master", StringComparison.OrdinalIgnoreCase))
                    {
                        float currvolume = VolumeUtilities.GetMasterVolume();
                        Console.WriteLine(currvolume);
                    }
                    else
                    {
                        var grabAppInfo = VolumeUtilities.GetApplicationVolumeInfo(VolumeNode);
                        if(grabAppInfo ==null)
                        {
                            Console.WriteLine("Unable to find Session " + VolumeNode);
                        }
                        else
                        {
                            Console.WriteLine("Current Volume of " + VolumeNode + " is " + grabAppInfo.Volume);
                        }
                    }
                }
                else if(String.Compare(args[0],"set",StringComparison.OrdinalIgnoreCase)==0)
                {
                    //set logic.
                    float assignvolume;
                    String sVolume = "";
                    String VolumeNode = "Master";
                    if(args.Length > 1)
                    {
                        VolumeNode = args[1];
                        sVolume = args[2];
                    }
                    else
                    {
                        
                        VolumeNode = "Master";
                        sVolume = args[1];
                    }
                    if(!float.TryParse(sVolume,out assignvolume))
                    {
                        Console.WriteLine("Specified volume level not valid:" + args[1]);
                        Console.WriteLine();
                        ShowHelp();
                        return;
                    }
                    else
                    {
                        if (VolumeNode.Equals("Master", StringComparison.OrdinalIgnoreCase))
                        {
                            VolumeUtilities.SetMasterVolume(assignvolume);
                            Console.WriteLine("Volume Level set to " + assignvolume);
                        }
                        else
                        {
                            var grabinfo  = VolumeUtilities.GetApplicationVolumeInfo(VolumeNode);
                            if (grabinfo != null)
                            {
                                grabinfo.Volume = assignvolume;
                                Console.WriteLine("Volume for " + VolumeNode + " Set to " + assignvolume);
                            }
                            else
                            {
                                Console.WriteLine("Unable to find " + VolumeNode);
                            }
                            
                        }
                    }
                }
                else if(String.Compare(args[0],"status",StringComparison.OrdinalIgnoreCase)==0)
                {
                    Console.WriteLine(" Master Volume is " + VolumeUtilities.GetMasterVolume());
                    foreach (var iterateApp in VolumeUtilities.EnumerateApplications())
                    {
                        var result = iterateApp.Volume;
                        if(result!=null)
                        {
                            
                            Console.WriteLine("Session:" + iterateApp.Name);
                            Console.WriteLine("ProcessID:" + iterateApp.ProcessID);
                            Console.WriteLine("Volume:" + result);
                            Console.WriteLine();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("unrecognized parameter:" + args[0]);
                    Console.WriteLine();
                    ShowHelp();
                    return;
                }


            }


            Console.ReadKey();
        }
        static void ShowHelp()
        {
            String sHelpText = String.Format(HelpText, Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
            Console.Write(sHelpText);
        }
    }

}
