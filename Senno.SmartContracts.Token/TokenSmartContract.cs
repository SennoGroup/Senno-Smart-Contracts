using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts
{
    public class TokenSmartContract : SmartContract
    {
        public static readonly byte[] owner = Neo.SmartContract.Framework.Helper.HexToBytes(Configuration.TokenSmartContractOwner);

        // Token Settings
        public static string Name() => Configuration.TokenSmartContractName;
        public static string Symbol() => Configuration.TokenSmartContractSymbol;
        public static byte Decimals() => Configuration.TokenSmartContractDecimals;
        private const ulong initSupply = Configuration.TokenSmartContractInitSupply;

        public delegate void TransferAction<in T, in T1, in T2>(T p0, T1 p1, T2 p3);

        [DisplayName("transfer")]
        public static event TransferAction<byte[], byte[], BigInteger> Transferred;

        public static object Main(string operation, params object[] args)
        {
            if (operation == "deploy") return Deploy();
            if (operation == "totalSupply") return TotalSupply();
            if (operation == "name") return Name();
            if (operation == "symbol") return Symbol();
            if (operation == "owner") return owner;
            if (operation == "decimals") return Decimals();
            if (operation == "transfer")
            {
                if (args.Length != 3) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                BigInteger value = (BigInteger)args[2];
                return Transfer(from, to, value);
            }
            if (operation == "balanceOf")
            {
                if (args.Length != 1) return 0;
                byte[] account = (byte[])args[0];
                return BalanceOf(account);
            }

            return false;
        }

        /// <summary>
        /// Initialization parameters, only once
        /// </summary>
        /// <returns></returns>
        private static bool Deploy()
        {
            byte[] total_supply = Storage.Get(Storage.CurrentContext, "totalSupply");
            if (total_supply.Length != 0) return false;

            Storage.Put(Storage.CurrentContext, owner, initSupply);
            Storage.Put(Storage.CurrentContext, "totalSupply", initSupply);

            return true;
        }

        /// <summary>
        /// Function that is always called when someone wants to transfer tokens.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Transfer(byte[] from, byte[] to, BigInteger value)
        {
            // Verifies that the calling contract has verified the required script hashes of the transaction/block
            if (!Runtime.CheckWitness(from))
            {
                return false;
            }
            if (value <= 0) return false;
            // Address only
            if (to.Length != 20) return false;

            // Get the account value of the source accounts.
            BigInteger from_value = Storage.Get(Storage.CurrentContext, from).AsBigInteger();

            if (from_value < value) return false;

            if (from == to) return true;

            if (from_value == value)
                Storage.Delete(Storage.CurrentContext, from);
            else
                Storage.Put(Storage.CurrentContext, from, from_value - value);

            // Get the account value of the destination accounts.
            BigInteger to_value = Storage.Get(Storage.CurrentContext, to).AsBigInteger();

            // Set the new account value of the destination accounts.
            Storage.Put(Storage.CurrentContext, to, to_value + value);

            Transferred(from, to, value);

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
    }
}
