namespace Senno.SmartContracts.Common
{
    public static class Operations
    {
        #region Token

        public const string TokenOwner = "owner";
        public const string TokenName = "name";
        public const string TokenSymbol = "symbol";
        public const string TokenDecimals = "decimals";
        public const string TokenInitSupply = "initSupply";
        public const string TokenDeploy = "deploy";
        public const string TokenTotalSupply = "totalSupply";
        public const string TokenBalanceOf = "balanceOf";
        public const string TokenTransfer = "transfer";

        #endregion

        #region Rewards

        public const string RewardsReward = "reward";

        #endregion

        #region InformationContribution

        public const string InformationContributionAssign = "assign";
        public const string InformationContributionReward = "reward";

        #endregion

        #region TaskDispatcher

        public const string TaskDispatcherGetTask = "gettask";
        public const string TaskDispatcherCreateTask = "createtask";
        public const string TaskDispatcherTakeTask = "taketask";
        public const string TaskDispatcherCompleteTask = "completetask";
        public const string TaskDispatcherVerifyTask = "verifytask";

        #endregion

        #region BusinessProcess

        public const string CreateJob = "createjob";
        public const string CompleteTaskJob = "completetaskjob";

        #endregion

        #region Clients

        public const string Clients = "clientslist";
        public const string ClientGetNext = "clientgetnext";
        public const string ClientGet = "clientget";
        public const string ClientAdd = "clientadd";
        public const string ClientRemove = "clientremove";
        public const string ClientChangeStatus = "clientchangestatus";

        #endregion

        #region PublicKeysManager

        public const string PublicKeysGet = "publickeyget";
        public const string PublicKeysAdd = "publickeyadd";
        public const string PublicKeysRemove = "publickeyremove";

        #endregion

        #region DataShareRequestManager

        public const string DataShareRequestManagerCreate = "datasharerequestcreate";
        public const string DataShareRequestManagerGet = "datasharerequestget";
        public const string DataShareRequestManagerResolve = "datasharerequestresolve";
        public const string DataShareRequestManagerReject = "datasharerequestreject";
        public const string DataShareRequestManagerConfirm = "datasharerequestconfirm";

        #endregion

        #region DataShareManager

        public const string DataShareManagerCreate = "datasharemanagercreate";
        public const string DataShareManagerGet = "datasharemanagerget";
        public const string DataShareManagerDelete = "datasharemanagerdelete";

        #endregion
    }
}