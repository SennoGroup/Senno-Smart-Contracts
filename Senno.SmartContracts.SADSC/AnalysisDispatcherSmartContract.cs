using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace Senno.SmartContracts.SADSC
{
    public class AnalysisDispatcherSmartContract : SmartContract
    {
        // TODO set RewardsSmartContractScriptHash
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object RewardsSmartContract(string method, params object[] args);

        public static void Main()
        {
            Storage.Put(Storage.CurrentContext, "Hello", "World");
        }
    }
}
