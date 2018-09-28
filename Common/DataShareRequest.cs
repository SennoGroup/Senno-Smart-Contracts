using System.Numerics;

namespace Senno.SmartContracts.Common
{
    public struct DataShareRequest
    {
        /// <summary>
        /// Request number
        /// </summary>
        public BigInteger Number;

        /// <summary>
        /// Request from
        /// </summary>
        public byte[] From;

        /// <summary>
        /// Request to
        /// </summary>
        public byte[] To;

        /// <summary>
        /// Request resource
        /// </summary>
        public byte[] Resource;

        /// <summary>
        /// Request status <see cref="DataShareRequestStatusEnum"/>
        /// </summary>
        public byte Status;
    }
}
