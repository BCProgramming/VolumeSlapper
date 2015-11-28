using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Interfaces;

namespace VolumeSlapper
{
    //quick console program slapped together to retrieve/set volume on Windows Vista and later.
    //I guess I should have checked the OS of the issue, oh well.
    class Program
    {
        private static String HelpText = @"
VolumeSlapper Command-Line Volume Utility. v" + Assembly.GetEntryAssembly().GetName().Version.ToString() + @".
 Syntax:
 VolumeSlapper (operation) [volumelevel]
 
 operation: either get or set. get doesn't require an argument; set requires the volumelevel argument.

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
        public static float GetMasterVolume()
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
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }
       
    }


}
