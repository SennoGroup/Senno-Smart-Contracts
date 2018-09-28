using LunarParser;
using Neo.Emulation;
using Neo.VM;
using System.Linq;
using System.Numerics;

namespace Senno.SmartContracts.Tests.Common
{
    public static class Helper
    {
        public const string TokenSmartContractFilePath =
            "../../../../../Senno.SmartContracts/STSC/bin/Debug/Senno.SmartContracts.STSC.avm";
        public const string RewardsSmartContractFilePath =
            "../../../../../Senno.SmartContracts/SRSC/bin/Debug/Senno.SmartContracts.SRSC.avm";
        public const string ClientsSmartContractFilePath =
            "../../../../../Senno.SmartContracts/SCSC/bin/Debug/Senno.SmartContracts.SCSC.avm";
        public const string TaskDispatcherSmartContractFilePath =
            "../../../../../Senno.SmartContracts/STDSC/bin/Debug/Senno.SmartContracts.STDSC.avm";

        public static StackItem Execute(this Emulator emulator, string operation, IScriptTable table = null,
            params object[] args)
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
            emulator.Reset(script, null, null, table);
            emulator.Run();

            return emulator.GetOutput();
        }

        private static DataNode CreateParameters(params object[] args)
        {
            if (args != null && args.Length > 0)
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
                                var innerBytesArray = DataNode.CreateArray();
                                bytes.Reverse().ToList().ForEach(b => innerBytesArray.AddValue(b));
                                inputs.AddNode(innerBytesArray);
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