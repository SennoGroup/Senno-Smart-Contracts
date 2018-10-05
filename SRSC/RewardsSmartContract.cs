using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts
{
    public class RewardsSmartContract : SmartContract
    {
        // safe store of SEN tokens
        private static readonly byte[] CoinBase = Configuration.TokenSmartContractOwner.ToScriptHash();

        [Appcall("8a5f11a1c82a064be5d9e355bed78b57a01200b5")]
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
            if (operation == Operations.RewardsReward)
            {
                if (args.Length != 6) return false;
                return RewardsForTask((byte[])args[0], (BigInteger)args[1], (int)args[2], (object[])args[3], (int)args[4], (byte)args[5]);
            }

            return false;
        }

        /// <summary>
        /// Rewards for task
        /// </summary>
        /// <param name="taskWorker"></param>
        /// <param name="payload"></param>
        /// <param name="workerSwapRate"></param>
        /// <param name="taskVerificators"></param>
        /// <param name="verificatorSwapRate"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        private static bool RewardsForTask(byte[] taskWorker, BigInteger payload, int workerSwapRate, object[] taskVerificators, int verificatorSwapRate, byte taskType)
        {
            Rewards(taskWorker, payload, workerSwapRate, taskType);

            if (taskVerificators != null && taskVerificators.Length > 0)
            {
                foreach (byte[] verificator in taskVerificators)
                {
                    Rewards(verificator, payload, verificatorSwapRate, taskType);
                }
            }
            return true;
        }

        /// <summary>
        /// Pay reward to address
        /// </summary>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool Rewards(byte[] to, BigInteger payload, int swapRate, byte taskType)
        {
            // get stored payload from storage by address 'to'
            BigInteger storedPayload = 0;
            string key = taskType + "_" + to;
            byte[] storedPayloadBytes
                = Storage.Get(Storage.CurrentContext, key);
            if (storedPayloadBytes != null && storedPayloadBytes.Length != 0)
            {
                storedPayload = storedPayloadBytes.AsBigInteger();
            }
            Storage.Put(Storage.CurrentContext, key, 0);

            // calculate and transfer tokens
            bool transferResult = CalculateAndTransferTokens(to, storedPayload + payload, swapRate);
            // if transfer fails restore payload
            if (!transferResult)
            {
                Storage.Put(Storage.CurrentContext, key, storedPayload);
            }

            return transferResult;
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
            if (CoinBase != to)
            {
                // calculate tokens count by swap rate
                BigInteger tokensCount = payload / swapRate;

                // if the tokensCount is greater than zero
                if (tokensCount > 0)
                {
                    // call external method for transfer token to address 'to'
                    return (bool)TokenSmartContract("transfer", CoinBase, to, tokensCount);
                }
            }
            return false;
        }
    }
}
