using System.Numerics;

namespace Senno.SmartContracts.Common
{
    /// <summary>
    /// Job
    /// </summary>
    public struct Job
    {
        /// <summary>
        /// Job number
        /// </summary>
        public BigInteger Number { get; set; }
        /// <summary>
        /// Job type <see cref="JobTypeEnum"/>
        /// </summary>
        public byte Type { get; set; }
        /// <summary>
        /// Job status <see cref="JobStatusEnum"/>
        /// </summary>
        public byte Status { get; set; }
    }
}
