using Neo.Emulation;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Senno.SmartContracts.Common;
using Senno.SmartContracts.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace Senno.SmartContracts.Tests.STSC.UnitTests
{
    public class TokenSmartContractUnitTest
    {
        private static Emulator _emulator;
        private static Account _tokenAccount;
        private static List<Account> _accounts;

        public TokenSmartContractUnitTest()
        {
            var chain = new Blockchain();
            _emulator = new Emulator(chain);

            _tokenAccount = chain.DeployContract("STSC", File.ReadAllBytes(Helper.TokenSmartContractFilePath));

            _emulator.SetExecutingAccount(_tokenAccount);
            Runtime.invokerKeys = _tokenAccount.keys;

            _accounts = new List<Account>();
            for (var i = 0; i < 10; i++)
            {
                var accountName = Guid.NewGuid().ToString();
                chain.CreateAddress(accountName);
                _accounts.Add(chain.FindAddressByName(accountName));
            }
        }

        [Fact]
        public void T01_CheckReturnFalseWhenInvalidOperation()
        {
            var result = _emulator.Execute("invalidOperation").GetBoolean();
            Assert.False(result);
        }

        [Fact]
        public void T02_CheckOwner()
        {
            var owner = _emulator.Execute(Operations.TokenOwner).GetByteArray().ToHexString();
            Assert.Equal(Configuration.TokenSmartContractOwner, owner);
        }

        [Fact]
        public void T03_CheckName()
        {
            var name = _emulator.Execute(Operations.TokenName).GetString();
            Assert.Equal(Configuration.TokenSmartContractName, name);
        }

        [Fact]
        public void T04_CheckSymbol()
        {
            var symbol = _emulator.Execute(Operations.TokenSymbol).GetString();
            Assert.Equal(Configuration.TokenSmartContractSymbol, symbol);
        }

        [Fact]
        public void T05_CheckDecimals()
        {
            var decimals = _emulator.Execute(Operations.TokenDecimals).GetBigInteger();
            Assert.Equal(Configuration.TokenSmartContractDecimals, decimals);
        }

        [Fact]
        public void T06_CheckInitSupply()
        {
            var decimals = _emulator.Execute(Operations.TokenInitSupply).GetBigInteger();
            Assert.Equal(Configuration.TokenSmartContractInitSupply, decimals);
        }

        [Fact]
        public void T07_CheckTotalSupplyBeforeDeploy()
        {
            var totalSupply = _emulator.Execute(Operations.TokenTotalSupply).GetBigInteger();
            Assert.Equal(0, totalSupply);
        }

        [Fact]
        public void T08_CheckDeploy()
        {
            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);
        }

        [Fact]
        public void T09_CheckReturnFalseWhenDeployTwice()
        {
            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);

            var secondDeployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.False(secondDeployResult);
        }

        [Fact]
        public void T10_CheckTotalSupplyAfterDeploy()
        {
            var totalSupply = _emulator.Execute(Operations.TokenTotalSupply).GetBigInteger();
            Assert.Equal(0, totalSupply);

            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);

            totalSupply = _emulator.Execute(Operations.TokenTotalSupply).GetBigInteger();
            Assert.Equal(Configuration.TokenSmartContractInitSupply, totalSupply);
        }

        [Theory]
        [InlineData("test")]
        [InlineData(345, 45)]
        [InlineData("test", 120)]
        [InlineData(new byte[] {1, 45, 65, 23, 5}, 120)]
        [InlineData(new byte[] {1, 45, 65, 23, 5}, typeof(string))]
        public void T11_CheckReturnFalseWhenBalanceCallWithNonValidArguments(object firstArgs = null,
            object secondArgs = null)
        {
            var balanceResult = _emulator.Execute(Operations.TokenBalanceOf, null, firstArgs, secondArgs).GetBoolean();
            Assert.False(balanceResult);
        }

        [Fact]
        public void T12_CheckOwnerBalanceBeforeDeploy()
        {
            var balanceResult = _emulator
                .Execute(Operations.TokenBalanceOf, null, _tokenAccount.keys.address.AddressToScriptHash()).GetBigInteger();
            Assert.Equal(balanceResult, BigInteger.Zero);
        }

        [Fact]
        public void T13_CheckOwnerBalanceAfterDeploy()
        {
            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);

            var balanceResult = _emulator
                .Execute(Operations.TokenBalanceOf, null, Configuration.TokenSmartContractOwner.HexToBytes())
                .GetBigInteger();
            Assert.Equal(balanceResult, Configuration.TokenSmartContractInitSupply);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test", 120)]
        [InlineData(new byte[] {1, 45, 65, 23, 5}, 120, 45)]
        [InlineData(new byte[] {1, 45, 65, 23, 5}, 120, 45, "sdfs")]
        public void T14_CheckReturnFalseWhenTransferCallWithNonValidArguments(object firstArgs = null,
            object secondArgs = null, object thirdArgs = null, object fourthArgs = null)
        {
            var balanceResult = _emulator
                .Execute(Operations.TokenBalanceOf, null, firstArgs, secondArgs, thirdArgs, fourthArgs).GetBoolean();
            Assert.False(balanceResult);
        }

        [Fact]
        public void T15_CheckOwnerTransferBeforeDeploy()
        {
            var scriptHash = Configuration.TokenSmartContractOwner.HexToBytes();
            var account = _accounts.First();
            var amount = 1000;

            var transferResult = _emulator.Execute(Operations.TokenTransfer, null, scriptHash,
                account.keys.address.AddressToScriptHash(), amount).GetBoolean();
            Assert.False(transferResult);
        }

        [Fact]
        public void T16_CheckOwnerTransferAfterDeploy()
        {
            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);

            var scriptHash = Configuration.TokenSmartContractOwner.HexToBytes();
            var accountScriptHash = _accounts.First().keys.address.AddressToScriptHash();
            var amount = 1200;

            var balanceOwnerBeforeTransfer = _emulator.Execute(Operations.TokenBalanceOf, null, scriptHash).GetBigInteger();
            var balanceAccountBeforeTransfer =
                _emulator.Execute(Operations.TokenBalanceOf, null, accountScriptHash).GetBigInteger();

            // It's very IMPORTANT
            // When you worked with Numeric values you needed convert your value to decimal type
            var transferResult = _emulator
                .Execute(Operations.TokenTransfer, null, scriptHash, accountScriptHash, (decimal)amount)
                .GetBoolean();
            Assert.True(transferResult);

            var balanceOwnerAfterTransfer = _emulator.Execute(Operations.TokenBalanceOf, null, scriptHash).GetBigInteger();
            Assert.Equal(balanceOwnerBeforeTransfer - amount, balanceOwnerAfterTransfer);

            var balanceAccountAfterTransfer =
                _emulator.Execute(Operations.TokenBalanceOf, null, accountScriptHash).GetBigInteger();
            Assert.Equal(balanceAccountBeforeTransfer + amount, balanceAccountAfterTransfer);
        }
    }
}