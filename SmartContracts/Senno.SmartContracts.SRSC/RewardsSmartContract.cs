using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts.SRSC
{
    public class RewardsSmartContract : SmartContract
    {
        // safestore of SEN tokens
        public static readonly byte[] coinBase = Neo.SmartContract.Framework.Helper.HexToBytes(Configuration.TokenSmartContractOwner);

        [Appcall("849f7fd094ac87bd3a797d2fdaddcd927050d686")]
        public static extern object TokenSmartContract(string method, params object[] args);

        /// <summary>
        /// Smart contract entry method
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Main(string operation, params object[] args)
        {
            // calculate and pay reward for payload
            if (operation == Operations.Reward)
            {
                return RewardsForTask((byte[])args[0], (BigInteger)args[1], (object[])args[2], (int)args[3], (int)args[4], (byte)args[5]);
            }

            return false;
        }

        /// <summary>
        /// Rewards for task
        /// </summary>
        /// <param name="taskWorker"></param>
        /// <param name="payload"></param>
        /// <param name="taskVerificators"></param>
        /// <param name="workerSwaprate"></param>
        /// <param name="verificatorSwaprate"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        private static object RewardsForTask(byte[] taskWorker, BigInteger payload, object[] taskVerificators, int workerSwaprate, int verificatorSwaprate, byte taskType)
        {
            Rewards(taskWorker, payload, workerSwaprate, taskType);

            if (taskVerificators != null && taskVerificators.Length > 0)
            {
                foreach (byte[] verificator in taskVerificators)
                {
                    Rewards(verificator, payload, verificatorSwaprate, taskType);
                }
            }
            return true;
        }

        /// <summary>
        /// Pay reward to address
        /// </summary>
        private static void Rewards(byte[] to, BigInteger payload, int swapRate, byte taskType)
        {
            //TODO: store payload by taskType
            // get stored payload from storage by address 'to'
            var storedPayload = Storage.Get(Storage.CurrentContext, taskType + "_" + to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, taskType + "_" + to, 0);

            // calculate and transfer tokens
            var transferResult = CalculateAndTransferTokens(to, storedPayload + payload, swapRate);
            // if transfer fails restore payload
            if (!transferResult)
            Storage.Put(Storage.CurrentContext, taskType + "_" + to, storedPayload);
        }

        /// <summary>
        /// Calculate tokens amount and transfer
        /// </summary>
        /// <param name="to"></param>
        /// <param name="payload"></param>
        /// <param name="swapRate"></param>
        /// <returns></returns>
        private static bool CalculateAndTransferTokens(byte[] to, BigInteger payload, int swapRate)
        {
            // check that addresses 'from' and 'to' do not match
            if (coinBase != to)
            {
                // calculte tokens count by swap rate
                var tokensCount = payload / swapRate;

                // if the tokensCount is greater than zero
                if (tokensCount > 0)
                {
                    // call external method for transfer token to address 'to'
                    return (bool)TokenSmartContract("transfer", coinBase, to, tokensCount);
                }
            }
            return false;
        }
    }
}
