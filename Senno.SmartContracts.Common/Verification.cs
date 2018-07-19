using System;

namespace Senno.SmartContracts.Common
{
    /// <summary>
    /// Verification result info
    /// </summary>
    public struct Verification
    {
        /// <summary>
        /// Verificator address
        /// </summary>
        public byte[] Owner;

        /// <summary>
        /// Verisifation result
        /// </summary>
        public bool Result;
    }
}
