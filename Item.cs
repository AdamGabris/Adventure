using System.Reflection.Metadata.Ecma335;
using Microsoft.VisualBasic;

namespace Adventure.BuildingBlocks
{
    public class Item
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        public Dictionary<string, string[]> actions { get; set; }

        public Item()
        {
            actions = new Dictionary<string, string[]>();
            Name = string.Empty;
        }



    }



}