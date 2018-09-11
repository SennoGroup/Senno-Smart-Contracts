using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;

namespace Senno.SmartContracts.SCSC
{
    /// <summary>
    /// Senno Control Network.
    /// Clients Reestr Smart Contract
    /// </summary>
    public class ClientsSmartContract : SmartContract
    {
        public static object Main(string operation, params object[] args)
        {
            if (operation == Operations.ClientGetRandom)
            {
                return ClientGetRandom();
            }
            if (operation == Operations.ClientAdd)
            {
                return ClientAdd((byte[])args[0]);
            }
            if (operation == Operations.ClientRemove)
            {
                return ClientRemove((byte[])args[0]);
            }
            if (operation == Operations.ClientChangeStatus)
            {
                return ClientChangeStatus((byte[])args[0], (byte)args[1]);
            }
            return false;
        }

        /// <summary>
        /// Get random client from exists
        /// </summary>
        /// <returns></returns>
        private static object ClientGetRandom()
        {
            var existClients = GetClientsList();
            if (existClients != null)
            {
                Random rnd = new Random();
                var clientScriptHash = existClients[rnd.Next(0, existClients.Keys.Length - 1)];
                return GetClientFromStorage(clientScriptHash);
            }
            return null;
        }

        /// <summary>
        /// Add new client by client's script hash
        /// </summary>
        /// <param name="clientScriptHash">client's script hash</param>
        /// <returns></returns>
        private static bool ClientAdd(byte[] clientScriptHash)
        {
            var existClients = GetClientsList();
            if (!ClientExist(existClients, clientScriptHash))
            {
                var client = GetClientFromStorage(clientScriptHash);
                client.StorageIndex = existClients.Keys.Length;
                SetClientToStorage(clientScriptHash, client);
                existClients[client.StorageIndex] = clientScriptHash;
                SetClientsList(existClients);
            }
            return true;
        }

        /// <summary>
        /// Remove client by client's script hash
        /// </summary>
        /// <param name="clientScriptHash">client's script hash</param>
        /// <returns></returns>
        private static bool ClientRemove(byte[] clientScriptHash)
        {
            var existClients = GetClientsList();
            if (ClientExist(existClients, clientScriptHash))
            {
                var client = GetClientFromStorage(clientScriptHash);
                if (client.StorageIndex >= 0)
                {
                    Storage.Delete(Storage.CurrentContext, clientScriptHash);
                    existClients.Remove(client.StorageIndex);
                    SetClientsList(existClients);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Change client status
        /// </summary>
        /// <param name="clientScriptHash">client's script hash</param>
        /// <param name="status">new status</param>
        /// <returns></returns>
        private static bool ClientChangeStatus(byte[] clientScriptHash, byte status)
        {
            var existClients = GetClientsList();
            if (ClientExist(existClients, clientScriptHash))
            {
                var client = GetClientFromStorage(clientScriptHash);
                client.Status = status;
                SetClientToStorage(clientScriptHash, client);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the list of clients scripthashes
        /// </summary>
        private static Map<int, byte[]> GetClientsList()
        {
            var storageIndex = Storage.Get(Storage.CurrentContext, "clientsIndex");

            if (storageIndex == null || storageIndex.Length == 0)
            {
                return new Map<int, byte[]>();
            }
            return (Map<int, byte[]>)storageIndex.Deserialize();
        }

        /// <summary>
        /// Sets the list of clients scripthashes
        /// </summary>
        /// <param name="clientsIndex">Total number of clients</param>
        private static void SetClientsList(Map<int, byte[]> clientsIndex)
        {
            Storage.Put(Storage.CurrentContext, "clientsIndex", clientsIndex.Serialize());
        }

        /// <summary>
        /// Check in client exist
        /// </summary>
        /// <param name="clientsIndex"></param>
        /// <param name="clientScriptHash"></param>
        /// <returns></returns>
        private static bool ClientExist(Map<int, byte[]> clientsIndex, byte[] clientScriptHash)
        {
            if (clientsIndex == null || clientsIndex.Keys.Length == 0)
            {
                return false;
            }
            foreach (byte[] existClientScriptHas in clientsIndex.Values)
            {
                if (existClientScriptHas == clientScriptHash)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns this client by client script hash
        /// </summary>
        private static Client GetClientFromStorage(byte[] clientScriptHash)
        {
            var storageClient = Storage.Get(Storage.CurrentContext, clientScriptHash);

            if (storageClient == null || storageClient.Length == 0)
            {
                return new Client() { ScriptHash = clientScriptHash, Status = (byte)ClientStatusEnum.Enabled, Ranking = 0, StorageIndex = -1 };
            }
            return (Client)storageClient.Deserialize();
        }

        /// <summary>
        /// Sets this client by client script hash
        /// </summary>
        /// <param name="clientScriptHash">client script hash</param>
        /// <param name="client">Client</param>
        private static void SetClientToStorage(byte[] clientScriptHash, Client client)
        {
            Storage.Put(Storage.CurrentContext, clientScriptHash, client.Serialize());
        }
    }
}
