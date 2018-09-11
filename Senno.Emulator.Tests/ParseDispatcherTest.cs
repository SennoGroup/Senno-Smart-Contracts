using System;
using System.IO;
using System.Linq;
using System.Numerics;
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



    }
}
