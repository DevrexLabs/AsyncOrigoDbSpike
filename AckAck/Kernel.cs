namespace AckAck
{
    public class Kernel
    {
        readonly object _model;

        public Kernel(object model)
        {
            _model = model;
        }

        public object Execute(Command command)
        {
            return command.ExecuteImpl(_model);
        }

        public object Execute(Query query)
        {
            return query.ExecuteImpl(_model);
        }
    }
}