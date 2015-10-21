using System;
using System.Runtime.InteropServices;
using System.Text; // for StringBuilder

namespace FliSharp
{
    class FLI
    {
        //
        #region Constants
        //

        public const int MAX_STRING_LEN = 256;

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
            _8BIT = 0,
            _16BIT = 1
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
        #region Types
        //

        /// <summary>
        /// Exception thrown when an API call returns non-zero, see status for details
        /// </summary>
        public class FliException : Exception
        {
            public int status;

            public FliException(int status)
            {
                this.status = status;
            }
        }

        #endregion

        //
        #region APIs
        //
        // private APIs refer to entry points exposed by libfli.dll, the  refers to the fact that it Returns a status Code
        // public APIs wrap with managed "calling convention" to throw exceptions vs. returning status codes
        //

        [DllImport("libfli.dll", EntryPoint = "FLIOpen")]
        private static extern int FLIOpen(out IntPtr dev, string name, DOMAIN domain);
        public static void Open(out IntPtr dev, string name, DOMAIN domain)
        {
            int status = FLIOpen(out dev, name, domain);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetDebugLevel")]
        private static extern int FLISetDebugLevel(string host, DEBUG level);
        public static void SetDebugLevel(string host, DEBUG level)
        {
            int status = FLISetDebugLevel(host, level);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIClose")]
        private static extern int FLIClose(IntPtr dev);
        public static void Close(IntPtr dev)
        {
            int status = FLIClose(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetLibVersion")]
        private static extern int FLIGetLibVersion(StringBuilder ver, int len);
        public static void GetLibVersion(out string ver)
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetLibVersion(sb, sb.MaxCapacity);
            if (0 != status)
                throw new FliException(status);
            ver = sb.ToString();
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetModel")]
        private static extern int FLIGetModel(IntPtr dev, StringBuilder model, int len);
        public static void GetModel(IntPtr dev, out string model)
        {
            StringBuilder sb = new StringBuilder(MAX_STRING_LEN);
            int status = FLIGetModel(dev, sb, sb.MaxCapacity);
            if (0 != status)
                throw new FliException(status);
            model = sb.ToString();
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetPixelSize")]
        private static extern int FLIGetPixelSize(IntPtr dev, out double pixel_x, out double pixel_y);
        public static void GetPixelSize(IntPtr dev, out double pixel_x, out double pixel_y)
        {
            int status = FLIGetPixelSize(dev, out pixel_x, out pixel_y);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetHWRevision")]
        private static extern int FLIGetHWRevision(IntPtr dev, out int hwrev);
        public static void GetHWRevision(IntPtr dev, out int hwrev)
        {
            int status = FLIGetHWRevision(dev, out hwrev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetFWRevision")]
        private static extern int FLIGetFWRevision(IntPtr dev, out int fwrev);
        public static void GetFWRevision(IntPtr dev, out int fwrev)
        {
            int status = FLIGetFWRevision(dev, out fwrev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetArrayArea")]
        private static extern int FLIGetArrayArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y);
        public static void GetArrayArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            int status = FLIGetArrayArea(dev, out ul_x, out ul_y, out lr_x, out lr_y);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetVisibleArea")]
        private static extern int FLIGetVisibleArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y);
        public static void GetVisibleArea(IntPtr dev, out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            int status = FLIGetVisibleArea(dev, out ul_x, out ul_y, out lr_x, out lr_y);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetExposureTime")]
        private static extern int FLISetExposureTime(IntPtr dev, int exptime);
        public static void SetExposureTime(IntPtr dev, int exptime)
        {
            int status = FLISetExposureTime(dev, exptime);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetImageArea")]
        private static extern int FLISetImageArea(IntPtr dev, int ul_x, int ul_y, int lr_x, int lr_y);
        public static void SetImageArea(IntPtr dev, int ul_x, int ul_y, int lr_x, int lr_y)
        {
            int status = FLISetImageArea(dev, ul_x, ul_y, lr_x, lr_y);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetHBin")]
        private static extern int FLISetHBin(IntPtr dev, int hbin);
        public static void SetHBin(IntPtr dev, int hbin)
        {
            int status = FLISetHBin(dev, hbin);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetVBin")]
        private static extern int FLISetVBin(IntPtr dev, int vbin);
        public static void SetVBin(IntPtr dev, int vbin)
        {
            int status = FLISetVBin(dev, vbin);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetFrameType")]
        private static extern int FLISetFrameType(IntPtr dev, FRAME_TYPE frametype);
        public static void SetFrameType(IntPtr dev, FRAME_TYPE frametype)
        {
            int status = FLISetFrameType(dev, frametype);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLICancelExposure")]
        private static extern int FLICancelExposure(IntPtr dev);
        public static void CancelExposure(IntPtr dev)
        {
            int status = FLICancelExposure(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetExposureStatus")]
        private static extern int FLIGetExposureStatus(IntPtr dev, out int timeleft);
        public static void GetExposureStatus(IntPtr dev, out int timeleft)
        {
            int status = FLIGetExposureStatus(dev, out timeleft);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetTemperature")]
        private static extern int FLISetTemperature(IntPtr dev, double temperature);
        public static void SetTemperature(IntPtr dev, double temperature)
        {
            int status = FLISetTemperature(dev, temperature);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetTemperature")]
        private static extern int FLIGetTemperature(IntPtr dev, out double temperature);
        public static void GetTemperature(IntPtr dev, out double temperature)
        {
            int status = FLIGetTemperature(dev, out temperature);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetCoolerPower")]
        private static extern int FLIGetCoolerPower(IntPtr dev, out double power);
        public static void GetCoolerPower(IntPtr dev, out double power)
        {
            int status = FLIGetCoolerPower(dev, out power);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGrabRow")]
        private static extern int FLIGrabRow(IntPtr dev, IntPtr buff, int width);
        public static void GrabRow(IntPtr dev, byte[] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabRow(dev, BuffPtr, buff.Length * sizeof(byte));
                if (0 != status)
                    throw new FliException(status);
            }
            finally
            {
                BuffGch.Free();
            }
        }
        public static void GrabRow(IntPtr dev, ushort[] buff)
        {
            GCHandle BuffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            IntPtr BuffPtr = BuffGch.AddrOfPinnedObject();

            try
            {
                int status = FLIGrabRow(dev, BuffPtr, buff.Length * sizeof(ushort));
                if (0 != status)
                    throw new FliException(status);
            }
            finally
            {
                BuffGch.Free();
            }
        }

        [DllImport("libfli.dll", EntryPoint = "FLIExposeFrame")]
        private static extern int FLIExposeFrame(IntPtr dev);
        public static void ExposeFrame(IntPtr dev)
        {
            int status = FLIExposeFrame(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIFlushRow")]
        private static extern int FLIFlushRow(IntPtr dev, int rows, int repeat);
        public static void FlushRow(IntPtr dev, int rows, int repeat)
        {
            int status = FLIFlushRow(dev, rows, repeat);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetNFlushes")]
        private static extern int FLISetNFlushes(IntPtr dev, int nflushes);
        public static void SetNFlushes(IntPtr dev, int nflushes)
        {
            int status = FLISetNFlushes(dev, nflushes);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetBitDepth")]
        private static extern int FLISetBitDepth(IntPtr dev, BIT_DEPTH bitdepth);
        public static void SetBitDepth(IntPtr dev, BIT_DEPTH bitdepth)
        {
            int status = FLISetBitDepth(dev, bitdepth);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIReadIOPort")]
        private static extern int FLIReadIOPort(IntPtr dev, out int ioportset);
        public static void ReadIOPort(IntPtr dev, out int ioportset)
        {
            int status = FLIReadIOPort(dev, out ioportset);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIWriteIOPort")]
        private static extern int FLIWriteIOPort(IntPtr dev, int ioportset);
        public static void WriteIOPort(IntPtr dev, int ioportset)
        {
            int status = FLIWriteIOPort(dev, ioportset);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIConfigureIOPort")]
        private static extern int FLIConfigureIOPort(IntPtr dev, int ioportset);
        public static void ConfigureIOPort(IntPtr dev, int ioportset)
        {
            int status = FLIConfigureIOPort(dev, ioportset);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLILockDevice")]
        private static extern int FLILockDevice(IntPtr dev);
        public static void LockDevice(IntPtr dev)
        {
            int status = FLILockDevice(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIUnlockDevice")]
        private static extern int FLIUnlockDevice(IntPtr dev);
        public static void UnlockDevice(IntPtr dev)
        {
            int status = FLIUnlockDevice(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIControlShutter")]
        private static extern int FLIControlShutter(IntPtr dev, SHUTTER shutter);
        public static void ControlShutter(IntPtr dev, SHUTTER shutter)
        {
            int status = FLIControlShutter(dev, shutter);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIControlBackgroundFlush")]
        private static extern int FLIControlBackgroundFlush(IntPtr dev, BGFLUSH bgflush);
        public static void ControlBackgroundFlush(IntPtr dev, BGFLUSH bgflush)
        {
            int status = FLIControlBackgroundFlush(dev, bgflush);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetDAC")]
        private static extern int FLISetDAC(IntPtr dev, uint dacset);
        public static void SetDAC(IntPtr dev, uint dacset)
        {
            int status = FLISetDAC(dev, dacset);
            if (0 != status)
                throw new FliException(status);
        }
/*
        [DllImport("libfli.dll", EntryPoint = "FLIList")]
        private static extern int FLIList(DOMAIN domain, string** names);
        public static void List(DOMAIN domain, string** names)
        {
            int status = FLIList(domain, names);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIFreeList")]
        private static extern int FLIFreeList(string* names);
        public static void FreeList(string* names)
        {
            int status = FLIFreeList(names);
            if (0 != status)
                throw new FliException(status);
        }
*/

        [DllImport("libfli.dll", EntryPoint = "FLIGetFilterName")]
        private static extern int FLIGetFilterName(IntPtr dev, int filter, string name, int len);
        public static void GetFilterName(IntPtr dev, int filter, string name, int len)
        {
            int status = FLIGetFilterName(dev, filter, name, len);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetActiveWheel")]
        private static extern int FLISetActiveWheel(IntPtr dev, int wheel);
        public static void SetActiveWheel(IntPtr dev, int wheel)
        {
            int status = FLISetActiveWheel(dev, wheel);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetActiveWheel")]
        private static extern int FLIGetActiveWheel(IntPtr dev, out int wheel);
        public static void GetActiveWheel(IntPtr dev, out int wheel)
        {
            int status = FLIGetActiveWheel(dev, out wheel);
            if (0 != status)
                throw new FliException(status);
        }


        [DllImport("libfli.dll", EntryPoint = "FLISetFilterPos")]
        private static extern int FLISetFilterPos(IntPtr dev, int filter);
        public static void SetFilterPos(IntPtr dev, int filter)
        {
            int status = FLISetFilterPos(dev, filter);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetFilterPos")]
        private static extern int FLIGetFilterPos(IntPtr dev, out int filter);
        public static void GetFilterPos(IntPtr dev, out int filter)
        {
            int status = FLIGetFilterPos(dev, out filter);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetFilterCount")]
        private static extern int FLIGetFilterCount(IntPtr dev, out int filter);
        public static void GetFilterCount(IntPtr dev, out int filter)
        {
            int status = FLIGetFilterCount(dev, out filter);
            if (0 != status)
                throw new FliException(status);
        }


        [DllImport("libfli.dll", EntryPoint = "FLIStepMotor")]
        private static extern int FLIStepMotor(IntPtr dev, int steps);
        public static void StepMotor(IntPtr dev, int steps)
        {
            int status = FLIStepMotor(dev, steps);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIStepMotorAsync")]
        private static extern int FLIStepMotorAsync(IntPtr dev, int steps);
        public static void StepMotorAsync(IntPtr dev, int steps)
        {
            int status = FLIStepMotorAsync(dev, steps);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetStepperPosition")]
        private static extern int FLIGetStepperPosition(IntPtr dev, out int position);
        public static void GetStepperPosition(IntPtr dev, out int position)
        {
            int status = FLIGetStepperPosition(dev, out position);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetStepsRemaining")]
        private static extern int FLIGetStepsRemaining(IntPtr dev, out int steps);
        public static void GetStepsRemaining(IntPtr dev, out int steps)
        {
            int status = FLIGetStepsRemaining(dev, out steps);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIHomeFocuser")]
        private static extern int FLIHomeFocuser(IntPtr dev);
        public static void HomeFocuser(IntPtr dev)
        {
            int status = FLIHomeFocuser(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLICreateList")]
        private static extern int FLICreateList(DOMAIN domain);
        public static void CreateList(DOMAIN domain)
        {
            int status = FLICreateList(domain);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIDeleteList")]
        private static extern int FLIDeleteList();
        public static void DeleteList()
        {
            int status = FLIDeleteList();
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIListFirst")]
        private static extern int FLIListFirst(out DOMAIN domain, string filename, int fnlen, string name, int namelen);
        public static void ListFirst(out DOMAIN domain, string filename, int fnlen, string name, int namelen)
        {
            int status = FLIListFirst(out domain, filename, fnlen, name, namelen);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIListNext")]
        private static extern int FLIListNext(out DOMAIN domain, string filename, int fnlen, string name, int namelen);
        public static void ListNext(out DOMAIN domain, string filename, int fnlen, string name, int namelen)
        {
            int status = FLIListNext(out domain, filename, fnlen, name, namelen);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIReadTemperature")]
        private static extern int FLIReadTemperature(IntPtr dev, CHANNEL channel, out double temperature);
        public static void ReadTemperature(IntPtr dev, CHANNEL channel, out double temperature)
        {
            int status = FLIReadTemperature(dev, channel, out temperature);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetFocuserExtent")]
        private static extern int FLIGetFocuserExtent(IntPtr dev, out int extent);
        public static void GetFocuserExtent(IntPtr dev, out int extent)
        {
            int status = FLIGetFocuserExtent(dev, out extent);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIUsbBulkIO")]
        private static extern int FLIUsbBulkIO(IntPtr dev, int ep, IntPtr buf, out int len);
        public static void UsbBulkIO(IntPtr dev, int ep, IntPtr buf, out int len)
        {
            int status = FLIUsbBulkIO(dev, ep, buf, out len);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetDeviceStatus")]
        private static extern int FLIGetDeviceStatus(IntPtr dev, out int status);
        public static void GetDeviceStatus(IntPtr dev, out int status)
        {
            int Status = FLIGetDeviceStatus(dev, out status);
            if (0 != Status)
                throw new FliException(Status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetCameraModeString")]
        private static extern int FLIGetCameraModeString(IntPtr dev, int mode_index, string mode_string, int siz);
        public static void GetCameraModeString(IntPtr dev, int mode_index, string mode_string, int siz)
        {
            int status = FLIGetCameraModeString(dev, mode_index, mode_string, siz);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetCameraMode")]
        private static extern int FLIGetCameraMode(IntPtr dev, out int mode_index);
        public static void GetCameraMode(IntPtr dev, out int mode_index)
        {
            int status = FLIGetCameraMode(dev, out mode_index);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetCameraMode")]
        private static extern int FLISetCameraMode(IntPtr dev, int mode_index);
        public static void SetCameraMode(IntPtr dev, int mode_index)
        {
            int status = FLISetCameraMode(dev, mode_index);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIHomeDevice")]
        private static extern int FLIHomeDevice(IntPtr dev);
        public static void HomeDevice(IntPtr dev)
        {
            int status = FLIHomeDevice(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGrabFrame")]
        private static extern int FLIGrabFrame(IntPtr dev, IntPtr buff, int buffsize, out int bytesgrabbed);
        public static void GrabFrame(IntPtr dev, IntPtr buff, int buffsize, out int bytesgrabbed)
        {
            int status = FLIGrabFrame(dev, buff, buffsize, out bytesgrabbed);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetTDI")]
        private static extern int FLISetTDI(IntPtr dev, int tdi_rate, int flags);
        public static void SetTDI(IntPtr dev, int tdi_rate, int flags)
        {
            int status = FLISetTDI(dev, tdi_rate, flags);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGrabVideoFrame")]
        private static extern int FLIGrabVideoFrame(IntPtr dev, IntPtr buff, int size);
        public static void GrabVideoFrame(IntPtr dev, IntPtr buff, int size)
        {
            int status = FLIGrabVideoFrame(dev, buff, size);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIStopVideoMode")]
        private static extern int FLIStopVideoMode(IntPtr dev);
        public static void StopVideoMode(IntPtr dev)
        {
            int status = FLIStopVideoMode(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIStartVideoMode")]
        private static extern int FLIStartVideoMode(IntPtr dev);
        public static void StartVideoMode(IntPtr dev)
        {
            int status = FLIStartVideoMode(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetSerialString")]
        private static extern int FLIGetSerialString(IntPtr dev, string serial, int len);
        public static void GetSerialString(IntPtr dev, string serial, int len)
        {
            int status = FLIGetSerialString(dev, serial, len);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIEndExposure")]
        private static extern int FLIEndExposure(IntPtr dev);
        public static void EndExposure(IntPtr dev)
        {
            int status = FLIEndExposure(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLITriggerExposure")]
        private static extern int FLITriggerExposure(IntPtr dev);
        public static void TriggerExposure(IntPtr dev)
        {
            int status = FLITriggerExposure(dev);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetFanSpeed")]
        private static extern int FLISetFanSpeed(IntPtr dev, int fan_speed);
        public static void SetFanSpeed(IntPtr dev, int fan_speed)
        {
            int status = FLISetFanSpeed(dev, fan_speed);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLISetVerticalTableEntry")]
        private static extern int FLISetVerticalTableEntry(IntPtr dev, int index, int height, int bin, int mode);
        public static void SetVerticalTableEntry(IntPtr dev, int index, int height, int bin, int mode)
        {
            int status = FLISetVerticalTableEntry(dev, index, height, bin, mode);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetVerticalTableEntry")]
        private static extern int FLIGetVerticalTableEntry(IntPtr dev, int index, out int height, out int bin, out int mode);
        public static void GetVerticalTableEntry(IntPtr dev, int index, out int height, out int bin, out int mode)
        {
            int status = FLIGetVerticalTableEntry(dev, index, out height, out bin, out mode);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIGetReadoutDimensions")]
        private static extern int FLIGetReadoutDimensions(IntPtr dev, out int width, out int hoffset, out int hbin, out int height, out int voffset, out int vbin);
        public static void GetReadoutDimensions(IntPtr dev, out int width, out int hoffset, out int hbin, out int height, out int voffset, out int vbin)
        {
            int status = FLIGetReadoutDimensions(dev, out width, out hoffset, out hbin, out height, out voffset, out vbin);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIEnableVerticalTable")]
        private static extern int FLIEnableVerticalTable(IntPtr dev, int width, int offset, int flags);
        public static void EnableVerticalTable(IntPtr dev, int width, int offset, int flags)
        {
            int status = FLIEnableVerticalTable(dev, width, offset, flags);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIReadUserEEPROM")]
        private static extern int FLIReadUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr rbuf);
        public static void ReadUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr rbuf)
        {
            int status = FLIReadUserEEPROM(dev, loc, address, length, rbuf);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll", EntryPoint = "FLIWriteUserEEPROM")]
        private static extern int FLIWriteUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr wbuf);
        public static void WriteUserEEPROM(IntPtr dev, int loc, int address, int length, IntPtr wbuf)
        {
            int status = FLIWriteUserEEPROM(dev, loc, address, length, wbuf);
            if (0 != status)
                throw new FliException(status);
        }


        #endregion // APIs

    } // class
} // namespace
