using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JosLibrary.Audio
{
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }

    internal enum EDataFlow
    {
        eRender,
        eCapture,
        eAll,
        EDataFlow_enum_count
    }

    internal enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications,
        ERole_enum_count
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int NotImpl1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        // the rest is not implemented
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        // the rest is not implemented
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int NotImpl1();
        int NotImpl2();

        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

        // the rest is not implemented
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig]
        int GetCount(out int SessionCount);

        [PreserveSig]
        int GetSession(int SessionCount, out IAudioSessionControl Session);
    }

    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl
    {
        int NotImpl1();

        [PreserveSig]
        int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        void GetProcessID(out string dn);

        // the rest is not implemented
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);

        [PreserveSig]
        int GetMasterVolume(out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid EventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///// audio utilities
    public static class AudioUtilities
    {
        private static IAudioSessionManager2 GetAudioSessionManager()
        {
            IMMDevice speakers = GetSpeakers();
            if (speakers == null)
                return  null;

            // win7+ only
            object o;
            if (speakers.Activate(typeof(IAudioSessionManager2).GUID, CLSCTX.CLSCTX_ALL, IntPtr.Zero, out o) != 0 || o == null)
                return null;

            return o as IAudioSessionManager2;
        }

        public static AudioDevice GetSpeakersDevice()
        {
            return CreateDevice(GetSpeakers());
        }

        /// <summary>
        /// Gets the current master volume in scalar values (percentage)
        /// </summary>
        /// <returns>-1 in case of an error, if successful the value will be between 0 and 100</returns>
        public static float GetMasterVolume()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return -1;

                float volumeLevel;
                masterVol.GetMasterVolumeLevelScalar(out volumeLevel);
                return volumeLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Gets the mute state of the master volume. 
        /// While the volume can be muted the <see cref="GetMasterVolume"/> will still return the pre-muted volume value.
        /// </summary>
        /// <returns>false if not muted, true if volume is muted</returns>
        public static bool GetMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return false;

                bool isMuted;
                masterVol.GetMute(out isMuted);
                return isMuted;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Sets the master volume to a specific level
        /// </summary>
        /// <param name="newLevel">Value between 0 and 100 indicating the desired scalar value of the volume</param>
        public static void SetMasterVolume(float newLevel)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return;

                masterVol.SetMasterVolumeLevelScalar(newLevel / 100, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Increments or decrements the current volume level by the <see cref="stepAmount"/>.
        /// </summary>
        /// <param name="stepAmount">Value between -100 and 100 indicating the desired step amount. Use negative numbers to decrease
        /// the volume and positive numbers to increase it.</param>
        /// <returns>the new volume level assigned</returns>
        public static float StepMasterVolume(float stepAmount)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return -1;

                float stepAmountScaled = stepAmount / 100;

                // Get the level
                float volumeLevel;
                masterVol.GetMasterVolumeLevelScalar(out volumeLevel);

                // Calculate the new level
                float newLevel = volumeLevel + stepAmountScaled;
                newLevel = Math.Min(1, newLevel);
                newLevel = Math.Max(0, newLevel);

                masterVol.SetMasterVolumeLevelScalar(newLevel, Guid.Empty);

                // Return the new volume level that was set
                return newLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Mute or unmute the master volume
        /// </summary>
        /// <param name="isMuted">true to mute the master volume, false to unmute</param>
        public static void SetMasterVolumeMute(bool isMuted)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return;

                masterVol.SetMute(isMuted, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Switches between the master volume mute states depending on the current state
        /// </summary>
        /// <returns>the current mute state, true if the volume was muted, false if unmuted</returns>
        public static bool ToggleMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return false;

                bool isMuted;
                masterVol.GetMute(out isMuted);
                masterVol.SetMute(!isMuted, Guid.Empty);

                return !isMuted;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        private static IAudioEndpointVolume GetMasterVolumeObject()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDevice speakers = null;
            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                object o;
                speakers.Activate(IID_IAudioEndpointVolume, 0, IntPtr.Zero, out o);
                IAudioEndpointVolume masterVol = (IAudioEndpointVolume)o;

                return masterVol;
            }
            finally
            {
                if (speakers != null) Marshal.ReleaseComObject(speakers);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        private static AudioDevice CreateDevice(IMMDevice dev)
        {
            if (dev == null)
                return null;

            string id;
            dev.GetId(out id);
            DEVICE_STATE state;
            dev.GetState(out state);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            IPropertyStore store;
            dev.OpenPropertyStore(STGM.STGM_READ, out store);
            if (store != null)
            {
                int propCount;
                store.GetCount(out propCount);
                for (int j = 0; j < propCount; j++)
                {
                    PROPERTYKEY pk;
                    if (store.GetAt(j, out pk) == 0)
                    {
                        PROPVARIANT value = new PROPVARIANT();
                        int hr = store.GetValue(ref pk, ref value);
                        object v = value.GetValue();
                        try
                        {
                            if (value.vt != VARTYPE.VT_BLOB) // for some reason, this fails?
                            {
                                PropVariantClear(ref value);
                            }
                        }
                        catch
                        {
                        }
                        string name = pk.ToString();
                        properties[name] = v;
                    }
                }
            }
            return new AudioDevice(id, (AudioDeviceState)state, properties);
        }

        public static IList<AudioDevice> GetAllDevices()
        {
            List<AudioDevice> list = new List<AudioDevice>();
            IMMDeviceEnumerator deviceEnumerator = null;
            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            }
            catch
            {
            }
            if (deviceEnumerator == null)
                return list;

            IMMDeviceCollection collection;
            deviceEnumerator.EnumAudioEndpoints(EDataFlow.eAll, DEVICE_STATE.MASK_ALL, out collection);
            if (collection == null)
                return list;

            int count;
            collection.GetCount(out count);
            for (int i = 0; i < count; i++)
            {
                IMMDevice dev;
                collection.Item(i, out dev);
                if (dev != null)
                {
                    list.Add(CreateDevice(dev));
                }
            }
            return list;
        }

        private static IMMDevice GetSpeakers()
        {
            // get the speakers (1st render + multimedia) device
            try
            {
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
                return speakers;
            }
            catch
            {
                return null;
            }
        }

        public static IList<AudioSession> GetAllSessions()
        {
            List<AudioSession> list = new List<AudioSession>();
            IAudioSessionManager2 mgr = GetAudioSessionManager();
            if (mgr == null)
                return list;

            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl ctl;
                sessionEnumerator.GetSession(i, out ctl);
                if (ctl == null)
                    continue;

                IAudioSessionControl2 ctl2 = ctl as IAudioSessionControl2;
                if (ctl2 != null)
                {
                    list.Add(new AudioSession(ctl2));
                }
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            return list;
        }

        public static AudioSession GetProcessSession()
        {
            int id = Process.GetCurrentProcess().Id;
            foreach (AudioSession session in GetAllSessions())
            {
                if (session.ProcessId == id)
                    return session;

                session.Dispose();
            }
            return null;
        }

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PROPVARIANT pvar);

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        [Flags]
        private enum CLSCTX
        {
            CLSCTX_INPROC_SERVER = 0x1,
            CLSCTX_INPROC_HANDLER = 0x2,
            CLSCTX_LOCAL_SERVER = 0x4,
            CLSCTX_REMOTE_SERVER = 0x10,
            CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
        }

        private enum STGM
        {
            STGM_READ = 0x00000000,
        }

        private enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
        }

        private enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
        }

        private enum DEVICE_STATE
        {
            ACTIVE = 0x00000001,
            DISABLED = 0x00000002,
            NOTPRESENT = 0x00000004,
            UNPLUGGED = 0x00000008,
            MASK_ALL = 0x0000000F
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPERTYKEY
        {
            public Guid fmtid;
            public int pid;

            public override string ToString()
            {
                return fmtid.ToString("B") + " " + pid;
            }
        }

        // NOTE: we only define what we handle
        [Flags]
        private enum VARTYPE : short
        {
            VT_I4 = 3,
            VT_BOOL = 11,
            VT_UI4 = 19,
            VT_LPWSTR = 31,
            VT_BLOB = 65,
            VT_CLSID = 72,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPVARIANT
        {
            public VARTYPE vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;
            public PROPVARIANTunion union;

            public object GetValue()
            {
                switch (vt)
                {
                    case VARTYPE.VT_BOOL:
                        return union.boolVal != 0;

                    case VARTYPE.VT_LPWSTR:
                        return Marshal.PtrToStringUni(union.pwszVal);

                    case VARTYPE.VT_UI4:
                        return union.lVal;

                    case VARTYPE.VT_CLSID:
                        return (Guid)Marshal.PtrToStructure(union.puuid, typeof(Guid));

                    default:
                        return vt.ToString() + ":?";
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PROPVARIANTunion
        {
            [FieldOffset(0)]
            public int lVal;
            [FieldOffset(0)]
            public ulong uhVal;
            [FieldOffset(0)]
            public short boolVal;
            [FieldOffset(0)]
            public IntPtr pwszVal;
            [FieldOffset(0)]
            public IntPtr puuid;
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            [PreserveSig]
            int EnumAudioEndpoints(EDataFlow dataFlow, DEVICE_STATE dwStateMask, out IMMDeviceCollection ppDevices);

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);

            [PreserveSig]
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);

            [PreserveSig]
            int RegisterEndpointNotificationCallback(IMMNotificationClient pClient);

            [PreserveSig]
            int UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
        }

        [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMNotificationClient
        {
            void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, DEVICE_STATE dwNewState);
            void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);
            void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
            void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId);
            void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PROPERTYKEY key);
        }

        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            [PreserveSig]
            int GetCount(out int pcDevices);

            [PreserveSig]
            int Item(int nDevice, out IMMDevice ppDevice);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate([MarshalAs(UnmanagedType.LPStruct)] Guid riid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            [PreserveSig]
            int OpenPropertyStore(STGM stgmAccess, out IPropertyStore ppProperties);

            [PreserveSig]
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

            [PreserveSig]
            int GetState(out DEVICE_STATE pdwState);
        }

        [Guid("6f79d558-3e96-4549-a1d1-7d75d2288814"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyDescription
        {
            [PreserveSig]
            int GetPropertyKey(out PROPERTYKEY pkey);

            [PreserveSig]
            int GetCanonicalName(out IntPtr ppszName);

            [PreserveSig]
            int GetPropertyType(out short pvartype);

            [PreserveSig]
            int GetDisplayName(out IntPtr ppszName);

            // WARNING: the rest is undefined. you *can't* implement it, only use it.
        }

        [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out int cProps);

            [PreserveSig]
            int GetAt(int iProp, out PROPERTYKEY pkey);

            [PreserveSig]
            int GetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);

            [PreserveSig]
            int SetValue(ref PROPERTYKEY key, ref PROPVARIANT propvar);

            [PreserveSig]
            int Commit();
        }

        [Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager
        {
            [PreserveSig]
            int GetAudioSessionControl([MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid, int StreamFlags, out IAudioSessionControl SessionControl);

            [PreserveSig]
            int GetSimpleAudioVolume([MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid, int StreamFlags, ISimpleAudioVolume AudioVolume);
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            [PreserveSig]
            int GetAudioSessionControl([MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid, int StreamFlags, out IAudioSessionControl SessionControl);

            [PreserveSig]
            int GetSimpleAudioVolume([MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid, int StreamFlags, ISimpleAudioVolume AudioVolume);

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

            [PreserveSig]
            int RegisterSessionNotification(IAudioSessionNotification SessionNotification);

            [PreserveSig]
            int UnregisterSessionNotification(IAudioSessionNotification SessionNotification);

            int RegisterDuckNotificationNotImpl();
            int UnregisterDuckNotificationNotImpl();
        }

        [Guid("641DD20B-4D41-49CC-ABA3-174B9477BB08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionNotification
        {
            void OnSessionCreated(IAudioSessionControl NewSession);
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl2
        {
            // IAudioSessionControl
            [PreserveSig]
            int GetState(out AudioSessionState pRetVal);

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetGroupingParam(out Guid pRetVal);

            [PreserveSig]
            int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);

            [PreserveSig]
            int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);

            // IAudioSessionControl2
            [PreserveSig]
            int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetProcessId(out int pRetVal);

            [PreserveSig]
            int IsSystemSoundsSession();

            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }

        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
            [PreserveSig]
            int GetState(out AudioSessionState pRetVal);

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetGroupingParam(out Guid pRetVal);

            [PreserveSig]
            int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);

            [PreserveSig]
            int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        }

        [Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEvents
        {
            void OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            void OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            void OnSimpleVolumeChanged(float NewVolume, bool NewMute, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            void OnChannelVolumeChanged(int ChannelCount, IntPtr NewChannelVolumeArray, int ChangedChannel, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            void OnGroupingParamChanged([MarshalAs(UnmanagedType.LPStruct)] Guid NewGroupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            void OnStateChanged(AudioSessionState NewState);
            void OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason);
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }
    }
}
