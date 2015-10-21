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

            string[] names;
            FLI.List(FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB, out names);
        }

    } // class
} // namespace
