﻿using System;
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
                cam.GetArrayArea(out ul_x, out ul_y, out lr_x, out lr_y);
                ushort[,] data = new ushort[3072, 3072];
                cam.SetImageArea(0, 0, 3072, 3072);
                cam.SetFrameType(FLI.FRAME_TYPE.NORMAL);
                cam.SetExposureTime(500);

                Console.WriteLine("ExposeFrame()...");
                cam.ExposeFrame();
                status = cam.GetDeviceStatus();
                while (!FLI.IsGrabFrameReady(cam.GetDeviceStatus()))
                    System.Threading.Thread.Sleep(100);
                
                Console.WriteLine("EndExposure()...");
                cam.EndExposure();
                status = cam.GetDeviceStatus();
                cam.GrabFrame(data);
            }
        }

    } // class
} // namespace
