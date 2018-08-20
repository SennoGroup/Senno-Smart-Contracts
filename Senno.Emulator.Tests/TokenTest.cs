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
    public class TokenTest
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
            _owner = _chain.DeployContract("owner", File.ReadAllBytes(TestHelper.TokenSmartContractFilePath));

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

        private static void ExecuteDeploy()
        {
            var initResult = _emulator.Execute(Operations.Deploy).GetBoolean();
            Console.WriteLine($"Init result: {initResult}");
            Assert.IsTrue(initResult);
        }

        [Test]
        public void T01_CheckReturnFalseWhenInvalidOperation()
        {
            ExecuteDeploy();
            var result = _emulator.Execute("invalidOperation").GetBoolean();
            Console.WriteLine($"Result: {result}");
            Assert.IsFalse(result, "Invalid operation execution result should be false");
        }

        [Test]
        public void T02_CheckName()
        {
            ExecuteDeploy();
            var name = _emulator.Execute(Operations.Name).GetString();
            Console.WriteLine($"Token name: {name}");
            Assert.AreEqual(Configuration.TokenSmartContractName, name);
        }

        [Test]
        public void T03_CheckSymbol()
        {
            ExecuteDeploy();
            var symbol = _emulator.Execute(Operations.Symbol).GetString();
            Console.WriteLine($"Token symbol: {symbol}");
            Assert.AreEqual(Configuration.TokenSmartContractSymbol, symbol);
        }

        [Test]
        public void T04_CheckDecimals()
        {
            ExecuteDeploy();
            var decimals = _emulator.Execute(Operations.Decimals).GetBigInteger();
            Console.WriteLine($"Token decimals: {decimals}");
            Assert.AreEqual((BigInteger)Configuration.TokenSmartContractDecimals, decimals);
        }

        [Test]
        public void T05_CheckInitSupply()
        {
            ExecuteDeploy();
            var totalSupply = _emulator.Execute(Operations.TotalSupply).GetBigInteger();
            Console.WriteLine($"Total supply: {totalSupply}");
            Assert.AreEqual((BigInteger)Configuration.TokenSmartContractInitSupply, totalSupply);
        }

        [Test]
        public void T06_CheckTransfer()
        {
            ExecuteDeploy();
            var _scriptHash = Configuration.TokenSmartContractOwner.HexToBytes();

            var transferResult = _emulator
                .Execute(Operations.Transfer, _scriptHash, _scriptHashes[1], 5)
                .GetBoolean();
            Console.WriteLine($"Transfer result: {transferResult}");
            Assert.IsTrue(transferResult);
        }

        [Test]
        public void T07_CheckBalanceAfterTransfer()
        {
            ExecuteDeploy();
            var tokensToTransfer = new BigInteger(10);
            var _scriptHash = Configuration.TokenSmartContractOwner.HexToBytes();

            _emulator.checkWitnessMode = CheckWitnessMode.AlwaysTrue;

            var transferResult = _emulator
                .Execute(Operations.Transfer, _scriptHash, _scriptHashes[1], tokensToTransfer)
                .GetBoolean();
            Console.WriteLine($"Transfer result: {transferResult}");

            var balanceFrom = _emulator.Execute(Operations.BalanceOf, _scriptHash).GetBigInteger();
            Console.WriteLine($"Sender balance: {balanceFrom}");

            var balanceTo = _emulator.Execute(Operations.BalanceOf, _scriptHashes[1]).GetBigInteger();
            Console.WriteLine($"Recepient balance: {balanceTo}");

            Assert.AreEqual(tokensToTransfer, balanceTo);
        }

    }
}
