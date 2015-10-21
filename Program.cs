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
            string ver;
            FLI.GetLibVersion(out ver);
            Console.WriteLine(ver);

            IntPtr dev;
            FLI.Open(out dev, "flipro0", FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB);
            int status;
            FLI.GetDeviceStatus(dev, out status);
            int mode_index;
            FLI.GetCameraMode(dev, out mode_index);
            double power;
            FLI.GetCoolerPower(dev, out power);
            int fwrev;
            FLI.GetFWRevision(dev, out fwrev);
            int hwrev;
            FLI.GetHWRevision(dev, out hwrev);
            string model;
            FLI.GetModel(dev, out model);
            double pixel_x;
            double pixel_y;
            FLI.GetPixelSize(dev, out pixel_x, out pixel_y);
            string serial;
            FLI.GetSerialString(dev, out serial);
            double ccdtemp;
            FLI.GetTemperature(dev, out ccdtemp);

            string[] names;
            // FLI.List(FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB, out names);

            FLI.Close(dev);
        }

    } // class
} // namespace
