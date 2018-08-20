using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;

namespace Neo.Manager
{
    /// <summary>
    /// Описывает транзакцию, которую необходимо выполнить в сети
    /// </summary>
    public class NeoTransaction
    {
        /// <summary>
        /// Выполнять транзакция или нет (для чтения данных из сети = false)
        /// </summary>
        public bool Invoke { get; set; }

        /// <summary>
        /// ScriptHash смартконтракта к которому происходит обращение
        /// </summary>
        public string ContractScriptHash { get; set; }

        /// <summary>
        /// Операция, которая вызывается у смартконтракта
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Параметры передаваемые при вызове операции
        /// </summary>
        public ContractParameter[] Parameters { get; set; }

        /// <summary>
        /// Кошелек, от имени которого выполняется операция
        /// </summary>
        public Wallet Wallet { get; set; }

        /// <summary>
        /// Обработчик приведения результата к строке
        /// </summary>
        public Func<StackItem, string> ResultToStringHandler { get; set; }
    }
}
