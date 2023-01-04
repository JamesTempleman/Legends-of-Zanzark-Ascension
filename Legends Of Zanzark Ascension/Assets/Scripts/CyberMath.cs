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


        public static double GetRandomDouble(double min, double max)
        {
            lock (syncLock)
            {
                return (rng.NextDouble() * max) + min;
            }
        }

        public static int GetRandomInt(int min, int max)
        {
            lock (syncLock)
            {
                return rng.Next(min, max);
            }
        }

        public static float GetDegreeInRadians(float degree)
        {
            return degree * (float)Math.PI / 180;
        }
    }
}
