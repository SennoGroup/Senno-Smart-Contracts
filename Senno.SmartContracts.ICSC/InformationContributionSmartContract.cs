using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.Numerics;

namespace Senno.SmartContracts
{
    /// <summary>
    /// Information contribution smart contract (ICSC).
    /// This SC will pay reward to Information contributors,
    /// which gave Senno listeners access to their private 
    /// channel / group while keeping the data unrevealed
    /// </summary>
    public class InformationContributionSmartContract : SmartContract
    {
        // payload exchange rate
        public static int SwapRate() => Configuration.InformationContributionSmartContractSwapRate;

        // TODO set ScriptHash of deployed SennoTokenSmartContract HERE
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object TokenSmartContract(string method, params object[] args);

        public static object Main(string operation, object[] args)
        {
            // TODO Stepochkin. Check args array length
            if (operation == "assign") return Assign((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
            if (operation == "rewards") return Rewards((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
            return false;
        }

        private static object Assign(byte[] from, byte[] to, BigInteger payload)
        {
            // validating input data
            if (!ValidateInputData(payload))
            {
                return false;
            }

            // get current payload from storage by address 'to'
            var currentPayload = Storage.Get(Storage.CurrentContext, to).AsBigInteger();

            // return the rest of the current payload
            var restPayload = CalculateAndTransferTokens(from, to, currentPayload + payload);

            // save current payload to storage by address 'to'
            Storage.Put(Storage.CurrentContext, to, restPayload);

            return true;
        }

        /// <summary>
        /// Pay reward to Information contributors
        /// </summary>
        private static object Rewards(byte[] from, byte[] to, BigInteger payload)
        {
            // validating input data
            if (!ValidateInputData(payload))
            {
                return false;
            }

            // get current payload from storage by address 'to'
            var currentPayload = Storage.Get(Storage.CurrentContext, to).AsBigInteger();

            // check that the value does not exceed the current payload
            if (payload > currentPayload)
            {
                return false;
            }

            // calculate and transfer tokens
            // return the rest of the current payload
            var restPayload = CalculateAndTransferTokens(from, to, payload);

            // save current payload to storage by address 'to'
            Storage.Put(Storage.CurrentContext, to, currentPayload - restPayload);

            return true;
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

        private static BigInteger CalculateAndTransferTokens(byte[] from, byte[] to, BigInteger payload)
        {
            // check that addresses 'from' and 'to' do not match
            if (from != to)
            {
                // calculte tokens count by swap rate
                var tokensCount = payload / SwapRate();

                // if the tokensCount is greater than zero
                if (tokensCount > 0)
                {
                    // call external method for transfer token to address 'to'
                    if (((bool)TokenSmartContract("transfer", from, to, tokensCount)))
                    {
                        // if the transfer operation is successful
                        // calculate the rest of the current payload
                        payload = payload % SwapRate();
                    }
                }
            }

            return payload;
        }
    }
}