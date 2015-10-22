using System;
using System.ComponentModel; // for Win32Exception
using System.Runtime.InteropServices;
using System.Text; // for StringBuilder

namespace FliSharp
{
    class FLI : IDisposable
    {
        //
        #region Constants
        //

        /// <summary>
        /// max string size for all APIs
        /// </summary>
        public const int MAX_STRING_LEN = 256;

        /// <summary>
        /// represents a closed device handle (instead of IntPtr.Zero)
        /// </summary>
        private const int INVALID_DEVICE = -1;

        #endregion

        //
        #region Enums
        //

        /// <summary>
        /// The domain of an FLI device.  This consists of a bitwise ORed
        /// combination of interface method and device type.  Valid interfaces
        /// are \texttt{FLIDOMAIN_PARALLEL_PORT}, \texttt{FLIDOMAIN_USB},
        /// \texttt{FLIDOMAIN_SERIAL}, and \texttt{FLIDOMAIN_INET}.  Valid
        /// device types are \texttt{FLIDEVICE_CAMERA},
        /// \texttt{FLIDOMAIN_FILTERWHEEL}, and \texttt{FLIDOMAIN_FOCUSER}.
        ///    
        /// @see FLIOpen
        /// @see FLIList
        /// </summary>
        [Flags]
        public enum DOMAIN : int
        {
            NONE = 0x00,
            PARALLEL_PORT = 0x01,
            USB = 0x02,
            SERIAL = 0x03,
            INET = 0x04,
            SERIAL_19200 = 0x05,
            SERIAL_1200 = 0x06,

            CAMERA = 0x100,
            FILTERWHEEL = 0x200,
            FOCUSER = 0x300,
            HS_FILTERWHEEL = 0x0f00,
            RAW = 0x500,
            ENUMERATE_BY_CONNECTION = 0x8000
        }

        /// <summary>
        /// The frame type for an FLI CCD camera device.  Valid frame types are
        /// \texttt{FLI_FRAME_TYPE_NORMAL} and \texttt{FLI_FRAME_TYPE_DARK}.
        ///
        /// @see FLISetFrameType
        /// </summary>
        [Flags]
        public enum FRAME_TYPE : int
        {
            NORMAL = 0,
            DARK = 1,
            FLOOD = 2,
            RBI_FLUSH = DARK | FLOOD
        }

        /// <summary>
        /// The gray-scale bit depth for an FLI camera device.  Valid bit
        /// depths are \texttt{FLI_MODE_8BIT} and \texttt{FLI_MODE_16BIT}.
        ///
        /// @see FLISetBitDepth
        /// </summary>
        public enum BIT_DEPTH : int
        {
            MODE_8BIT = 0,
            MODE_16BIT = 1
        }

        /// <summary>
        /// Type used for shutter operations for an FLI camera device.  Valid
        /// shutter types are \texttt{FLI_SHUTTER_CLOSE},
        /// \texttt{FLI_SHUTTER_OPEN},
        /// \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER},
        /// \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER_LOW}, and
        /// \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER_HIGH}.
        ///
        /// @see FLIControlShutter
        /// </summary>
        public enum SHUTTER : int
        {
            CLOSE = 0x0000,
            OPEN = 0x0001,
            EXTERNAL_TRIGGER = 0x0002,
            EXTERNAL_TRIGGER_LOW = 0x0002,
            EXTERNAL_TRIGGER_HIGH = 0x0004,
            EXPOSURE_CONTROL = 0x0008
        }

        /// <summary>
        /// Type used for background flush operations for an FLI camera device.  Valid
        /// bgflush types are \texttt{FLI_BGFLUSH_STOP} and
        /// \texttt{FLI_BGFLUSH_START}.
        ///
        /// @see FLIControlBackgroundFlush
        /// </summary>
        public enum BGFLUSH : int
        {
            STOP = 0x0000,
            START = 0x0001
        }

        /// <summary>
        /// Type used to determine which temperature channel to read.  Valid
        /// channel types are \texttt{FLI_TEMPERATURE_INTERNAL} and
        /// \texttt{FLI_TEMPERATURE_EXTERNAL}.
        ///
        /// @see FLIReadTemperature
        /// </summary>
        public enum CHANNEL : int
        {
            INTERNAL = 0x0000,
            EXTERNAL = 0x0001,
            CCD = 0x0000,
            BASE = 0x0001
        }

        /// <summary>
        /// Type specifying library debug levels.  Valid debug levels are
        /// \texttt{FLIDEBUG_NONE}, \texttt{FLIDEBUG_INFO},
        /// \texttt{FLIDEBUG_WARN}, and \texttt{FLIDEBUG_FAIL}.
        ///
        /// @see FLISetDebugLevel
        /// </summary>
        [Flags]
        public enum DEBUG : int
        {
            NONE = 0x00,
            INFO = 0x01,
            WARN = 0x02,
            FAIL = 0x04,
            IO = 0x08,
            ALL = INFO | WARN | FAIL
        }

        [Flags]
        public enum FAN_SPEED : int
        {
            OFF = 0x00,
            ON = unchecked((int)0xffffffff)
        }

        /// <summary>
        /// Status settings
        /// </summary>
        [Flags]
        public enum STATUS : int
        {
            CAMERA_STATUS_UNKNOWN = unchecked((int)0xffffffff),
            CAMERA_STATUS_MASK = 0x00000003,
            CAMERA_STATUS_IDLE = 0x00,
            CAMERA_STATUS_WAITING_FOR_TRIGGER = 0x01,
            CAMERA_STATUS_EXPOSING = 0x02,
            CAMERA_STATUS_READING_CCD = 0x03,
            CAMERA_DATA_READY = unchecked((int)0x80000000),

            FOCUSER_STATUS_UNKNOWN = unchecked((int)0xffffffff),
            FOCUSER_STATUS_HOMING = 0x00000004,
            FOCUSER_STATUS_MOVING_IN = 0x00000001,
            FOCUSER_STATUS_MOVING_OUT = 0x00000002,
            FOCUSER_STATUS_MOVING_MASK = 0x00000007,
            FOCUSER_STATUS_HOME = 0x00000080,
            FOCUSER_STATUS_LIMIT = 0x00000040,
            FOCUSER_STATUS_LEGACY = 0x10000000,

            FILTER_WHEEL_PHYSICAL = 0x100,
            FILTER_WHEEL_VIRTUAL = 0,
            FILTER_WHEEL_LEFT = FILTER_WHEEL_PHYSICAL | 0x00,
            FILTER_WHEEL_RIGHT = FILTER_WHEEL_PHYSICAL | 0x01,
            FILTER_STATUS_MOVING_CCW = 0x01,
            FILTER_STATUS_MOVING_CW = 0x02,
            FILTER_POSITION_UNKNOWN = 0xff,
            FILTER_POSITION_CURRENT = 0x200,
            FILTER_STATUS_HOMING = 0x00000004,
            FILTER_STATUS_HOME = 0x00000080,
            FILTER_STATUS_HOME_LEFT = 0x00000080,
            FILTER_STATUS_HOME_RIGHT = 0x00000040,
            FILTER_STATUS_HOME_SUCCEEDED = 0x00000008
        }

        #endregion // enums

        //
        #region Members
        //

        /// <summary>
        /// Stores a handle to the device once it's been opened
        /// </summary>
        IntPtr dev = (IntPtr)INVALID_DEVICE;
        
        /// <summary>
        /// for IDisposable pattern
        /// </summary>
        bool disposed = false;

        #endregion

        //
        #region static APIs
        //

        [DllImport("libfli.dll")]
        private static extern int FLISetDebugLevel(string host, DEBUG level);
        public static void SetDebugLevel(string host, DEBUG level)
        {
            int status = FLISetDebugLevel(host, level);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetLibVersion(StringBuilder ver, int len);
        public static string GetLibVersion()
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetLibVersion(sb, sb.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            return sb.ToString();
        }

        /// <summary>
        /// TODO: This leaks the triple pointer assigned by FLIList
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        [DllImport("libfli.dll")]
        private static extern int FLIList(DOMAIN domain, out string[] names);
        public static void List(DOMAIN domain, out string[] names)
        {
            int status = FLIList(domain, out names);
            if (0 != status)
                throw new Win32Exception(-status);
        }
/*
        [DllImport("libfli.dll")]
        private static extern int FLIFreeList(string[] names);
        public void FreeList(string[] names)
        {
            int status = FLIFreeList(names);
            if (0 != status)
                throw new Win32Exception(-status);
        }
*/

        [DllImport("libfli.dll")]
        private static extern int FLICreateList(DOMAIN domain);
        public static void CreateList(DOMAIN domain)
        {
            int status = FLICreateList(domain);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIDeleteList();
        public static void DeleteList()
        {
            int status = FLIDeleteList();
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIListFirst(out DOMAIN domain, StringBuilder filename, int fnlen, StringBuilder name, int namelen);
        public static void ListFirst(out DOMAIN domain, out string filename, out string name)
        {
            StringBuilder sbFilename = new StringBuilder(MAX_STRING_LEN);
            StringBuilder sbName = new StringBuilder(MAX_STRING_LEN);
            int status = FLIListFirst(out domain, sbFilename, sbFilename.MaxCapacity, sbName, sbName.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            filename = sbFilename.ToString();
            name = sbName.ToString();
        }

        [DllImport("libfli.dll")]
        private static extern int FLIListNext(out DOMAIN domain, StringBuilder filename, int fnlen, StringBuilder name, int namelen);
        public static void ListNext(out DOMAIN domain, out string filename, out string name)
        {
            StringBuilder sbFilename = new StringBuilder(MAX_STRING_LEN);
            StringBuilder sbName = new StringBuilder(MAX_STRING_LEN);
            int status = FLIListNext(out domain, sbFilename, sbFilename.MaxCapacity, sbName, sbName.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            filename = sbFilename.ToString();
            name = sbName.ToString();
        }

        #endregion

        //
        #region device APIs
        //

        [DllImport("libfli.dll")]
        private static extern int FLIOpen(out IntPtr dev, string name, DOMAIN domain);
        public FLI(string name, DOMAIN domain)
        {
            int status = FLIOpen(out dev, name, domain);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIClose(IntPtr dev);
        public void Close()
        {
            // don't do anything if there's nothing to do
            if ((IntPtr)INVALID_DEVICE == dev)
                return;

            int status = FLIClose(dev);
            if (0 != status)
                throw new Win32Exception(-status);
            // "null" out the handle
            dev = (IntPtr)INVALID_DEVICE;
        }

        ~FLI()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // this space intentionally left blank
            }

            Close();
            disposed = true;
        }
        
        [DllImport("libfli.dll")]
        private static extern int FLIGetModel(IntPtr dev, StringBuilder model, int len);
        public string GetModel()
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetModel(dev, sb, sb.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            return sb.ToString();
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetPixelSize(IntPtr dev, out double pixel_x, out double pixel_y);
        public void GetPixelSize(out double pixel_x, out double pixel_y)
        {
            int status = FLIGetPixelSize(dev, out pixel_x, out pixel_y);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetHWRevision(IntPtr dev, out int hwrev);
        public int GetHWRevision()
        {
            int hwrev;
            int status = FLIGetHWRevision(dev, out hwrev);
            if (0 != status)
                throw new Win32Exception(-status);
            return hwrev;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetFWRevision(IntPtr dev, out int fwrev);
        public int GetFWRevision()
        {
            int fwrev;
            int status = FLIGetFWRevision(dev, out fwrev);
            if (0 != status)
                throw new Win32Exception(-status);
            return fwrev;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetArrayArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y);
        public void GetArrayArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            int status = FLIGetArrayArea(dev, out ul_x, out ul_y, out lr_x, out lr_y);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetVisibleArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y);
        public void GetVisibleArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            int status = FLIGetVisibleArea(dev, out ul_x, out ul_y, out lr_x, out lr_y);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetExposureTime(IntPtr dev, int exptime);
        public void SetExposureTime(int exptime)
        {
            int status = FLISetExposureTime(dev, exptime);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetImageArea(IntPtr dev, int ul_x, int ul_y, int lr_x, int lr_y);
        public void SetImageArea(int ul_x, int ul_y, int lr_x, int lr_y)
        {
            int status = FLISetImageArea(dev, ul_x, ul_y, lr_x, lr_y);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetHBin(IntPtr dev, int hbin);
        public void SetHBin(int hbin)
        {
            int status = FLISetHBin(dev, hbin);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetVBin(IntPtr dev, int vbin);
        public void SetVBin(int vbin)
        {
            int status = FLISetVBin(dev, vbin);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetFrameType(IntPtr dev, FRAME_TYPE frametype);
        public void SetFrameType(FRAME_TYPE frametype)
        {
            int status = FLISetFrameType(dev, frametype);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLICancelExposure(IntPtr dev);
        public void CancelExposure()
        {
            int status = FLICancelExposure(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetExposureStatus(IntPtr dev, out int timeleft);
        public int GetExposureStatus()
        {
            int timeleft;
            int status = FLIGetExposureStatus(dev, out timeleft);
            if (0 != status)
                throw new Win32Exception(-status);
            return timeleft;
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetTemperature(IntPtr dev, double temperature);
        public void SetTemperature(double temperature)
        {
            int status = FLISetTemperature(dev, temperature);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetTemperature(IntPtr dev, out double temperature);
        public double GetTemperature()
        {
            double temperature;
            int status = FLIGetTemperature(dev, out temperature);
            if (0 != status)
                throw new Win32Exception(-status);
            return temperature;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetCoolerPower(IntPtr dev, out double power);
        public double GetCoolerPower()
        {
            double power;
            int status = FLIGetCoolerPower(dev, out power);
            if (0 != status)
                throw new Win32Exception(-status);
            return power;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGrabRow(IntPtr dev, IntPtr buff, int width);
        public void GrabRow(byte[] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabRow(dev, BuffPtr, buff.Length * sizeof(byte));
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }
        public void GrabRow(ushort[] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabRow(dev, BuffPtr, buff.Length * sizeof(ushort));
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }

        [DllImport("libfli.dll")]
        private static extern int FLIExposeFrame(IntPtr dev);
        public void ExposeFrame()
        {
            int status = FLIExposeFrame(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIFlushRow(IntPtr dev, int rows, int repeat);
        public void FlushRow(int rows, int repeat)
        {
            int status = FLIFlushRow(dev, rows, repeat);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetNFlushes(IntPtr dev, int nflushes);
        public void SetNFlushes(int nflushes)
        {
            int status = FLISetNFlushes(dev, nflushes);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetBitDepth(IntPtr dev, BIT_DEPTH bitdepth);
        public void SetBitDepth(BIT_DEPTH bitdepth)
        {
            int status = FLISetBitDepth(dev, bitdepth);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIReadIOPort(IntPtr dev, out int ioportset);
        public int ReadIOPort()
        {
            int ioportset;
            int status = FLIReadIOPort(dev, out ioportset);
            if (0 != status)
                throw new Win32Exception(-status);
            return ioportset;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIWriteIOPort(IntPtr dev, int ioportset);
        public void WriteIOPort(int ioportset)
        {
            int status = FLIWriteIOPort(dev, ioportset);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIConfigureIOPort(IntPtr dev, int ioportset);
        public void ConfigureIOPort(int ioportset)
        {
            int status = FLIConfigureIOPort(dev, ioportset);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLILockDevice(IntPtr dev);
        public void LockDevice()
        {
            int status = FLILockDevice(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIUnlockDevice(IntPtr dev);
        public void UnlockDevice()
        {
            int status = FLIUnlockDevice(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIControlShutter(IntPtr dev, SHUTTER shutter);
        public void ControlShutter(SHUTTER shutter)
        {
            int status = FLIControlShutter(dev, shutter);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIControlBackgroundFlush(IntPtr dev, BGFLUSH bgflush);
        public void ControlBackgroundFlush(BGFLUSH bgflush)
        {
            int status = FLIControlBackgroundFlush(dev, bgflush);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetDAC(IntPtr dev, uint dacset);
        public void SetDAC(uint dacset)
        {
            int status = FLISetDAC(dev, dacset);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetFilterName(IntPtr dev, int filter, StringBuilder name, int len);
        public string GetFilterName(int filter)
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetFilterName(dev, filter, sb, sb.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            return sb.ToString();
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetActiveWheel(IntPtr dev, int wheel);
        public void SetActiveWheel(int wheel)
        {
            int status = FLISetActiveWheel(dev, wheel);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetActiveWheel(IntPtr dev, out int wheel);
        public int GetActiveWheel()
        {
            int wheel;
            int status = FLIGetActiveWheel(dev, out wheel);
            if (0 != status)
                throw new Win32Exception(-status);
            return wheel;
        }


        [DllImport("libfli.dll")]
        private static extern int FLISetFilterPos(IntPtr dev, int filter);
        public void SetFilterPos(int filter)
        {
            int status = FLISetFilterPos(dev, filter);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetFilterPos(IntPtr dev, out int filter);
        public int GetFilterPos()
        {
            int filter;
            int status = FLIGetFilterPos(dev, out filter);
            if (0 != status)
                throw new Win32Exception(-status);
            return filter;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetFilterCount(IntPtr dev, out int filter);
        public int GetFilterCount()
        {
            int filter;
            int status = FLIGetFilterCount(dev, out filter);
            if (0 != status)
                throw new Win32Exception(-status);
            return filter;
        }


        [DllImport("libfli.dll")]
        private static extern int FLIStepMotor(IntPtr dev, int steps);
        public void StepMotor(int steps)
        {
            int status = FLIStepMotor(dev, steps);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIStepMotorAsync(IntPtr dev, int steps);
        public void StepMotorAsync(int steps)
        {
            int status = FLIStepMotorAsync(dev, steps);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetStepperPosition(IntPtr dev, out int position);
        public int GetStepperPosition()
        {
            int position;
            int status = FLIGetStepperPosition(dev, out position);
            if (0 != status)
                throw new Win32Exception(-status);
            return position;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetStepsRemaining(IntPtr dev, out int steps);
        public int GetStepsRemaining()
        {
            int steps;
            int status = FLIGetStepsRemaining(dev, out steps);
            if (0 != status)
                throw new Win32Exception(-status);
            return steps;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIHomeFocuser(IntPtr dev);
        public void HomeFocuser()
        {
            int status = FLIHomeFocuser(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIReadTemperature(IntPtr dev, CHANNEL channel, out double temperature);
        public double ReadTemperature(CHANNEL channel)
        {
            double temperature;
            int status = FLIReadTemperature(dev, channel, out temperature);
            if (0 != status)
                throw new Win32Exception(-status);
            return temperature;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetFocuserExtent(IntPtr dev, out int extent);
        public int GetFocuserExtent()
        {
            int extent;
            int status = FLIGetFocuserExtent(dev, out extent);
            if (0 != status)
                throw new Win32Exception(-status);
            return extent;
        }

        /// <summary>
        /// TODO: No docs on this function, a bit unclear what it's supposed to be doing
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="ep"></param>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport("libfli.dll")]
        private static extern int FLIUsbBulkIO(IntPtr dev, int ep, IntPtr buf, out int len);
        public int UsbBulkIO(int ep, byte[] buf)
        {
            int len;
            GCHandle BufGch = GCHandle.Alloc(buf, GCHandleType.Pinned);
            IntPtr BufPtr = BufGch.AddrOfPinnedObject();

            try
            {
                int status = FLIUsbBulkIO(dev, ep, BufPtr, out len);
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BufGch.Free();
            }
            return len;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetDeviceStatus(IntPtr dev, out int status);
        public int GetDeviceStatus()
        {
            int status;
            int Status = FLIGetDeviceStatus(dev, out status);
            if (0 != Status)
                throw new Win32Exception(-Status);
            return status;
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetCameraModeString(IntPtr dev, int mode_index, StringBuilder mode_string, int siz);
        public string GetCameraModeString(int mode_index)
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetCameraModeString(dev, mode_index, sb, sb.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            return sb.ToString();
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetCameraMode(IntPtr dev, out int mode_index);
        public int GetCameraMode()
        {
            int mode_index;
            int status = FLIGetCameraMode(dev, out mode_index);
            if (0 != status)
                throw new Win32Exception(-status);
            return mode_index;
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetCameraMode(IntPtr dev, int mode_index);
        public void SetCameraMode(int mode_index)
        {
            int status = FLISetCameraMode(dev, mode_index);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIHomeDevice(IntPtr dev);
        public void HomeDevice()
        {
            int status = FLIHomeDevice(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGrabFrame(IntPtr dev, IntPtr buff, int buffsize, out int bytesgrabbed);
        public void GrabFrame(byte[,] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();
            int bytesgrabbed;

            try
            {
                int status = FLIGrabFrame(dev, BuffPtr, buff.Length * sizeof(byte), out bytesgrabbed);
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
            if (buff.Length * sizeof(byte) != bytesgrabbed)
                throw new InvalidOperationException("bytesgrabbed != sizeof(buff)");
        }
        public void GrabFrame(ushort[,] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();
            int bytesgrabbed;

            try
            {
                int status = FLIGrabFrame(dev, BuffPtr, buff.Length * sizeof(ushort), out bytesgrabbed);
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
            if (buff.Length * sizeof(ushort) != bytesgrabbed)
                throw new InvalidOperationException("bytesgrabbed != sizeof(buff)");
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetTDI(IntPtr dev, int tdi_rate, int flags);
        public void SetTDI(int tdi_rate, int flags)
        {
            int status = FLISetTDI(dev, tdi_rate, flags);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGrabVideoFrame(IntPtr dev, IntPtr buff, int size);
        public void GrabVideoFrame(byte[,] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabVideoFrame(dev, BuffPtr, buff.Length * sizeof(byte));
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }
        public void GrabVideoFrame(IntPtr dev, ushort[,] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabVideoFrame(dev, BuffPtr, buff.Length * sizeof(ushort));
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }

        [DllImport("libfli.dll")]
        private static extern int FLIStopVideoMode(IntPtr dev);
        public void StopVideoMode()
        {
            int status = FLIStopVideoMode(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIStartVideoMode(IntPtr dev);
        public void StartVideoMode()
        {
            int status = FLIStartVideoMode(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetSerialString(IntPtr dev, StringBuilder serial, int len);
        public string GetSerialString()
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetSerialString(dev, sb, sb.MaxCapacity);
            if (0 != status)
                throw new Win32Exception(-status);
            return sb.ToString();
        }

        [DllImport("libfli.dll")]
        private static extern int FLIEndExposure(IntPtr dev);
        public void EndExposure()
        {
            int status = FLIEndExposure(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLITriggerExposure(IntPtr dev);
        public void TriggerExposure()
        {
            int status = FLITriggerExposure(dev);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetFanSpeed(IntPtr dev, FAN_SPEED fan_speed);
        public void SetFanSpeed(FAN_SPEED fan_speed)
        {
            int status = FLISetFanSpeed(dev, fan_speed);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLISetVerticalTableEntry(IntPtr dev, int index, int height, int bin, int mode);
        public void SetVerticalTableEntry(int index, int height, int bin, int mode)
        {
            int status = FLISetVerticalTableEntry(dev, index, height, bin, mode);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetVerticalTableEntry(IntPtr dev, int index, out int height, out int bin, out int mode);
        public void GetVerticalTableEntry(int index, out int height, out int bin, out int mode)
        {
            int status = FLIGetVerticalTableEntry(dev, index, out height, out bin, out mode);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIGetReadoutDimensions(IntPtr dev, out int width, out int hoffset, out int hbin, out int height, out int voffset, out int vbin);
        public void GetReadoutDimensions(out int width, out int hoffset, out int hbin, out int height, out int voffset, out int vbin)
        {
            int status = FLIGetReadoutDimensions(dev, out width, out hoffset, out hbin, out height, out voffset, out vbin);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIEnableVerticalTable(IntPtr dev, int width, int offset, int flags);
        public void EnableVerticalTable(int width, int offset, int flags)
        {
            int status = FLIEnableVerticalTable(dev, width, offset, flags);
            if (0 != status)
                throw new Win32Exception(-status);
        }

        [DllImport("libfli.dll")]
        private static extern int FLIReadUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr rbuf);
        public void ReadUserEEPROM(int loc, int address, byte[] rbuf)
        {
            GCHandle BuffGch = GCHandle.Alloc(rbuf, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIReadUserEEPROM(dev, loc, address, rbuf.Length, BuffPtr);
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }

        [DllImport("libfli.dll")]
        private static extern int FLIWriteUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr wbuf);
        public void WriteUserEEPROM(int loc, int address, byte[] wbuf)
        {
            GCHandle BuffGch = GCHandle.Alloc(wbuf, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIWriteUserEEPROM(dev, loc, address, wbuf.Length, BuffPtr);
                if (0 != status)
                    throw new Win32Exception(-status);
            }
            finally
            {
                BuffGch.Free();
            }
        }


        #endregion // APIs

    } // class
} // namespace
