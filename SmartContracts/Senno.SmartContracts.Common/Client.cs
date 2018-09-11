using System.Numerics;

namespace Senno.SmartContracts.Common
{
    /// <summary>
    /// Client
    /// </summary>
    public struct Client
    {
        /// <summary>
        /// Client ScriptHash
        /// </summary>
        public byte[] ScriptHash;

        /// <summary>
        /// Client ranking
        /// </summary>
        public BigInteger Ranking;

        /// <summary>
        /// Client status <see cref="ClientStatusEnum"/>
        /// </summary>
        public byte Status;

        /// <summary>
        /// Index into storage
        /// </summary>
        public int StorageIndex;
    }
}
