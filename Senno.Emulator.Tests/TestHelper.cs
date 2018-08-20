using System.Linq;
using System.Numerics;
using LunarParser;
using Neo.Emulation;
using Neo.VM;
using NUnit.Framework;

namespace Senno.Tests
{
    public static class TestHelper
    {
        public const string TokenSmartContractFilePath = "../../../../SmartContracts/Senno.SmartContracts.TSC/bin/Debug/Senno.SmartContracts.TSC.avm";
        public const string RewardsSmartContractFilePath = "../../../../SmartContracts/Senno.SmartContracts.SRSC/bin/Debug/Senno.SmartContracts.SRSC.avm";
        public const string ParseDispatcherSmartContractFilePath = "../../../../SmartContracts/Senno.SmartContracts.SPDSC/bin/Debug/Senno.SmartContracts.SPDSC.avm";

        public static StackItem Execute(this Emulator emulator, string operation, params object[] args)
        {
            var inputs = DataNode.CreateArray();
            inputs.AddValue(operation);

            var parameters = CreateParameters(args);
            if (parameters != null)
            {
                inputs.AddNode(parameters);
            }
            else
            {
                inputs.AddValue(null);
            }

            var script = emulator.GenerateLoaderScriptFromInputs(inputs, new ABI());
            emulator.Reset(script, null, null);
            emulator.Run();

            var result = emulator.GetOutput();

            Assert.NotNull(result);
            return result;
        }

        private static DataNode CreateParameters(params object[] args)
        {
            if (args.Length > 0)
            {
                var parameters = DataNode.CreateArray();
                foreach (var a in args)
                {
                    switch (a)
                    {
                        case byte[] bytes:
                            var bytesArray = DataNode.CreateArray();
                            bytes.Reverse().ToList().ForEach(b => bytesArray.AddValue(b));
                            parameters.AddNode(bytesArray);
                            break;
                        case byte[][] arrBytes:
                            var arr = DataNode.CreateArray();
                            var inputs = DataNode.CreateArray();
                            foreach (var bytes in arrBytes)
                            {
                                var _bytesArray = DataNode.CreateArray();
                                bytes.Reverse().ToList().ForEach(b => _bytesArray.AddValue(b));
                                inputs.AddNode(_bytesArray);
                            }
                            arr.AddNode(inputs);
                            parameters.AddNode(arr);
                            break;
                        case int _:
                        case long _:
                        case BigInteger _:
                            var arrData = DataNode.CreateArray();
                            arrData.AddValue(a);
                            parameters.AddNode(arrData);
                            break;
                        default:
                            parameters.AddValue(a);
                            break;
                    }
                }

                return parameters;
            }

            return null;
        }
    }
}
