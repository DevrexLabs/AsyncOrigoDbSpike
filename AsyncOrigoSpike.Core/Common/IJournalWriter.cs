using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncOrigoSpike
{
    public interface IJournalWriter : IDisposable
    {
        Task AppendAsync(IEnumerable<Command> commands);
    }
}