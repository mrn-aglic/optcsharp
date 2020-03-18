using System;
using TracingCore.TracingManagers;

namespace TracingCore.Exceptions
{
    public class NumIterationsException : Exception
    {
        public override string Message =>
            $"The maximum number of iterations (${MaxIterations}) was exceeded by ${LoopType.ToString()} loop at row and column: {Line}:{Character}";

        public int Iterations { get; }
        public int MaxIterations { get; }
        public LoopType LoopType { get; }
        public int Line { get; }
        public int Character { get; }

        public NumIterationsException(int iters, int maxIters, LoopType type, int line, int character)
        {
            Iterations = iters;
            MaxIterations = maxIters;
            LoopType = type;
            Line = line;
            Character = character;
        }
    }
}