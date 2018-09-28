using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;

namespace Senno.SmartContracts
{
    public class DataShareRequestSmartContract : SmartContract
    {

        public delegate void DataShareRequestAction<in T, in T1, in T2, in T3, in T4>(T p0, T1 p1, T2 p2, T3 p3, T4 p4);

        /// <summary>
        /// operation
        /// number
        /// sender
        /// receiver
        /// resource
        /// </summary>
        [DisplayName("dataShareRequest")]
        public static event DataShareRequestAction<string, BigInteger, byte[], byte[], byte[]> DataShareRequested;

        public static object Main(string operation, params object[] args)
        {
            if (operation == Operations.DataShareRequestManagerCreate)
            {
                if (args.Length != 3) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                byte[] resource = (byte[])args[2];
                return Create(from, to, resource);
            }

            if (operation == Operations.DataShareRequestManagerGet)
            {
                if (args.Length != 1) return false;
                BigInteger request = (BigInteger)args[0];
                return Get(request);
            }

            if (operation == Operations.DataShareRequestManagerResolve)
            {
                if (args.Length != 2) return false;
                BigInteger request = (BigInteger)args[0];
                byte[] to = (byte[])args[1];
                return Resolve(request, to);
            }

            if (operation == Operations.DataShareRequestManagerReject)
            {
                if (args.Length != 2) return false;
                BigInteger request = (BigInteger)args[0];
                byte[] to = (byte[])args[1];
                return Reject(request, to);
            }

            if (operation == Operations.DataShareRequestManagerConfirm)
            {
                if (args.Length != 2) return false;
                BigInteger request = (BigInteger)args[0];
                byte[] from = (byte[])args[1];
                return Confirm(request, from);
            }

            return false;
        }

        private static BigInteger Create(byte[] from, byte[] to, byte[] resource)
        {
            BigInteger number = GetCounter() + 1;

            SetCounter(number);

            // create new request
            var request = new DataShareRequest()
            {
                Number = number,
                Status = (byte)TaskStatusEnum.Created,
                From = from,
                To = to,
                Resource = resource
            };

            // save task to storage
            Storage.Put(Storage.CurrentContext, number.AsByteArray(), request.Serialize());

            RequestNotification(Operations.DataShareRequestManagerCreate, request.Number, from, to, resource);

            return request.Number;
        }

        private static object Get(BigInteger requestNumber)
        {
            var storageRequest = Storage.Get(Storage.CurrentContext, requestNumber.AsByteArray());
            if (storageRequest == null || storageRequest.Length == 0) return null;
            var request = (DataShareRequest)storageRequest.Deserialize();

            return request;
        }

        private static bool Resolve(BigInteger requestNumber, byte[] caller)
        {
            // get task from storage
            var storageRequest = Storage.Get(Storage.CurrentContext, requestNumber.AsByteArray());
            if (storageRequest == null || storageRequest.Length == 0) return false;

            var request = (DataShareRequest)storageRequest.Deserialize();
            if (request.Number == 0)
            {
                return false;
            }

            // if task owner exists
            if (request.Status != (byte)DataShareRequestStatusEnum.Created)
            {
                return false;
            }

            if (request.To != caller)
            {
                return false;
            }

            // set request status
            request.Status = (byte)DataShareRequestStatusEnum.Resolved;

            // save request to storage
            Storage.Put(Storage.CurrentContext, request.Number.AsByteArray(), request.Serialize());

            // event for platform
            RequestNotification(Operations.DataShareRequestManagerResolve, request.Number, request.From, request.To, request.Resource);

            return true;
        }

        private static bool Reject(BigInteger requestNumber, byte[] caller)
        {
            // get task from storage
            var storageRequest = Storage.Get(Storage.CurrentContext, requestNumber.AsByteArray());
            if (storageRequest == null || storageRequest.Length == 0) return false;

            var request = (DataShareRequest)storageRequest.Deserialize();
            if (request.Number == 0)
            {
                return false;
            }

            // if task owner exists
            if (request.Status != (byte)DataShareRequestStatusEnum.Created)
            {
                return false;
            }

            // if task owner exists
            if (request.To != caller)
            {
                return false;
            }

            // set request status
            request.Status = (byte)DataShareRequestStatusEnum.Rejected;

            // save request to storage
            Storage.Put(Storage.CurrentContext, request.Number.AsByteArray(), request.Serialize());

            // event for platform
            RequestNotification(Operations.DataShareRequestManagerReject, request.Number, request.From, request.To, request.Resource);

            return true;
        }

        private static bool Confirm(BigInteger requestNumber, byte[] caller)
        {
            // get task from storage
            var storageRequest = Storage.Get(Storage.CurrentContext, requestNumber.AsByteArray());
            if (storageRequest == null || storageRequest.Length == 0) return false;

            var request = (DataShareRequest)storageRequest.Deserialize();
            if (request.Number == 0)
            {
                return false;
            }

            // if task owner exists
            if (request.Status != (byte)DataShareRequestStatusEnum.Resolved)
            {
                return false;
            }

            if (request.From != caller)
            {
                return false;
            }

            // set request status
            request.Status = (byte)DataShareRequestStatusEnum.Confirmed;

            // save request to storage
            Storage.Put(Storage.CurrentContext, request.Number.AsByteArray(), request.Serialize());

            // event for platform
            RequestNotification(Operations.DataShareRequestManagerConfirm, request.Number, request.From, request.To, request.Resource);

            return true;
        }

        /// <summary>
        /// Returns the total number of requests
        /// </summary>
        private static BigInteger GetCounter()
        {
            byte[] counter = Storage.Get(Storage.CurrentContext, "requestsCounter");

            if (counter == null || counter.Length == 0)
            {
                return 0;
            }
            return counter.AsBigInteger();
        }

        /// <summary>
        /// Sets the total number of requests
        /// </summary>
        /// <param name="requestsCounter">Total number of tasks</param>
        private static void SetCounter(BigInteger requestsCounter)
        {
            Storage.Put(Storage.CurrentContext, "requestsCounter", requestsCounter);
        }

        private static void RequestNotification(string operation, BigInteger requestNumber, byte[] from, byte[] to, byte[] resource)
        {
            DataShareRequested.Invoke(operation, requestNumber, from, to, resource);
        }
    }
}
