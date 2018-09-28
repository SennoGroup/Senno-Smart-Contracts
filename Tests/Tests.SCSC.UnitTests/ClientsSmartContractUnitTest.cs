using Neo.Emulation;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Neo.VM;
using Neo.VM.Types;
using Senno.SmartContracts.Common;
using Senno.SmartContracts.Tests.Common;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace Senno.SmartContracts.Tests.SCSC.UnitTests
{
    public class ClientsSmartContractUnitTest
    {
        private static byte[][] _scriptHashes;

        private static Emulator _emulator;

        private static Client ToClient(StackItem stackItem)
        {
            var args = stackItem is Struct @struct ? @struct.ToArray() : null;
            if (args == null)
            {
                return new Client();
            }

            Client client = new Client
            {
                ScriptHash = ((ByteArray)args[0]).GetByteArray(),
                Ranking = args[1].GetBigInteger(),
                Status = (int)args[2].GetBigInteger(),
                StorageIndex = (int)args[3].GetBigInteger()
            };
            return client;
        }

        public ClientsSmartContractUnitTest()
        {
            var chain = new Blockchain();
            _emulator = new Emulator(chain, true);
            var owner = chain.DeployContract("owner", File.ReadAllBytes(Helper.ClientsSmartContractFilePath));

            chain.CreateAddress("account1");
            var account1 = chain.FindAddressByName("account1");
            chain.CreateAddress("account2");
            var account2 = chain.FindAddressByName("account2");

            _emulator.SetExecutingAccount(owner);
            Runtime.invokerKeys = owner.keys;

            _scriptHashes = new[] { owner, account1, account2 }
                .Select(a => a.keys.address.AddressToScriptHash())
                .ToArray();
        }

        private static BigInteger ExecuteClientAdd(byte[] scriptHash)
        {
            var clientIndex = _emulator.Execute(Operations.ClientAdd, null, scriptHash).GetBigInteger();
            Console.WriteLine($"Init result: {clientIndex}");
            Assert.True(clientIndex >= BigInteger.Zero);
            return clientIndex;
        }

        [Fact]
        public void T01_CheckReturnFalseWhenInvalidOperation()
        {
            var result = _emulator.Execute("invalidOperation").GetBoolean();
            Console.WriteLine($"Result: {result}");
            Assert.False(result, "Invalid operation execution result should be false");
        }

        [Fact]
        public void T02_ClientAdd()
        {
            var clientIndex = ExecuteClientAdd(_scriptHashes[1]);
            Assert.Equal(clientIndex, BigInteger.Zero);
        }

        [Fact]
        public void T03_ClientList()
        {
            var firstClientIndex = ExecuteClientAdd(_scriptHashes[1]);
            Assert.Equal(BigInteger.Zero, firstClientIndex);
            var secondClientIndex = ExecuteClientAdd(_scriptHashes[2]);
            Assert.Equal(BigInteger.One, secondClientIndex);
            var clients = (Map)_emulator.Execute(Operations.Clients);
            Assert.Equal(2, clients.Count);
            var firstAccount = clients[firstClientIndex].GetByteArray();
            Assert.Equal(firstAccount.ToHexString(), _scriptHashes[1].ToHexString());
        }

        [Fact]
        public void T04_ClientGet()
        {
            var clientIndex = ExecuteClientAdd(_scriptHashes[1]);
            Assert.Equal(BigInteger.Zero, clientIndex);
            var client = ToClient(_emulator.Execute(Operations.ClientGet, null, _scriptHashes[1]));
            Assert.Equal(20, client.ScriptHash.Length);
        }

        [Fact]
        public void T05_ClientRemove()
        {
            ExecuteClientAdd(_scriptHashes[1]);
            var clients = (Map)_emulator.Execute(Operations.Clients);
            Assert.Single(clients);
            var result = _emulator.Execute(Operations.ClientRemove, null, _scriptHashes[1]).GetBoolean();
            Assert.True(result);
            clients = (Map)_emulator.Execute(Operations.Clients);
            Assert.Empty(clients);
        }

        [Fact]
        public void T06_ClientChangeStatus()
        {
            ExecuteClientAdd(_scriptHashes[1]);
            var result = _emulator.Execute(Operations.ClientChangeStatus, null, _scriptHashes[1], (int)ClientStatusEnum.Disabled).GetBoolean();
            Assert.True(result);
            var client = ToClient(_emulator.Execute(Operations.ClientGet, null, _scriptHashes[1]));
            Assert.Equal((int)ClientStatusEnum.Disabled, client.Status);
        }

        [Fact]
        public void T07_ClientGetNext()
        {
            var firstClientIndex = ExecuteClientAdd(_scriptHashes[1]);
            Assert.Equal(firstClientIndex, BigInteger.Zero);
            var secondClientIndex = ExecuteClientAdd(_scriptHashes[2]);
            Assert.Equal(secondClientIndex, BigInteger.One);
            var clients = (Map)_emulator.Execute(Operations.Clients);
            Assert.Equal(2, clients.Count);
            var client1 = ToClient(_emulator.Execute(Operations.ClientGetNext));
            Assert.Equal(client1.ScriptHash.ToHexString(), _scriptHashes[1].ToHexString());
            var client2 = ToClient(_emulator.Execute(Operations.ClientGetNext));
            Assert.Equal(client2.ScriptHash.ToHexString(), _scriptHashes[2].ToHexString());
        }
    }
}
