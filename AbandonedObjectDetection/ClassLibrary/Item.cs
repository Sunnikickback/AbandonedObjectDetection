using FastYolo.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Item
    {
        Point pointUL;
        Point pointDR;
        Rectangle rect;
        int CountIsAway = 0;
        bool isAway = false;
        public bool IsAway { get => isAway; set => isAway = value; }
        public Rectangle Rect { get => rect; set => rect = value; }
        public Point PointUL { get => pointUL; set => pointUL = value; }
       
        public Item(YoloItem item, string types)
        {
            if (types.Contains(item.Type))
            {
                PointUL = new Point(item.X, item.Y);
                pointDR = new Point(item.X + item.Width, item.Y + item.Height);
                Rect = new Rectangle(item.X, item.Y, item.Width, item.Height);
            }
            else
                throw new ItemException(message: "Item is not a suitable type");
        }

        public PersonAndObject NewPair(ref List<Item> people) {
            IoU iou = new IoU();
            KeyValuePair<int, int> maxIntersection = new KeyValuePair<int, int>(-1, 0);
            for (int i = 0; i < people.Count; i++)
            {
                var person = people[0];
                if (iou.Intersection(this.Rect, person.Rect) > maxIntersection.Value)
                    maxIntersection = new KeyValuePair<int, int>(i, iou.Intersection(this.Rect, person.Rect));
            }
            if (maxIntersection.Key != -1)
            {
                var person = people[maxIntersection.Key];
                people.RemoveAt(maxIntersection.Key);
                PersonAndObject track = new PersonAndObject(person, this);
                //Logging
               
                return track;
            }
            else
            {
                return null;
            }
        }

        public bool NewCords(ref List<Item> items)
        {
            if (IsAway)
                return IsAway;
            IoU iou = new IoU();
            KeyValuePair<int, int> maxIntersection = new KeyValuePair<int, int>(-1, 0);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (iou.Intersection(this.Rect, item.Rect) > maxIntersection.Value)
                 maxIntersection = new KeyValuePair<int, int>(i, iou.Intersection(Rect, item.Rect));
            }
            if (maxIntersection.Key != -1)
            {
                var item = items[maxIntersection.Key];
                this.Rect = item.Rect;
                this.pointDR = item.pointDR;
                this.PointUL = item.PointUL;
                items.RemoveAt(maxIntersection.Key);
                CountIsAway = 0;
            }
            else
            { 
                CountIsAway++;
                if(CountIsAway > 10)
                {
                    isAway = true;
                }
            }
            return IsAway;
        }
    }
}
