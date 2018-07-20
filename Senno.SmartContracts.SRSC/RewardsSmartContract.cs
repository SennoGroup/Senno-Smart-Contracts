using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts.SRSC
{
    public class RewardsSmartContract : SmartContract
    {

        // payload exchange rate
        public static int AnalysisSwapRate() => Configuration.AnalysisDispatcherSmartContractSwapRate;

        // payload exchange rate
        public static int ParseSwapRate() => Configuration.ParseDispatcherSmartContractSwapRate;

        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object TokenSmartContract(string method, params object[] args);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Main(string operation, params object[] args)
        {
            // create new job
            if (operation == "parse")
            {
                return RewardsForParse(args);
            }
            // call next step of job
            if (operation == "analysis")
            {
                return RewardsForAnalysis(args);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static object RewardsForParse(params object[] args)
        {
            // get task worker address from arguments
            byte[] taskWorker = (byte[])args[0];
            BigInteger payload = (BigInteger)args[1];

            Rewards(ExecutionEngine.CallingScriptHash, taskWorker, payload, ParseSwapRate());
            
            // get task verificators addresses from arguments
            byte[][] taskVerificators = (byte[][])args[2];

            if(taskVerificators != null && taskVerificators.Length > 0)
            {
                foreach(byte[] verificator in taskVerificators)
                {
                    Rewards(ExecutionEngine.CallingScriptHash, verificator, payload, ParseSwapRate() * 3 );
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static object RewardsForAnalysis(params object[] args)
        {
            // get task worker address from arguments
            byte[] taskWorker = (byte[])args[0];
            BigInteger payload = (BigInteger)args[1];

            Rewards(ExecutionEngine.CallingScriptHash, taskWorker, payload, AnalysisSwapRate());

            // get task verificators addresses from arguments
            byte[][] taskVerificators = (byte[][])args[2];

            if (taskVerificators != null && taskVerificators.Length > 0)
            {
                foreach (byte[] verificator in taskVerificators)
                {
                    Rewards(ExecutionEngine.CallingScriptHash, verificator, payload, AnalysisSwapRate() * 3);
                }
            }
            return true;
        }

        /// <summary>
        /// Pay reward to address
        /// </summary>
        private static object Rewards(byte[] from, byte[] to, BigInteger payload, int swapRate)
        {
            // get stored payload from storage by address 'to'
            var storedPayload = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, 0);

            // calculate and transfer tokens
            // return the rest of the current payload
            var restPayload = CalculateAndTransferTokens(from, to, storedPayload + payload, swapRate);

            // save current payload to storage by address 'to'
            Storage.Put(Storage.CurrentContext, to, restPayload);

            return true;
        }

        private static BigInteger CalculateAndTransferTokens(byte[] from, byte[] to, BigInteger payload, int swapRate)
        {
            // check that addresses 'from' and 'to' do not match
            if (from != to)
            {
                // calculte tokens count by swap rate
                var tokensCount = payload / swapRate;

                // if the tokensCount is greater than zero
                if (tokensCount > 0)
                {
                    // call external method for transfer token to address 'to'
                    if (((bool)TokenSmartContract("transfer", from, to, tokensCount)))
                    {
                        // if the transfer operation is successful
                        // calculate the rest of the current payload
                        payload = payload - tokensCount;
                    }
                }
            }

            return payload;
        }
    }
}
