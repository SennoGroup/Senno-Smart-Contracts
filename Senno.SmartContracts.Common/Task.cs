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
        /// Task owner address
        /// </summary>
        public byte[] Owner;

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
    }
}
