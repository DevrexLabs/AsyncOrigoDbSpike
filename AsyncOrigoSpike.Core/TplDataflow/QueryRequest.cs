using System.Threading.Tasks.Dataflow;

namespace AsyncOrigoSpike
{
    internal class QueryRequest
    {
        public readonly Query Query;
        public readonly WriteOnceBlock<object> Response;

        public QueryRequest(Query query, WriteOnceBlock<object> responseBlock)
        {
            Response = responseBlock;
            Query = query;
        }
    }
}
