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
        ///    combination of interface method and device type.  Valid interfaces
        ///    are \texttt{FLIDOMAIN_PARALLEL_PORT}, \texttt{FLIDOMAIN_USB},
        ///    \texttt{FLIDOMAIN_SERIAL}, and \texttt{FLIDOMAIN_INET}.  Valid
        ///    device types are \texttt{FLIDEVICE_CAMERA},
        ///    \texttt{FLIDOMAIN_FILTERWHEEL}, and \texttt{FLIDOMAIN_FOCUSER}.
        ///    
        ///    @see FLIOpen
        ///    @see FLIList
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
        ///    \texttt{FLI_FRAME_TYPE_NORMAL} and \texttt{FLI_FRAME_TYPE_DARK}.
        ///
        ///    @see FLISetFrameType
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
        ///    depths are \texttt{FLI_MODE_8BIT} and \texttt{FLI_MODE_16BIT}.
        ///
        ///    @see FLISetBitDepth
        /// </summary>
        public enum BIT_DEPTH : long
        {
            _8BIT = 0,
            _16BIT = 1
        }

        /// <summary>
        /// Type used for shutter operations for an FLI camera device.  Valid
        ///    shutter types are \texttt{FLI_SHUTTER_CLOSE},
        ///    \texttt{FLI_SHUTTER_OPEN},
        ///    \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER},
        ///    \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER_LOW}, and
        ///    \texttt{FLI_SHUTTER_EXTERNAL_TRIGGER_HIGH}.
        ///
        ///    @see FLIControlShutter
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
        ///    bgflush types are \texttt{FLI_BGFLUSH_STOP} and
        ///    \texttt{FLI_BGFLUSH_START}.
        ///
        ///    @see FLIControlBackgroundFlush
        /// </summary>
        public enum BGFLUSH : long
        {
            STOP = 0x0000,
            START = 0x0001
        }

        /// <summary>
        /// Type used to determine which temperature channel to read.  Valid
        ///    channel types are \texttt{FLI_TEMPERATURE_INTERNAL} and
        ///    \texttt{FLI_TEMPERATURE_EXTERNAL}.
        ///
        ///    @see FLIReadTemperature
        /// </summary>
        public enum CHANNEL : long
        {
            INTERNAL = 0x0000,
            EXTERNAL = 0x0001,
            CCD = 0x0000,
            BASE = 0x0001
        }

        #endregion

    } // class
} // namespace
