namespace Senno.SmartContracts.Common
{
    /// <summary>
    /// Common configuration data
    /// </summary>
    public class Configuration
    {
        #region TSC

        /// <summary>
        /// Address wallet of the owner TokenSmartContract
        /// </summary>
        public const string TokenSmartContractOwner = "APgeXL3KRP1kGf4MVPLF2muRUB8U6s5cgz";

        /// <summary>
        /// Token smart contract name 
        /// </summary>
        public const string TokenSmartContractName = "Senno";

        /// <summary>
        /// Token smart contract symbol
        /// </summary>
        public const string TokenSmartContractSymbol = "SEN";

        /// <summary>
        /// Token smart contract decimals
        /// </summary>
        public const byte TokenSmartContractDecimals = 0;

        /// <summary>
        /// Token smart contract init supply
        /// </summary>
        public const ulong TokenSmartContractInitSupply = 10000000000;

        #endregion

        #region ICSC

        /// <summary>
        /// Payload exchange rate for Information contribution smart contract
        /// </summary>
        public const int InformationContributionSmartContractSwapRate = 1000;

        #endregion

        #region SADSC

        /// <summary>
        /// Payload exchange rate for Analysis Dispatcher smart contract
        /// </summary>
        public const int AnalysisDispatcherSmartContractSwapRate = 1000;

        #endregion

        #region SPDSC

        /// <summary>
        /// Payload exchange rate for Parse Dispatcher smart contract
        /// </summary>
        public const int ParseDispatcherSmartContractSwapRate = 1000;

        #endregion

        #region STDSC

        /// <summary>
        /// 
        /// </summary>
        public const byte TaskCandidatesInWorkerNumber = 2;

        #endregion

        #region SDSC

        /// <summary>
        /// Payload exchange rate for Software development smart contract
        /// </summary>
        public const int SoftwareDevelopmentSmartContractSwapRate = 1000;

        #endregion
    }
}