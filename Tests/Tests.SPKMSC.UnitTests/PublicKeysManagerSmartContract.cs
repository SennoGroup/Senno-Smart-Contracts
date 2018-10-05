using Neo.Emulation;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Senno.SmartContracts.Common;
using Senno.SmartContracts.Tests.Common;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Senno.SmartContracts.Tests.SPKMSC.UnitTests
{
    public class PublicKeysManagerSmartContract
    {
        private static byte[][] _scriptHashes;
        private static Emulator _emulator;
        private static byte[] _testPublicKey;

        public PublicKeysManagerSmartContract()
        {
            var chain = new Blockchain();
            _emulator = new Emulator(chain, true);
            var publicKeysManagerAccount = chain.DeployContract("SPKMSC",
                File.ReadAllBytes(Helper.PublicKeysManagerSmartContractFilePath));

            chain.CreateAddress("account1");
            var account1 = chain.FindAddressByName("account1");

            _emulator.SetExecutingAccount(publicKeysManagerAccount);
            Runtime.invokerKeys = publicKeysManagerAccount.keys;

            _scriptHashes = new[] {publicKeysManagerAccount, account1}
                .Select(a => a.keys.address.AddressToScriptHash())
                .ToArray();

            _testPublicKey = Encoding.ASCII.GetBytes("TestKey");
        }

        [Fact]
        public void T01_PublicKeyGetFirst()
        {
            var publicKey = _emulator.Execute(Operations.PublicKeysGet, null, _scriptHashes[1]).GetByteArray();
            Assert.Equal(publicKey.Length, 0);
        }

        [Fact]
        public void T02_PublicKeysRemoveWhenKeyNotExist()
        {
            var result = _emulator.Execute(Operations.ClientRemove, null, _scriptHashes[1]).GetBoolean();
            Assert.False(result);
        }

        [Fact]
        public void T03_PublicKeyGetIncorrectCountParameters()
        {
            var result = _emulator.Execute(Operations.PublicKeysGet).GetBoolean();
            Assert.False(result);
        }

        [Fact]
        public void T04_PublicKeyAddIncorrectCountParameters()
        {
            var result = _emulator.Execute(Operations.PublicKeysAdd, null, _scriptHashes[1]).GetBoolean();
            Assert.False(result);
        }

        [Fact]
        public void T05_PublicKeyGetAfterAdd()
        {
            var result = _emulator.Execute(Operations.PublicKeysAdd, null, _scriptHashes[1], _testPublicKey)
                .GetBoolean();
            Assert.True(result);

            var publicKey = _emulator.Execute(Operations.PublicKeysGet, null, _scriptHashes[1]).GetByteArray();
            Assert.Equal(publicKey, _testPublicKey);
        }


        [Fact]
        public void T06_PublicKeyGetAfterRemove()
        {
            var publicKey = _emulator.Execute(Operations.PublicKeysGet, null, _scriptHashes[1]).GetByteArray();
            Assert.Equal(publicKey.Length, 0);
        }
    }
}