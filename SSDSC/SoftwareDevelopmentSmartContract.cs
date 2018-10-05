using Neo.SmartContract.Framework;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts
{
    /// <summary>
    /// Senno Control Network.
    /// Software Development Smart Contract
    /// Completes a transaction and pays reward
    /// to the software developer once data feed
    /// plugin has been used
    /// </summary>
    public class SoftwareDevelopmentSmartContract : SmartContract
    {
        [Appcall("b4cde8ad63e3c3a3f2c32011c0f3543292f04ccf")]
        public static extern object RewardsSmartContract(string method, params object[] args);

        public static object Main(string operation, object[] args)
        {
            if (operation == Operations.SoftwareDevelopmentReward)
            {
                if (args.Length != 2) return false;
                return Rewards((byte[])args[0], (BigInteger)args[1]);
            }

            return false;
        }

        // payload exchange rate
        private static int SwapRate() => Configuration.SoftwareDevelopmentSmartContractSwapRate;

        /// <summary>
        /// Pay reward to software developer
        /// </summary>
        private static bool Rewards(byte[] to, BigInteger payload)
        {
            // validating input data
            if (!ValidateInputData(payload))
            {
                return false;
            }

            // call external method for transfer token to address 'to'
            // todo current task type == 0x20 (make task type enum)
            return (bool)RewardsSmartContract(Operations.RewardsReward, to, payload, SwapRate(), null, null, 0x20);
        }

        private static bool ValidateInputData(BigInteger payload)
        {
            // check that the payload is not passed a zero value
            if (payload <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
