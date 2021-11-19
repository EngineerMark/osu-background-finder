using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_background_finder
{
    //quick vector struct
    public struct Vector
    {
        public double x;
        public double y;

        public Vector(double x, double y){
            this.x = x; this.y = y;
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.x * b.x, a.y * b.y);
        }
    }
}
