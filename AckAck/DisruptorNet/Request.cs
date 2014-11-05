using System.Threading.Tasks;

namespace AsyncOrigoSpike
{
    public class Request
    {
        //either a query or a command
        public object Transaction;
        public TaskCompletionSource<object> Response;
    }
}