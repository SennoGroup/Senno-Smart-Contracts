using Neo.Emulation;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Senno.SmartContracts.Common;
using Senno.SmartContracts.Tests.Common;
using System.IO;
using System.Linq;
using Xunit;

namespace Senno.SmartContracts.Tests.SRSC.UnitTests
{
    public class RewardsSmartContractUnitTest
    {
        private static Emulator _emulator;
        private static byte[][] _scriptHashes;
        private static Account _rewardsAccount;
        private static Account _tokenAccount;
        private static ScriptTable _scriptTableRewards;

        public RewardsSmartContractUnitTest()
        {
            var chain = new Blockchain();
            _emulator = new Emulator(chain, true);
            _rewardsAccount = chain.DeployContract("SRSC", File.ReadAllBytes(Helper.RewardsSmartContractFilePath));
            _tokenAccount = chain.DeployContract("STSC", File.ReadAllBytes(Helper.TokenSmartContractFilePath));

            _scriptTableRewards = new ScriptTable();
            _scriptTableRewards.AddScript("0x8a5f11a1c82a064be5d9e355bed78b57a01200b5".HexToBytes(),
                _tokenAccount.byteCode);
            _emulator.SetExecutingAccount(_rewardsAccount);

            chain.CreateAddress("account1");
            var account1 = chain.FindAddressByName("account1");

            _scriptHashes = new[] { _rewardsAccount, account1 }
                .Select(a => a.keys.address.AddressToScriptHash())
                .ToArray();
        }


        [Fact]
        public void T01_CorrectCallRewardsReward()
        {
            var result = _emulator.Execute(Operations.RewardsReward, _scriptTableRewards, _scriptHashes[1], 100, null, 1, 1, (byte)0x0).GetBoolean();
            Assert.True(result);
        }

        [Fact]
        public void T02_CorrectCallRewardsRewardNotDeployTokenSC()
        {
            var amountRewards = 100;

            _emulator.SetExecutingAccount(_tokenAccount);
            var testAccountBalanceBeforeRewards = _emulator
                .Execute(Operations.TokenBalanceOf, null, _scriptHashes[1])
                .GetBigInteger();

            _emulator.SetExecutingAccount(_rewardsAccount);

            var result = _emulator.Execute(Operations.RewardsReward, _scriptTableRewards, _scriptHashes[1], amountRewards, 1, null, 1, (byte)0x0).GetBoolean();
            Assert.True(result);

            _emulator.SetExecutingAccount(_tokenAccount);

            var balanceResult = _emulator
                 .Execute(Operations.TokenBalanceOf, null, _scriptHashes[1])
                 .GetBigInteger();
            Assert.Equal(balanceResult, testAccountBalanceBeforeRewards);
        }

        [Fact]
        public void T03_CorrectCallRewardsRewardWithDeployTokenSC()
        {
            var amountRewards = 100;

            _emulator.SetExecutingAccount(_tokenAccount);
            var deployResult = _emulator.Execute(Operations.TokenDeploy).GetBoolean();
            Assert.True(deployResult);

            var balanceResult = _emulator
                .Execute(Operations.TokenBalanceOf, null, Configuration.TokenSmartContractOwner.AddressToScriptHash())
                .GetBigInteger();
            Assert.Equal(balanceResult, Configuration.TokenSmartContractInitSupply);

            var testAccountBalanceBeforeRewards = _emulator
                .Execute(Operations.TokenBalanceOf, null, _scriptHashes[1])
                .GetBigInteger();

            _emulator.SetExecutingAccount(_rewardsAccount);

            var result = _emulator.Execute(Operations.RewardsReward, _scriptTableRewards, _scriptHashes[1], amountRewards, 1, null, 1, (byte)0x0).GetBoolean();
            Assert.True(result);

            _emulator.SetExecutingAccount(_tokenAccount);

            balanceResult = _emulator
                .Execute(Operations.TokenBalanceOf, null, _scriptHashes[1])
                .GetBigInteger();
            Assert.Equal(balanceResult, testAccountBalanceBeforeRewards + amountRewards);
        }


        [Fact]
        public void T04_InvalidNumberParametersRewardsReward()
        {
            var result = _emulator.Execute(Operations.RewardsReward, _scriptTableRewards, _scriptHashes[1], 1, null, 1, 1).GetBoolean();
            Assert.False(result);
        }
    }
}