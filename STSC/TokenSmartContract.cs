using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

// ReSharper disable PossibleNullReferenceException

namespace Senno.SmartContracts
{
    public class TokenSmartContract : SmartContract
    {
        private static readonly byte[] Owner = Configuration.TokenSmartContractOwner.ToScriptHash();

        // Token Settings
        private static string Name() => Configuration.TokenSmartContractName;
        private static string Symbol() => Configuration.TokenSmartContractSymbol;
        private static byte Decimals() => Configuration.TokenSmartContractDecimals;
        private static ulong InitSupply() => Configuration.TokenSmartContractInitSupply;

        public delegate void TransferAction<in T, in T1, in T2>(T p0, T1 p1, T2 p3);

        [DisplayName("transfer")]
        public static event TransferAction<byte[], byte[], BigInteger> Transferred;

        /// <summary>
        /// Smart contract entry method
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Main(string operation, params object[] args)
        {
            if (operation == Operations.TokenOwner)
            {
                return Owner;
            }

            if (operation == Operations.TokenName)
            {
                return Name();
            }

            if (operation == Operations.TokenSymbol)
            {
                return Symbol();
            }

            if (operation == Operations.TokenDecimals)
            {
                return Decimals();
            }

            if (operation == Operations.TokenInitSupply)
            {
                return InitSupply();
            }

            if (operation == Operations.TokenDeploy)
            {
                return Deploy();
            }

            if (operation == Operations.TokenTotalSupply)
            {
                return TotalSupply();
            }

            if (operation == Operations.TokenBalanceOf)
            {
                if (args.Length != 1) return 0;
                byte[] account = (byte[])args[0];
                return BalanceOf(account);
            }

            if (operation == Operations.TokenTransfer)
            {
                if (args.Length != 3) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                BigInteger value = (BigInteger)args[2];
                return Transfer(from, to, value);
            }

            return false;
        }

        /// <summary>
        /// Initialization parameters, only once
        /// </summary>
        /// <returns></returns>
        private static bool Deploy()
        {
            byte[] totalSupply = Storage.Get(Storage.CurrentContext, "totalSupply");
            if (totalSupply.Length != 0) return false;

            Storage.Put(Storage.CurrentContext, Owner, InitSupply());
            Storage.Put(Storage.CurrentContext, "totalSupply", InitSupply());

            return true;
        }

        /// <summary>
        /// Get the total token supply
        /// </summary>
        /// <returns></returns>
        private static BigInteger TotalSupply()
        {
            return Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        }

        /// <summary>
        /// Get the account balance of another account with address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static BigInteger BalanceOf(byte[] address)
        {
            return Storage.Get(Storage.CurrentContext, address).AsBigInteger();
        }

        /// <summary>
        /// Function that is always called when someone wants to transfer tokens.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool Transfer(byte[] from, byte[] to, BigInteger value)
        {
            // Verifies that the calling contract has verified the required script hashes of the transaction/block
            /*if (!Runtime.CheckWitness(from))
            {
                return false;
            }*/
            if (value <= 0) return false;
            // Address only
            if (to.Length != 20) return false;

            // Get the account value of the source accounts.
            BigInteger fromValue = Storage.Get(Storage.CurrentContext, from).AsBigInteger();

            if (fromValue < value) return false;

            if (from == to) return true;

            if (fromValue == value)
                Storage.Delete(Storage.CurrentContext, from);
            else
                Storage.Put(Storage.CurrentContext, from, fromValue - value);

            // Get the account value of the destination accounts.
            BigInteger toValue = Storage.Get(Storage.CurrentContext, to).AsBigInteger();

            // Set the new account value of the destination accounts.
            Storage.Put(Storage.CurrentContext, to, toValue + value);

            Transferred.Invoke(from, to, value);

            return true;
        }
    }
}