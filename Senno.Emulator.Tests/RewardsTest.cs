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
    public class RewardsTest
    {
        private byte[][] _scriptHashes;

        private static Blockchain _chain;
        private static Emulator _emulator;
        private static Account _rewards;
        private static Account _token;
        private static Account _account1;
        private static Account _account2;

        [OneTimeSetUp]
        public void Setup()
        {
            _chain = new Blockchain();
            _emulator = new Emulator(_chain);
            _rewards = _chain.DeployContract("rewards", File.ReadAllBytes(TestHelper.RewardsSmartContractFilePath));

            _chain.CreateAddress("account1");
            _account1 = _chain.FindAddressByName("account1");
            _chain.CreateAddress("account2");
            _account2 = _chain.FindAddressByName("account2");

            _emulator.SetExecutingAccount(_rewards);
            Runtime.invokerKeys = _rewards.keys;

            _scriptHashes = new[] { _rewards, _account1, _account2 }
                .Select(a => a.keys.address.AddressToScriptHash())
                .ToArray();
        }

        [Test]
        public void T01_CheckReturnFalseWhenInvalidOperation()
        {
            var result = _emulator.Execute("invalidOperation").GetBoolean();
            Console.WriteLine($"Result: {result}");
            Assert.IsFalse(result, "Invalid operation execution result should be false");
        }

        [Test]
        public void T02_CheckRewardsForParse()
        {
            var _scriptHash = Configuration.TokenSmartContractOwner.HexToBytes();
            var transferPayload = new BigInteger(1000);
            var rewardsResult = _emulator.Execute(
                Operations.Parse,
                _scriptHash,
                transferPayload,
                _scriptHashes).GetBoolean();
            Console.WriteLine($"Rewards result: {rewardsResult}");
            Assert.IsTrue(rewardsResult);
        }

    }
}
