using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberMath
{
    public class CyberMath
    {
        private static Random rng = new Random();
        private static readonly object syncLock = new object();

        //returns a random double between the given paramaters
        public static double GetRandomDouble(double min, double max)
        {
            lock (syncLock)
            {
                return (rng.NextDouble() * max) + min;
            }
        }


        //returns a random int between the given paramaters
        public static int GetRandomInt(int min, int max)
        {
            lock (syncLock)
            {
                return rng.Next(min, max);
            }
        }

        //returns a given degree in radians
        public static float GetDegreeInRadians(float degree)
        {
            return degree * (float)Math.PI / 180;
        }
    }
}
