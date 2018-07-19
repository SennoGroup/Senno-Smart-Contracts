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
        public const string TokenSmartContractOwner = "bb4b3659159122b242e0674131fe0656aff8c356";

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
       
    }
}
