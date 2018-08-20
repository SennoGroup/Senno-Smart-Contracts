using System;

namespace Neo.Manager
{
    public class TransactionExecutedEventArgs : EventArgs
    {
        public string Operation { get; set; }

        public UInt256 TxHash { get; set; }

        public bool Success { get; set; }

        public TransactionExecutedEventArgs(string operation, UInt256 txHash, bool success)
        {
            Operation = operation;
            TxHash = txHash;
            Success = success;
        }
    }
}
