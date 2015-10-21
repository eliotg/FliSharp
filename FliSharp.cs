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

        #endregion

    } // class
} // namespace
