using System;

namespace Neo.Manager
{
    public class TransactionCreatedInWalletEventArgs : EventArgs
    {
        public string Operation { get; set; }

        public UInt256 TxHash { get; set; }

        public TransactionCreatedInWalletEventArgs(string operation, UInt256 txHash)
        {
            Operation = operation;
            TxHash = txHash;
        }
    }
}
