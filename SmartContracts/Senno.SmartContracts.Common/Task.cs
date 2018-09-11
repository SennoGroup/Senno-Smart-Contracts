using System.Numerics;

namespace Senno.SmartContracts.Common
{
    /// <summary>
    /// Task
    /// </summary>
    public struct Task
    {
        /// <summary>
        /// Task number
        /// </summary>
        public BigInteger Number;

        /// <summary>
        /// Task status <see cref="TaskStatusEnum"/>
        /// </summary>
        public byte Status;

        /// <summary>
        /// Task worker address
        /// </summary>
        public byte[] Worker;

        /// <summary>
        /// Task candidats in worker
        /// 
        /// </summary>
        public byte[][] CandidatesInWorker;

        /// <summary>
        /// Source file hash from IPFS
        /// </summary>
        public string Source;

        /// <summary>
        /// Destination file hash from IPFS
        /// </summary>
        public string Destination;

        /// <summary>
        /// Task payload
        /// </summary>
        public BigInteger Payload;

        /// <summary>
        /// Number of verifications required
        /// </summary>
        public int VerificationNeeded;

        /// <summary>
        /// Values of verifications
        /// </summary>
        public Verification[] Verifications;

        /// <summary>
        /// Success of the task
        /// </summary>
        public bool IsSuccess;

        /// <summary>
        /// Tokens swaprate for worker
        /// </summary>
        public int WorkerSwaprate;

        /// <summary>
        /// Tokens swaprate for verificator
        /// </summary>
        public int VerificatorSwaprate;

        /// <summary>
        /// Job number
        /// </summary>
        public BigInteger JobNumber;

        /// <summary>
        /// Task type <see cref="TaskTypeEnum"/>
        /// </summary>
        public byte Type;
    }
}
