using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncOrigoSpike
{

    [Serializable]
    public class CommandBatch
    {
        public readonly Command[] Commands;

        public CommandBatch(IEnumerable<Command> commands)
        {
            Commands = commands.ToArray();
        }
    }
}