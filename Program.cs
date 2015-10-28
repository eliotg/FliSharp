using System;
using System.Threading;

namespace FliSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            FLI.SetDebugLevel("FLIdebug.log", FLI.DEBUG.ALL);

            Console.WriteLine(FLI.GetLibVersion());

            using (FLI cam = new FLI("flipro0", FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB))
            {
                FLI.STATUS status = cam.GetDeviceStatus();
                int mode_index = cam.GetCameraMode();
                //string mode = cam.GetCameraModeString(mode_index);
                double power = cam.GetCoolerPower();
                int fwrev = cam.GetFWRevision();
                int hwrev = cam.GetHWRevision();
                string model = cam.GetModel();
                double pixel_x;
                double pixel_y;
                cam.GetPixelSize(out pixel_x, out pixel_y);
                string serial = cam.GetSerialString();
                double ccdtemp = cam.GetTemperature();
                double ccdtemp2 = cam.ReadTemperature(FLI.CHANNEL.CCD);
                double exttemp = cam.ReadTemperature(FLI.CHANNEL.EXTERNAL);
                Console.WriteLine("CCD temp: " + ccdtemp);

                cam.SetFanSpeed(FLI.FAN_SPEED.OFF);
                cam.SetTemperature(FLI.COOLER_MAX_TEMP);

                string[] names;
                // FLI.List(FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB, out names);

                int ul_x, ul_y, lr_x, lr_y;
                cam.GetVisibleArea(out ul_x, out ul_y, out lr_x, out lr_y);
                int width = lr_x - ul_x;
                int height = lr_y - ul_y;
                ushort[][] data = new ushort[height][];
                cam.SetImageArea(ul_x, ul_y, lr_x, lr_y);
                cam.SetExposureTime(500);
                cam.SetFrameType(FLI.FRAME_TYPE.NORMAL);
                cam.SetHBin(1);
                cam.SetVBin(1);
                cam.SetTDI(0, 0);

                Console.WriteLine("ExposeFrame()...");
                cam.ExposeFrame();
                status = cam.GetDeviceStatus();
                while (!cam.IsDownloadReady())
                    Thread.Sleep(100);
                
                Console.WriteLine("Downloading...");
                for (int y = 0; y < height; y++)
                {
                    data[y] = new ushort[width];
                    cam.GrabRow(data[y]);
                }
            }
        }

    } // class
} // namespace
