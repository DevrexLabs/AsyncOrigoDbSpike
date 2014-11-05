using System;
using System.Collections.Generic;

namespace AsyncOrigoSpike.Test
{
    [Serializable]
    public class AddItemCommand : Command<List<string>, int>
    {
        public readonly string Item;

        public AddItemCommand(string item)
        {
            Item = item;
        }

        public override int Execute(List<string> model)
        {
            model.Add(Item);
            return model.Count;
        }
    }
}