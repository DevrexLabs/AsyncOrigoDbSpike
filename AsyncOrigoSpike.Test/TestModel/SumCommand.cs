using System;

namespace AsyncOrigoSpike.Test
{
    [Serializable]
    public class SumCommand : Command<IntModel, int>
    {
        public readonly int Operand;
        public SumCommand(int i)
        {
            Operand = i;
        }

        public override int Execute(IntModel model)
        {
            model.Value += Operand;
            return Operand;
        }
    }
}