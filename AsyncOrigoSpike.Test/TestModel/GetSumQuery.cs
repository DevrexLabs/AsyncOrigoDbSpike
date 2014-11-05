namespace AsyncOrigoSpike.Test
{
    public class GetSumQuery : Query<IntModel, int>
    {

        public override int Execute(IntModel model)
        {
            return model.Value;
        }
    }
}