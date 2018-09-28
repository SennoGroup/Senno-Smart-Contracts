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
        /// Job status <see cref="JobStatusEnum"/>
        /// </summary>
        public byte Status { get; set; }
        /// <summary>
        /// Index of current execution task
        /// </summary>
        public int CurrentTaskIndex { get; set; }
        /// <summary>
        /// Job tasks list <see cref="Task"/>
        /// </summary>
        public Task[] Tasks { get; set; }
    }
}
