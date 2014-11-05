using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncOrigoSpike
{

    /// <summary>
    /// For baseline comparison of internal performance
    /// </summary>
    public class NullJournalWriter : IJournalWriter
    {
        public Task AppendAsync(IEnumerable<Command> commands)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            
        }
    }
}