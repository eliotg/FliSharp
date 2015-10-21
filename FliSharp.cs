using System;
using System.Runtime.InteropServices;

namespace FliSharp
{
    class FLI
    {
        //
        #region enums
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
        public enum DOMAIN : long
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
        public enum FRAME_TYPE : long
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
        public enum BIT_DEPTH : long
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
        public enum SHUTTER : long
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
        public enum BGFLUSH : long
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
        public enum CHANNEL : long
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
        public enum DEBUG : long
        {
            NONE = 0x00,
            INFO = 0x01,
            WARN = 0x02,
            FAIL = 0x04,
            IO = 0x08,
            ALL = INFO | WARN | FAIL
        }

        [Flags]
        public enum FAN_SPEED : long
        {
            OFF = 0x00,
            ON = 0xffffffff
        }

        /// <summary>
        /// Status settings
        /// </summary>
        [Flags]
        public enum STATUS : long
        {
            CAMERA_STATUS_UNKNOWN = 0xffffffff,
            CAMERA_STATUS_MASK = 0x00000003,
            CAMERA_STATUS_IDLE = 0x00,
            CAMERA_STATUS_WAITING_FOR_TRIGGER = 0x01,
            CAMERA_STATUS_EXPOSING = 0x02,
            CAMERA_STATUS_READING_CCD = 0x03,
            CAMERA_DATA_READY = 0x80000000,

            FOCUSER_STATUS_UNKNOWN = 0xffffffff,
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
            public long status;

            public FliException(long status)
            {
                this.status = status;
            }
        }

        #endregion

        //
        #region APIs
        //
        // private APIs refer to entry points exposed by libfli.dll, the _RC refers to the fact that it Returns a status Code
        // public APIs wrap with managed "calling convention" to throw exceptions vs. returning status codes
        //

        [DllImport("libfli.dll", EntryPoint="FLIOpen")] 
        private static extern long FLIOpen_RC(out IntPtr dev, string name, DOMAIN domain);
        public static void FLIOpen(out IntPtr dev, string name, DOMAIN domain)
        {
            long status = FLIOpen_RC(out dev, name, domain);
            if (0 != status)
                throw new FliException(status);
        }

        [DllImport("libfli.dll")] private static extern long FLISetDebugLevel(string host, DEBUG level);
        [DllImport("libfli.dll")] private static extern long FLIClose(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIGetLibVersion(string  ver, long len);
        [DllImport("libfli.dll")] private static extern long FLIGetModel(IntPtr dev, string  model, long len);
        [DllImport("libfli.dll")] private static extern long FLIGetPixelSize(IntPtr dev, out double pixel_x, out double pixel_y);
        [DllImport("libfli.dll")] private static extern long FLIGetHWRevision(IntPtr dev, out long hwrev);
        [DllImport("libfli.dll")] private static extern long FLIGetFWRevision(IntPtr dev, out long fwrev);
        [DllImport("libfli.dll")] private static extern long FLIGetArrayArea(IntPtr dev, out long ul_x, out long ul_y, out long lr_x, out long lr_y);
        [DllImport("libfli.dll")] private static extern long FLIGetVisibleArea(IntPtr dev, out long ul_x, out long ul_y, out long lr_x, out long lr_y);
        [DllImport("libfli.dll")] private static extern long FLISetExposureTime(IntPtr dev, long exptime);
        [DllImport("libfli.dll")] private static extern long FLISetImageArea(IntPtr dev, long ul_x, long ul_y, long lr_x, long lr_y);
        [DllImport("libfli.dll")] private static extern long FLISetHBin(IntPtr dev, long hbin);
        [DllImport("libfli.dll")] private static extern long FLISetVBin(IntPtr dev, long vbin);
        [DllImport("libfli.dll")] private static extern long FLISetFrameType(IntPtr dev, FRAME_TYPE frametype);
        [DllImport("libfli.dll")] private static extern long FLICancelExposure(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIGetExposureStatus(IntPtr dev, out long timeleft);
        [DllImport("libfli.dll")] private static extern long FLISetTemperature(IntPtr dev, double temperature);
        [DllImport("libfli.dll")] private static extern long FLIGetTemperature(IntPtr dev, out double temperature);
        [DllImport("libfli.dll")] private static extern long FLIGetCoolerPower(IntPtr dev, out double power);
/*        [DllImport("libfli.dll")] private static extern long FLIGrabRow(IntPtr dev, void *buff, size_t width);
        [DllImport("libfli.dll")] private static extern long FLIExposeFrame(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIFlushRow(IntPtr dev, long rows, long repeat);
        [DllImport("libfli.dll")] private static extern long FLISetNFlushes(IntPtr dev, long nflushes);
        [DllImport("libfli.dll")] private static extern long FLISetBitDepth(IntPtr dev, BIT_DEPTH bitdepth);
        [DllImport("libfli.dll")] private static extern long FLIReadIOPort(IntPtr dev, long *ioportset);
        [DllImport("libfli.dll")] private static extern long FLIWriteIOPort(IntPtr dev, long ioportset);
        [DllImport("libfli.dll")] private static extern long FLIConfigureIOPort(IntPtr dev, long ioportset);
        [DllImport("libfli.dll")] private static extern long FLILockDevice(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIUnlockDevice(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIControlShutter(IntPtr dev, SHUTTER shutter);
        [DllImport("libfli.dll")] private static extern long FLIControlBackgroundFlush(IntPtr dev, BGFLUSH bgflush);
        [DllImport("libfli.dll")] private static extern long FLISetDAC(IntPtr dev, unsigned long dacset);
        [DllImport("libfli.dll")] private static extern long FLIList(DOMAIN domain, string **names);
        [DllImport("libfli.dll")] private static extern long FLIFreeList(string *names);

        [DllImport("libfli.dll")] private static extern long FLIGetFilterName(IntPtr dev, long filter, string name, size_t len);
        [DllImport("libfli.dll")] private static extern long FLISetActiveWheel(IntPtr dev, long wheel);
        [DllImport("libfli.dll")] private static extern long FLIGetActiveWheel(IntPtr dev, long *wheel);

        [DllImport("libfli.dll")] private static extern long FLISetFilterPos(IntPtr dev, long filter);
        [DllImport("libfli.dll")] private static extern long FLIGetFilterPos(IntPtr dev, long *filter);
        [DllImport("libfli.dll")] private static extern long FLIGetFilterCount(IntPtr dev, long *filter);

        [DllImport("libfli.dll")] private static extern long FLIStepMotor(IntPtr dev, long steps);
        [DllImport("libfli.dll")] private static extern long FLIStepMotorAsync(IntPtr dev, long steps);
        [DllImport("libfli.dll")] private static extern long FLIGetStepperPosition(IntPtr dev, long *position);
        [DllImport("libfli.dll")] private static extern long FLIGetStepsRemaining(IntPtr dev, long *steps);
        [DllImport("libfli.dll")] private static extern long FLIHomeFocuser(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLICreateList(DOMAIN domain);
        [DllImport("libfli.dll")] private static extern long FLIDeleteList();
        [DllImport("libfli.dll")] private static extern long FLIListFirst(DOMAIN *domain, string filename,
		      size_t fnlen, string name, size_t namelen);
        [DllImport("libfli.dll")] private static extern long FLIListNext(DOMAIN *domain, string filename,
		      size_t fnlen, string name, size_t namelen);
        [DllImport("libfli.dll")] private static extern long FLIReadTemperature(IntPtr dev,
					CHANNEL channel, double *temperature);
        [DllImport("libfli.dll")] private static extern long FLIGetFocuserExtent(IntPtr dev, long *extent);
        [DllImport("libfli.dll")] private static extern long FLIUsbBulkIO(IntPtr dev, int ep, void *buf, long *len);
        [DllImport("libfli.dll")] private static extern long FLIGetDeviceStatus(IntPtr dev, long *status);
        [DllImport("libfli.dll")] private static extern long FLIGetCameraModeString(IntPtr dev, flimode_t mode_index, string mode_string, size_t siz);
        [DllImport("libfli.dll")] private static extern long FLIGetCameraMode(IntPtr dev, flimode_t *mode_index);
        [DllImport("libfli.dll")] private static extern long FLISetCameraMode(IntPtr dev, flimode_t mode_index);
        [DllImport("libfli.dll")] private static extern long FLIHomeDevice(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIGrabFrame(IntPtr dev, void* buff, size_t buffsize, size_t* bytesgrabbed);
        [DllImport("libfli.dll")] private static extern long FLISetTDI(IntPtr dev, flitdirate_t tdi_rate, flitdiflags_t flags);
        [DllImport("libfli.dll")] private static extern long FLIGrabVideoFrame(IntPtr dev, void *buff, size_t size);
        [DllImport("libfli.dll")] private static extern long FLIStopVideoMode(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIStartVideoMode(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLIGetSerialString(IntPtr dev, string  serial, size_t len);
        [DllImport("libfli.dll")] private static extern long FLIEndExposure(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLITriggerExposure(IntPtr dev);
        [DllImport("libfli.dll")] private static extern long FLISetFanSpeed(IntPtr dev, long fan_speed);
        [DllImport("libfli.dll")] private static extern long FLISetVerticalTableEntry(IntPtr dev, long index, long height, long bin, long mode);
        [DllImport("libfli.dll")] private static extern long FLIGetVerticalTableEntry(IntPtr dev, long index, long *height, long *bin, long *mode);
        [DllImport("libfli.dll")] private static extern long FLIGetReadoutDimensions(IntPtr dev, long *width, long *hoffset, long *hbin, long *height, long *voffset, long *vbin);
        [DllImport("libfli.dll")] private static extern long FLIEnableVerticalTable(IntPtr dev, long width, long offset, long flags);
        [DllImport("libfli.dll")] private static extern long FLIReadUserEEPROM(IntPtr dev, long loc, long address, long length, void *rbuf);
        [DllImport("libfli.dll")] private static extern long FLIWriteUserEEPROM(IntPtr dev, long loc, long address, long length, void *wbuf);
*/
        #endregion // APIs

    } // class
} // namespace
