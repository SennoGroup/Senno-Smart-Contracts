using Neo.VM;
using System;

namespace Neo.Manager
{
    public class TransactionExecutedOnEngineEventArgs : EventArgs
    {
        public string Operation { get; set; }

        public VMState VMState { get; set; }

        public RandomAccessStack<StackItem> EvaluationStack { get; set; }

        public Func<StackItem, string> ResultToStringHandler { get; set; }

        public TransactionExecutedOnEngineEventArgs(string operation, VMState vmState, RandomAccessStack<StackItem> evaluationStack, Func<StackItem, string> resultToStringHandler)
        {
            Operation = operation;
            VMState = vmState;
            EvaluationStack = evaluationStack;
            ResultToStringHandler = resultToStringHandler;
        }
    }
}
