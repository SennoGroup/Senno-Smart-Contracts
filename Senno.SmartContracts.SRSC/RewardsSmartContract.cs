using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts.SRSC
{
    public class RewardsSmartContract : SmartContract
    {
        // payload exchange rate for analys
        public static int AnalysisSwapRate() => Configuration.AnalysisDispatcherSmartContractSwapRate;

        // payload exchange rate for parse
        public static int ParseSwapRate() => Configuration.ParseDispatcherSmartContractSwapRate;

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
            // create new job
            if (operation == Operations.Parse)
            {
                return RewardsForTask((byte[])args[0], (BigInteger)args[1], (object[])args[2], ParseSwapRate());
            }
            // call next step of job
            if (operation == Operations.Analysis)
            {
                return RewardsForTask((byte[])args[0], (BigInteger)args[1], (object[])args[2], AnalysisSwapRate());
            }

            return false;
        }

        /// <summary>
        /// Rewards for task
        /// </summary>
        /// <param name="taskWorker"></param>
        /// <param name="payload"></param>
        /// <param name="taskVerificators"></param>
        /// <returns></returns>
        private static object RewardsForTask( byte[] taskWorker, BigInteger payload, object[] taskVerificators, int swapRate)
        {
            Rewards(taskWorker, payload, swapRate);

            if (taskVerificators != null && taskVerificators.Length > 0)
            {
                foreach (byte[] verificator in taskVerificators)
                {
                    Rewards(verificator, payload, swapRate * 3);
                }
            }
            return true;
        }

        /// <summary>
        /// Pay reward to address
        /// </summary>
        private static void Rewards(byte[] to, BigInteger payload, int swapRate)
        {
            // get stored payload from storage by address 'to'
            var storedPayload = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, 0);

            // calculate and transfer tokens
            // return the rest of the current payload
            var restPayload = CalculateAndTransferTokens(to, storedPayload + payload, swapRate);

            // save current payload to storage by address 'to'
            Storage.Put(Storage.CurrentContext, to, restPayload);
        }

        /// <summary>
        /// Calculate tokens amount and transfer
        /// </summary>
        /// <param name="to"></param>
        /// <param name="payload"></param>
        /// <param name="swapRate"></param>
        /// <returns></returns>
        private static BigInteger CalculateAndTransferTokens(byte[] to, BigInteger payload, int swapRate)
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
                    if ((bool)TokenSmartContract("transfer", coinBase, to, tokensCount))
                    {
                        // if the transfer operation is successful
                        // calculate the rest of the current payload
                        payload = payload % swapRate;
                    }
                }
            }

            return payload;
        }
    }
}
