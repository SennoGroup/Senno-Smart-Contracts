using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.NEP6;
using Neo.Network;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Manager
{
    public class NeoManager : IDisposable
    {
        #region private constants and fields
        /// <summary>
        /// Название блокчейна
        /// </summary>
        private const string solarChainName = "SolarNET";

        /// <summary>
        /// Словарь обработчиков результата выполнения транзакции
        /// </summary>
        private Dictionary<UInt256, (string operation, bool success, Action<bool>)> _transactionHandlers;
        #endregion

        #region events
        /// <summary>
        /// Событие успешной синхронизации ноды
        /// </summary>
        public event EventHandler NodeSyncCompleted;
        /// <summary>
        /// Событие получения стека выполнения операции на локальной ноде
        /// </summary>
        public event EventHandler<TransactionExecutedOnEngineEventArgs> TransactionExecutedOnEngine;
        /// <summary>
        /// Событие создания транзакции в кошельке и получения хэша
        /// </summary>
        public event EventHandler<TransactionCreatedInWalletEventArgs> TransactionCreatedInWallet;
        /// <summary>
        /// Событие успешного выполнения транзакции в сети
        /// </summary>
        public event EventHandler<TransactionExecutedEventArgs> TransactionExecuted;
        #endregion

        #region properties
        /// <summary>
        /// Признак окончания синхронизации ноды
        /// </summary>
        public bool NodeSyncComplete { get; set; }

        /// <summary>
        /// Блокчейн
        /// </summary>
        public Blockchain Blockchain { get; set; }

        /// <summary>
        /// Локальная нода
        /// </summary>
        public LocalNode Node { get; set; }

        /// <summary>
        /// Текущий открытый кошелек
        /// </summary>
        public Wallet CurrentWallet { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public NeoManager()
        {
            _transactionHandlers = new Dictionary<UInt256, (string operation, bool success, Action<bool>)>();
            NodeSyncComplete = false;

            // Регистрируем блокчейн в системе
            Blockchain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(solarChainName));

            // Поднимаем локальную ноду блокчейна
            Node = new LocalNode();

            // Стартуем локальную ноду
            Node.Start();

            // Подписываемся на событие успешного сохранения нового блока 
            Blockchain.PersistCompleted += Blockchain_PersistCompleted;

            // Подписываемся на событие успешного выполнения транзакции
            LevelDBBlockchain.ApplicationExecuted += LevelDBBlockchain_ApplicationExecuted;
        }

        #endregion

        #region public methods
        /// <summary>
        /// Возвращает кошелек для работы с транзакциями
        /// </summary>
        /// <param name="path">Путь к файлу кошелька</param>
        /// <param name="password">Пароль к кошельку</param>
        public Wallet OpenWallet(string path, string password)
        {
            NEP6Wallet nep6wallet = new NEP6Wallet(path);
            nep6wallet.Unlock(password);

            CurrentWallet = nep6wallet;

            return nep6wallet;
        }


        /// <summary>
        /// Выполняет транзакцию в сети
        /// </summary>
        /// <param name="scriptHash">ScriptHash смартконтракта к которому происходит обращение</param>
        /// <param name="operation">Операция, которая вызывается у смартконтракта</param>
        /// <param name="parameters">Параметры передаваемые при вызове операции</param>
        /// <param name="wallet">Кошелек, от имени которого выполняется операция (если не передавать, будет использоваться последний открытый)</param>
        /// <param name="resultToStringHandler">Обработчик приведения результата к строке</param>
        /// <param name="handler">Обработчик результата выполнения транзакции</param>
        /// <returns>Успешность выполнения операции</returns>
        public bool ExecuteTransaction(
            string scriptHash,
            string operation,
            ContractParameter[] parameters = null,
            Wallet wallet = null,
            Func<StackItem, string> resultToStringHandler = null,
            Action<bool> handler = null)
        {
            if (wallet == null)
            {
                if (CurrentWallet == null) return false;
                wallet = CurrentWallet;
            }

            byte[] transactionScript = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                if (parameters != null)
                {
                    transactionScript = VM.Helper.EmitAppCall(sb, UInt160.Parse(scriptHash), operation, parameters).ToArray();
                }
                else
                {
                    transactionScript = VM.Helper.EmitAppCall(sb, UInt160.Parse(scriptHash), operation).ToArray();
                }
            }

            // Формируем объект транзакции
            var transactionObject = new InvocationTransaction
            {
                Version = 1,
                Script = transactionScript,
                Gas = Fixed8.Zero,
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new TransactionOutput[0]
            };
            // Выполняем транзакцию на локальном движке
            ApplicationEngine engine = ApplicationEngine.Run(transactionScript, transactionObject);

            // Кидаем событие с результатми выполнения транзакции на локальном движке
            TransactionExecutedOnEngine?.Invoke(this, new TransactionExecutedOnEngineEventArgs(operation, engine.State, engine.EvaluationStack, resultToStringHandler));

            // Если произошла ошибка при выполнении, то не продолжаем
            if (engine.State.HasFlag(VMState.FAULT))
            {
                return false;
            }

            // Определяем необходимый для выполнения NEO Gas
            transactionObject.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);
            if (transactionObject.Gas < Fixed8.Zero) transactionObject.Gas = Fixed8.Zero;
            transactionObject.Gas = transactionObject.Gas.Ceiling();

            // Создаем транзакции в кошельке
            var transaction = wallet.MakeTransaction(transactionObject,
                fee: transactionObject.Gas.Equals(Fixed8.Zero) ? Fixed8.FromDecimal(0.001m) : Fixed8.Zero,
                from: wallet.GetAccounts().First().ScriptHash);

            // Кидаем событие с хэшем созданной транзакции
            TransactionCreatedInWallet?.Invoke(this, new TransactionCreatedInWalletEventArgs(operation, transaction.Hash));

            // Создаем контекст транзакции для подписи его кошельком
            var context = new ContractParametersContext(transaction);

            // Подписываем контекст кошельком
            if (wallet.Sign(context))
            {
                // Если контекст подписан успешно
                if (context.Completed)
                {
                    context.Verifiable.Scripts = context.GetScripts();
                    // Применяем транзакцию
                    wallet.ApplyTransaction(transaction);
                    // Передаем транзакцию в локальную ноду
                    if (Node.Relay(transaction))
                    {
                        // если передан обработчик результата, складываем его в массив
                        if (handler != null)
                        {
                            _transactionHandlers.Add(transaction.Hash, (operation, false, handler));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Последовательно выполняет очередь транзакций
        /// </summary>
        /// <param name="transactions">Очередь транзакций</param>
        /// <param name="completeHandler">Метод, выполняющий при окончании операции</param>
        public void ExecuteTransactions(Queue<NeoTransaction> transactions, Action<bool> completeHandler = null)
        {
            NeoTransaction current;
            if (!transactions.TryDequeue(out current))
            {
                if (completeHandler != null)
                {
                    completeHandler.Invoke(true);
                    return;
                }
            }

            Action<bool> handler = null;
            if (current.Invoke)
            {
                handler = (bool success) =>
                {
                    if (!success)
                    {
                        completeHandler.Invoke(false);
                        return;
                    }
                    ExecuteTransactions(transactions, completeHandler);
                };
            }

            var executionResult = ExecuteTransaction(current.ContractScriptHash, current.Operation, current.Parameters, current.Wallet, current.ResultToStringHandler, handler);
            if (executionResult)
            {
                if (!current.Invoke)
                {
                    ExecuteTransactions(transactions, completeHandler);
                }
            }
            else
            {
                completeHandler.Invoke(false);
                return;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Обработчик события сохранения нового блока в блокчейн
        /// </summary>
        private void Blockchain_PersistCompleted(object sender, Block e)
        {
            if (((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds) - e.Timestamp < 10)
            {
                if (!NodeSyncComplete)
                {
                    NodeSyncComplete = true;
                    NodeSyncCompleted(sender, null);
                }
            }

            // Данный код проверяет что транзакция успешно выполненая записана в блок в локальный блокчейн
            if (e.Transactions != null && e.Transactions.Length > 0)
            {
                foreach (var transaction in e.Transactions)
                {
                    if (_transactionHandlers.ContainsKey(transaction.Hash))
                    {
                        var (operation, result, handler) = _transactionHandlers[transaction.Hash];
                        handler?.Invoke(result);
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события успешного выполнения транзакции
        /// </summary>
        private void LevelDBBlockchain_ApplicationExecuted(object sender, ApplicationExecutedEventArgs e)
        {
            // Данный код заносит в словарь информацию об успешности выполнения транзакции
            if (_transactionHandlers.ContainsKey(e.Transaction.Hash))
            {
                var (operation, result, handler) = _transactionHandlers[e.Transaction.Hash];
                result = e.ExecutionResults.FirstOrDefault()?.VMState.HasFlag(VMState.HALT) == true;
                _transactionHandlers[e.Transaction.Hash] = (operation, result, handler);
                TransactionExecuted?.Invoke(sender, new TransactionExecutedEventArgs(operation, e.Transaction.Hash, result));
            }
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (Node != null)
            {
                Node.Dispose();
            }
            if (Blockchain != null)
            {
                Blockchain.Dispose();
            }
        }
        #endregion
    }
}
