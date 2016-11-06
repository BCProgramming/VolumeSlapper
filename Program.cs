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
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Interfaces;

namespace VolumeSlapper
{
    //quick console program slapped together to retrieve/set volume on Windows Vista and later.

    class Program
    {
        private static String HelpText = @"
VolumeSlapper Command-Line Volume Utility. v" + Assembly.GetEntryAssembly().GetName().Version.ToString() + @".
 Syntax:
 VolumeSlapper (operation) [arguments]
 
 operation: one of get, set, or status. set accepts an argument for the volume level.

 volumelevel: a value parsable as a floating point value between 0 and 1 to use as the volume level. used only for the set operation.

 examples:
   Retrieve current audio level:
   {0} get
   set current audio level to 50%:
   {0} set 0.5
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
                if(String.Compare(args[0],"get",StringComparison.OrdinalIgnoreCase)==0)
                {
                    //get logic
                    float currvolume = VolumeUtilities.GetMasterVolume();
                    Console.WriteLine(currvolume);
                }
                else if(String.Compare(args[0],"set",StringComparison.OrdinalIgnoreCase)==0)
                {
                    //set logic.
                    float assignvolume;
                    if(!float.TryParse(args[1],out assignvolume))
                    {
                        Console.WriteLine("Specified volume level not valid:" + args[1]);
                        Console.WriteLine();
                        ShowHelp();
                        return;
                    }
                    else
                    {
                        VolumeUtilities.SetMasterVolume(assignvolume);
                        Console.WriteLine("Volume Level set to " + assignvolume);
                    }
                }
                else if(String.Compare(args[0],"status",StringComparison.OrdinalIgnoreCase)==0)
                {
                    Console.WriteLine("Application\t\tVolume");
                    foreach (var iterateApp in VolumeUtilities.EnumerateApplications())
                    {
                        var result = iterateApp.Volume;
                        if(result!=null)
                        {
                            Console.WriteLine(iterateApp.Name + "\t\t" + result);
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
    public static class VolumeUtilities
    {
        
        public static IEnumerable<ApplicationVolumeInformation> EnumerateApplications()
        {
            // get the speakers (1st render + multimedia) device
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

            // activate the session manager. we need the enumerator
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            speakers.Activate(IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            // enumerate sessions for on this device
            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl ctl;
                sessionEnumerator.GetSession(i, out ctl);
                uint GetProcessID=0;
                String GetName = "";
                float GetVolume = 0;
                String GetIconPath = "";
                
                string dn;
                if (ctl is IAudioSessionControl2)
                {
                    IAudioSessionControl2 ctl2 = ((IAudioSessionControl2)ctl);

                    ctl2.GetProcessId(out GetProcessID);
                    ctl2.GetDisplayName(out GetName);
                    
                    String sIconPath;
                    ctl2.GetIconPath(out sIconPath);
                    ISimpleAudioVolume volcast = (ctl2 as ISimpleAudioVolume);
                    float grabvolume;
                    volcast.GetMasterVolume(out grabvolume);
                    
                    Process grabProcess = Process.GetProcessById((int)GetProcessID);
                    if(String.IsNullOrEmpty(GetName))
                    {
                        GetName = grabProcess.ProcessName;
                    }
                    
                }
                ApplicationVolumeInformation avi = new ApplicationVolumeInformation(GetProcessID,GetVolume,GetName,GetIconPath);
                
                yield return avi;
                Marshal.ReleaseComObject(ctl);
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(deviceEnumerator);
        }
        public static float GetMasterVolume()
        {
            // retrieve audio device...
            IMMDeviceEnumerator useenumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            const int eRender = 0;
            //retrieve the actual endpoint
            
            useenumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

            object o;
            //retrieve the actual interface instance to retrieve the volume information from.
            speakers.Activate(typeof(IAudioEndpointVolume).GUID, 0, IntPtr.Zero, out o);
            IAudioEndpointVolume aepv = (IAudioEndpointVolume)o;
            float result;
            aepv.GetMasterVolumeLevelScalar(out result);
            Marshal.ReleaseComObject(aepv);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(useenumerator);
            return result;
        }
        public static float SetMasterVolume(float newValue)
        {
            // retrieve audio device...
            
            IMMDeviceEnumerator useenumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            const int eRender = 0;
            const int eMultimedia = 1;
            //retrieve the actual endpoint
            useenumerator.GetDefaultAudioEndpoint(eRender, ERole.eMultimedia, out speakers);

            object o;
            //retrieve the actual interface instance to retrieve the volume information from.
            speakers.Activate(typeof(IAudioEndpointVolume).GUID, 0, IntPtr.Zero, out o);
            IAudioEndpointVolume aepv = (IAudioEndpointVolume)o;
            float result;
            int hresult = aepv.GetMasterVolumeLevelScalar(out result);
            aepv.SetMasterVolumeLevelScalar(newValue,new System.Guid());
            Marshal.ReleaseComObject(aepv);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(useenumerator);
            return result;
        }
        public static float? GetApplicationVolume(string name)
        {
            ISimpleAudioVolume volume = GetVolumeObject(name);
            if (volume == null)
                return null;

            float level;
            volume.GetMasterVolume(out level);
            return level * 100;
        }

        public static bool? GetApplicationMute(string name)
        {
            ISimpleAudioVolume volume = GetVolumeObject(name);
            if (volume == null)
                return null;
            
            bool mute;
            volume.GetMute(out mute);
            return mute;
        }

        public static void SetApplicationVolume(string name, float level)
        {
            ISimpleAudioVolume volume = GetVolumeObject(name);
            if (volume == null)
                return;

            Guid guid = Guid.Empty;
            volume.SetMasterVolume(level / 100, guid);
        }

        public static void SetApplicationMute(string name, bool mute)
        {
            ISimpleAudioVolume volume = GetVolumeObject(name);
            if (volume == null)
                return;

            Guid guid = Guid.Empty;
            volume.SetMute(mute, guid);
        }
        private static ISimpleAudioVolume GetVolumeObject(string name)
        {
            // get the speakers (1st render + multimedia) device
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

            // activate the session manager. we need the enumerator
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            speakers.Activate(IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            // enumerate sessions for on this device
            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            // search for an audio session with the required name
            // NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
            ISimpleAudioVolume volumeControl = null;
            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl ctl;
                sessionEnumerator.GetSession(i, out ctl);
                string dn;
                ctl.GetDisplayName(out dn);
                if (string.Compare(name, dn, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    volumeControl = ctl as ISimpleAudioVolume;
                    break;
                }
                Marshal.ReleaseComObject(ctl);
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(deviceEnumerator);
            return volumeControl;
        }


        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        public class ApplicationVolumeInformation
        {
            public uint ProcessID { get; set; }
            public float Volume { get; set; }

            public String  Name { get; set; }
            
            public String IconPath { get; set; }
            public ApplicationVolumeInformation(uint pProcessID,float pVolume,String pName,String pIconPath)
            {
                ProcessID = pProcessID;
                Volume = pVolume;
                Name = pName;
                IconPath = pIconPath;
            }
        }
    }
}
