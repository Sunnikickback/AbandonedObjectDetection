using FastYolo.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    class IoU
    {
        public double IOU(Rectangle rect1, Rectangle rect2)
        {
            double union = Union(rect1, rect2);
            double intersection = Intersection(rect1, rect2);
            return intersection / union;
        }
        

        public int Intersection(Rectangle rect1, Rectangle rect2)
        {

            int width = Math.Min(rect1.X + rect1.Width, rect2.X + rect2.Width) - Math.Max(rect1.X, rect2.X);
            int height = Math.Min(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - Math.Max(rect1.Y, rect2.Y);
            if(width != 0 && height != 0)
            {
                return width * height;
            }
            return 0;
        }

        private int RectArea(Rectangle rect)
        {
            return rect.Width * rect.Height;
        }

        private int Union(Rectangle rect1, Rectangle rect2)
        {
            return RectArea(rect1) + RectArea(rect2) - Intersection(rect1, rect2);
        }
    }
}
