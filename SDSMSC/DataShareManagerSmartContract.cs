using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts
{
    /// <summary>
    /// Data Share Manager (DSM) SC
    /// stores data hashes of completed tasks and information
    /// on the data that was provided to the consumer,
    /// actual data is stored in DDS.
    /// The DSM also initiates reward process for data sharing. 
    /// </summary>
    public class DataShareManagerSmartContract : SmartContract
    {
        public delegate void DataShareManagerAction<in T, in T1, in T2, in T3, in T4>(T p0, T1 p1, T2 p2, T3 p3, T4 p4);

        /// <summary>
        /// operation
        /// number
        /// sender
        /// receiver
        /// resource
        /// </summary>
        [DisplayName("dataShareManagerEvent")]
        public static event DataShareManagerAction<string, BigInteger, byte[], byte[], byte[]> DataShareManagerEvent;

        public static object Main(string operation, params object[] args)
        {

            if (operation == Operations.DataShareManagerGet)
            {
                if (args.Length != 1) return false;
                BigInteger number = (BigInteger)args[0];
                return Get(number);
            }
            if (operation == Operations.DataShareManagerCreate)
            {
                if (args.Length != 3) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                byte[] resource = (byte[])args[2];
                return Create(from, to, resource);
            }

            if (operation == Operations.DataShareManagerDelete)
            {
                if (args.Length != 1) return false;
                BigInteger number = (BigInteger)args[0];
                byte[] caller = (byte[])args[1];
                return Delete(number, caller);
            }

            return false;
        }

        private static BigInteger Create(byte[] from, byte[] to, byte[] resource)
        {
            BigInteger number = GetCounter() + 1;

            SetCounter(number);

            // create new share resource
            var shareResource = new DataShareManagerResource()
            {
                Number = number,
                Owner = from,
                SennoId = to,
                Resource = resource
            };

            // save resource to storage
            Storage.Put(Storage.CurrentContext, number.AsByteArray(), shareResource.Serialize());

            ResourceNotification(Operations.DataShareManagerCreate, shareResource.Number, from, to, resource);

            return shareResource.Number;
        }

        private static object Get(BigInteger requestNumber)
        {
            var storageResource = Storage.Get(Storage.CurrentContext, requestNumber.AsByteArray());
            if (storageResource == null || storageResource.Length == 0) return null;
            var shareResource = (DataShareManagerResource)storageResource.Deserialize();

            return shareResource;
        }

        private static bool Delete(BigInteger number, byte[] caller)
        {
            // get resource from storage
            var storageResource = Storage.Get(Storage.CurrentContext, number.AsByteArray());
            if (storageResource == null || storageResource.Length == 0) return true;

            var shareResource = (DataShareManagerResource)storageResource.Deserialize();

            if (shareResource.Owner != caller)
            {
                return false;
            }

            // remove resource from storage
            Storage.Delete(Storage.CurrentContext, number.AsByteArray());

            // event for platform
            ResourceNotification(Operations.DataShareManagerDelete, shareResource.Number, shareResource.Owner, shareResource.SennoId, shareResource.Resource);

            return true;
        }

        /// <summary>
        /// Returns the total number of resource
        /// </summary>
        private static BigInteger GetCounter()
        {
            byte[] counter = Storage.Get(Storage.CurrentContext, "managerCounter");

            if (counter == null || counter.Length == 0)
            {
                return 0;
            }
            return counter.AsBigInteger();
        }

        /// <summary>
        /// Sets the total number of resource
        /// </summary>
        /// <param name="managerCounter">Total number of resource</param>
        private static void SetCounter(BigInteger managerCounter)
        {
            Storage.Put(Storage.CurrentContext, "managerCounter", managerCounter);
        }

        private static void ResourceNotification(string operation, BigInteger requestNumber, byte[] from, byte[] to, byte[] resource)
        {
            // ReSharper disable once PossibleNullReferenceException
            DataShareManagerEvent.Invoke(operation, requestNumber, from, to, resource);
        }
    }
}
