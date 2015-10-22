using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FliSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(FLI.GetLibVersion());

            using (FLI cam = new FLI("flipro0", FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB))
            {
                int status = cam.GetDeviceStatus();
                int mode_index = cam.GetCameraMode();
                double power = cam.GetCoolerPower();
                int fwrev = cam.GetFWRevision();
                int hwrev = cam.GetHWRevision();
                string model = cam.GetModel();
                double pixel_x;
                double pixel_y;
                cam.GetPixelSize(out pixel_x, out pixel_y);
                string serial = cam.GetSerialString();
                double ccdtemp = cam.GetTemperature();
                Console.WriteLine("CCD temp: " + ccdtemp);

                cam.SetFanSpeed(FLI.FAN_SPEED.ON);

                string[] names;
                // FLI.List(FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB, out names);

            }
        }

    } // class
} // namespace
