using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Interfaces;

namespace VolumeLib
{
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
                uint GetProcessID = 0;
                String GetName = "";
                float GetVolume = 0;
                String GetIconPath = "";
                IAudioSessionControl getsession = null;
                getsession = ctl;
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
                    GetVolume = grabvolume;
                    try
                    {
                        Process grabProcess = Process.GetProcessById((int)GetProcessID);
                        if (String.IsNullOrEmpty(GetName))
                        {
                            GetName = grabProcess.ProcessName;
                        }
                    }
                    catch(Exception exx)
                    {
                        GetName = "Name Not Available";
                    }

                }
                ApplicationVolumeInformation avi = new ApplicationVolumeInformation(getsession, GetProcessID, GetVolume, GetName, GetIconPath);

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
            aepv.SetMasterVolumeLevelScalar(newValue, new System.Guid());
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
        public static ApplicationVolumeInformation GetApplicationVolumeInfo(String sName)
        {
            foreach (var iterateNode in EnumerateApplications())
            {
                if (iterateNode.Name.Equals(sName, StringComparison.OrdinalIgnoreCase))
                {
                    return iterateNode;
                }
            }
            return null;
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
            private IAudioSessionControl AudioSession { get; set; }
            public uint ProcessID { get; set; }
            private float _Volume = 0;

            public float Volume
            {
                get
                {
                    return _Volume;
                }
                set
                {
                    _Volume = value;
                    if (AudioSession != null)
                    {
                        ISimpleAudioVolume vol = AudioSession as ISimpleAudioVolume;
                        vol?.SetMasterVolume(_Volume, Guid.Empty);
                    }

                }
            }

            public String Name { get; private set; }

            public String IconPath { get; private set; }
            public ApplicationVolumeInformation(IAudioSessionControl Session, uint pProcessID, float pVolume, String pName, String pIconPath)
            {
                AudioSession = Session;
                ProcessID = pProcessID;
                _Volume = pVolume;
                Name = pName;
                IconPath = pIconPath;
            }
        }
    }
}
