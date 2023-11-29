using System.Collections;
using Utils;
using static Utils.Constants;
using Adventure.BuildingBlocks;
using static Utils.Output;
using static Adventure.AssetsAndSettings;




namespace Adventure
{

    public class Inventory
    {
        private List<Item> items;

        public Inventory()
        {
            items = new List<Item>();
        }

        public void Add(Item item)
        {
            items.Add(item);
        }

        public void Remove(Item item)
        {
            items.Remove(item);
        }

        public bool HasItem(Item item)
        {
            return items.Contains(item);
        }

        public override string ToString()
        {
            return string.Join(", ", items.Select(item => item.Name));
        }
    }



}