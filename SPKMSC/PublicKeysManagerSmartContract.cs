using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;

namespace Senno.SmartContracts
{
    /// <summary>
    /// Senno Control Network.
    /// Public Keys Manager Smart Contract
    /// A register of the public keys which are used
    /// for the encryption during the storage stage.
    /// </summary>
    public class PublicKeysManagerSmartContract : SmartContract
    {
        public delegate void PublicKeysManagerAction<in T, in T1, in T2>(T p0, T1 p1, T2 p2);

        /// <summary>
        /// operation
        /// scripthash
        /// publickey
        /// </summary>
        [DisplayName("publicKeysManagerEvent")]
        public static event PublicKeysManagerAction<string, byte[], byte[]> PublicKeysManagerEvent;

        public static object Main(string operation, params object[] args)
        {
            if (operation == Operations.PublicKeysGet)
            {
                if (args.Length != 1) return false;
                return PublicKeyGet((byte[])args[0]);
            }

            if (operation == Operations.PublicKeysAdd)
            {
                if (args.Length != 2) return false;
                return PublicKeyAdd((byte[])args[0], (byte[])args[1]);
            }

            if (operation == Operations.PublicKeysRemove)
            {
                if (args.Length != 1) return false;
                return PublicKeyRemove((byte[])args[0]);
            }

            return false;
        }

        /// <summary>
        /// Get public key by wallet's script hash
        /// </summary>
        /// <param name="scriptHash">Wallet's script hash</param>
        /// <returns>Public key if exist, or null otherwise</returns>
        private static byte[] PublicKeyGet(byte[] scriptHash)
        {
            // get public key from storage
            return Storage.Get(Storage.CurrentContext, scriptHash);
        }

        /// <summary>
        /// Add new public key by wallet's script hash
        /// </summary>
        /// <param name="scriptHash">Wallet's script hash</param>
        /// <param name="publicKey">Wallet's public key</param>
        /// <returns>The success of the operation</returns>
        private static bool PublicKeyAdd(byte[] scriptHash, byte[] publicKey)
        {
            // add public key to storage
            Storage.Put(Storage.CurrentContext, scriptHash, publicKey);

            // event for platform
            PublicKeyNotification(Operations.PublicKeysAdd, scriptHash, publicKey);

            return true;
        }

        /// <summary>
        /// Remove public key by wallet's script hash
        /// </summary>
        /// <param name="scriptHash">Wallet's script hash</param>
        /// <returns>The success of the operation</returns>
        private static bool PublicKeyRemove(byte[] scriptHash)
        {
            // get public key from storage
            var publicKey = Storage.Get(Storage.CurrentContext, scriptHash);
            if (publicKey == null || publicKey.Length == 0) return true;

            // delete public key from storage
            Storage.Delete(Storage.CurrentContext, scriptHash);

            // event for platform
            PublicKeyNotification(Operations.PublicKeysRemove, scriptHash, publicKey);

            return true;
        }

        private static void PublicKeyNotification(string operation, byte[] scriptHash, byte[] publicKey)
        {
            // ReSharper disable once PossibleNullReferenceException
            PublicKeysManagerEvent.Invoke(operation, scriptHash, publicKey);
        }
    }
}