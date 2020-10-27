using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluscream
{
    static class Extensions
    {
        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static OpenRGB.NET.Models.Color Dim(this OpenRGB.NET.Models.Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            return new OpenRGB.NET.Models.Color((byte)red, (byte)green, (byte)blue);
        }
        public static OpenRGB.NET.Models.Device GetDeviceByName(this OpenRGB.NET.OpenRGBClient client, string name)
        {
            return client.GetAllControllerData().ToList().Where(d => d.Name == name).FirstOrDefault();
        }
        public static byte ToByte(int intValue) => BitConverter.GetBytes(intValue).First();
    }
}
