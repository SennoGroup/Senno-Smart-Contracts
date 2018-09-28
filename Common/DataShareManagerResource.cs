using System.Numerics;

namespace Senno.SmartContracts.Common
{
    public struct DataShareManagerResource
    {
        /// <summary>
        /// Resource number
        /// </summary>
        public BigInteger Number;

        /// <summary>
        /// Resource owner
        /// </summary>
        public byte[] Owner;

        /// <summary>
        /// Resource SennoId
        /// </summary>
        public byte[] SennoId;

        /// <summary>
        /// Request resource
        /// </summary>
        public byte[] Resource;
    }
}
