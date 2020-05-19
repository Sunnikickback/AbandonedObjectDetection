using System.Collections.Generic;
namespace ClassLibrary
{
    public class PersonAndObject
    {
        private Item person;
        private Item item;
        private int personIsAway = 0;
        private IoU IoU = new IoU();
        public PersonAndObject(Item person, Item item)
        {
            this.person = person;
            this.item = item;
        }

        public bool UpdateCords(ref List<Item> people, ref List<Item> objects)
        {
            this.person.NewCords(ref people);
            this.item.NewCords(ref objects);
            if (item.IsAway)
            {
                return false;
            }
            if (IoU.Intersection(person.Rect, item.Rect) == 0 || person.IsAway )
            {
                personIsAway++;
            }
            return true;
        }

        public Item Person { get => person; set => person = value; }
        public Item Item { get => item; set => item = value; }
        public int PersonIsAway { get => personIsAway; set => personIsAway = value; }
    }
}
