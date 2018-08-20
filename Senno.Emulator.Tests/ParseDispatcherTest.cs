using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Neo.Emulation;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using NUnit.Framework;
using Senno.SmartContracts.Common;

namespace Senno.Tests
{
    [TestFixture]
    public class ParseDispatcherTest
    {
        private byte[][] _scriptHashes;

        private static Blockchain _chain;
        private static Emulator _emulator;
        private static Account _owner;
        private static Account _account1;
        private static Account _account2;

        [SetUp]
        public void Setup()
        {
            _chain = new Blockchain();
            _emulator = new Emulator(_chain);
            _owner = _chain.DeployContract("owner", File.ReadAllBytes(TestHelper.ParseDispatcherSmartContractFilePath));

            _chain.CreateAddress("account1");
            _account1 = _chain.FindAddressByName("account1");
            _chain.CreateAddress("account2");
            _account2 = _chain.FindAddressByName("account2");

            _emulator.SetExecutingAccount(_owner);
            Runtime.invokerKeys = _owner.keys;

            _scriptHashes = new[] { _owner, _account1, _account2 }
                .Select(a => a.keys.address.AddressToScriptHash())
                .ToArray();
        }

        private static BigInteger ExecuteCreateTask()
        {
            var createResult = _emulator.Execute(Operations.CreateTask, "test-source");
            Console.WriteLine($"Create result: {createResult}");
            //Assert.NotZero((int)createResult);
            return 1;
        }

        [Test]
        public void T01_CheckCreateTask()
        {
            ExecuteCreateTask();
        }

        [Test]
        public void T02_CheckGetTask()
        {
            var taskNumber = ExecuteCreateTask();
            var result = _emulator.Execute(Operations.GetTask, taskNumber);
            Console.WriteLine($"Result: {result}");
            Assert.AreEqual(taskNumber, result);
        }

        [Test]
        public void T3_stringToTask()
        {
            //var result = ByteArrayToTask(StringToByteArray("81090201020201010100000474657374010001000201028002010001000100"));

            var task = new Task()
            {
                Number = 1,
                Status = (byte)TaskStatusEnum.Created,
                Source = "test",
                VerificationNeeded = 2,
                Verifications = new Verification[2]
            };

            var bytes = SerializeTask(task).ToHexString();
            Console.Write(bytes);
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private Task ByteArrayToTask(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {

                var formatter = new BinaryFormatter();
                return (Task)formatter.Deserialize(stream);
            }
        }

        public static byte[] SerializeTask(Task task)
        {
            List<byte> result = new List<byte>();
            result.AddRange(task.Number.ToByteArray());
            result.Add(task.Status);
            result.AddRange(task.Owner);
            result.AddRange(Encoding.UTF8.GetBytes(task.Source));
            result.AddRange(Encoding.UTF8.GetBytes(task.Destination));
            result.AddRange(task.Payload.ToByteArray());
            result.AddRange(BitConverter.GetBytes(task.VerificationNeeded));
            result.AddRange(BitConverter.GetBytes(task.VerificationNeeded));
            result.AddRange(BitConverter.GetBytes(task.IsSuccess));

            return result.ToArray();
        }
    }
}
