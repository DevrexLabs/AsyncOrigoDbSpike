using System.Threading.Tasks.Dataflow;

namespace AckAck
{


    internal class QueryContext
    {
        public readonly Query Query;
        public readonly WriteOnceBlock<object> Response;

        public QueryContext(Query query, WriteOnceBlock<object> responseBlock)
        {
            Response = responseBlock;
            Query = query;
        }
    }
}
