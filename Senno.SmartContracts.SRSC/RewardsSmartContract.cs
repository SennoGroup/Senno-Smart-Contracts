using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace Senno.SmartContracts.SRSC
{
    public class RewardsSmartContract : SmartContract
    {
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object TokenSmartContract(string method, params object[] args);


        public static void Main()
        {
            Storage.Put(Storage.CurrentContext, "Hello", "World");
        }
    }
}
