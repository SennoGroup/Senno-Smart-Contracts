using Neo.Core;
using Neo.Manager;
using Neo.SmartContract;
using Neo.VM;
using Newtonsoft.Json;
using Senno.SmartContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Helper = Neo.Helper;

namespace Senno.Tests
{
    internal class Program
    {
        private static NeoManager chainManager;

        private static readonly string walletPath = @"sw1.json";
        private static readonly string walletPassword = "1";

        private static readonly string scriptHashTokenSmartContract = "0x849f7fd094ac87bd3a797d2fdaddcd927050d686";
        private static readonly string scriptHashRewardsSmartContract = "0x0d4a40411eded4c788707dcdb88abccf93356df5";
        private static readonly string scriptHashClientSmartContract = "0x3b2d0d3240bfdea5fabe1833d90271f4c519b974";
        private static readonly string scriptHashTaskDispatcherSmartContract = "0xdd6d388ab71534596b6e31e8f56ad8aee4d47cc5";
        private static readonly string scriptHashParseDispatcherSmartContract = "0x357789484ecbf4d504a239ebbd06e3af4edfad3e";


        private static readonly string scriptHashAccount1 = "d05215e1c57d030e522883d9fe8608605f6a0a15";
        private static readonly string scriptHashAccount2 = "80c83624072c311d3fbf555e596c040a006614eb";
        private static readonly string scriptHashAccount3 = "b45f0323f5cfb1da6787ab460fac1d0bb8218cfe";

        private static void Main(string[] args)
        {
            using (chainManager = new NeoManager())
            {
                // Подписываемся на событие успешного сохранения нового блока 
                Blockchain.PersistCompleted += (sender, e) => { Console.WriteLine(Blockchain.Default.Height); };

                chainManager.TransactionExecutedOnEngine += (sender, e) =>
                {
                    Console.WriteLine($"------ {e.Operation} : local state: {e.VMState.HasFlag(VMState.HALT)}");
                    Console.WriteLine(
                        $"------ {e.Operation} : local result: {JsonConvert.SerializeObject(e.EvaluationStack.ToList().Select(x => e.ResultToStringHandler(x)))}");
                };

                chainManager.TransactionCreatedInWallet += (sender, e) =>
                {
                    Console.WriteLine($"------ {e.Operation} : {e.TxHash}");
                };

                chainManager.TransactionExecuted += (sender, e) =>
                {
                    Console.WriteLine($"------ {e.Operation} : {e.TxHash} : {e.Success}");
                };

                chainManager.NodeSyncCompleted += (sender, e) =>
                {
                    // Открываем кошелек для работы с транзакциями
                    Neo.Wallets.Wallet wallet = chainManager.OpenWallet(walletPath, walletPassword);

                    // Выполним последовательность операций
                    chainManager.ExecuteTransactions(GetTransactions(),
                        success =>
                        {
                            if (success)
                            {
                                Console.WriteLine("------ Все транзакции успешно отправлены.");
                            }
                            else
                            {
                                Console.WriteLine("------ Ошибка при отправке транзакций.");
                            }
                        });
                };

                // Дожидаемся выполнения операции
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static Queue<NeoTransaction> GetTransactions()
        {
            Queue<NeoTransaction> operations = new Queue<NeoTransaction>();

            #region -= TokenSmartContract =-


            //// deploy 
            //operations.Enqueue(new NeoTransaction()
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Deploy,
            //    Invoke = true,
            //    ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString()
            //});

            ////Owner
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Owner,
            //    ResultToStringHandler = item => Helper.ToHexString(item.GetByteArray())
            //});

            //// Total Suply testing
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.TotalSupply,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString()
            //});

            //// Check balance of owner
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(Configuration.TokenSmartContractOwner)
            //        }
            //    }
            //});

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Transfer Testing
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Transfer,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address from
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(Configuration.TokenSmartContractOwner)
            //        },
            //        // Address to
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        },
            //        // Amount
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 10
            //        }
            //    }
            //});

            //// Check balance of owner
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(Configuration.TokenSmartContractOwner)
            //        }
            //    }
            //});

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Name testing
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Name,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetString()
            //});

            //// Symbol testing
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Symbol,
            //    ResultToStringHandler = item => item.GetString()
            //});

            //// Decimals testing
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.Decimals,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString()
            //});

            #endregion

            #region -= RewardsSmartContract =-

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Check balance of wallet 2
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount2)
            //        }
            //    }
            //});

            //// Check balance of wallet 3
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount3)
            //        }
            //    }
            //});

            //// Reward operation with taskverificators
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashRewardsSmartContract,
            //    Operation = Operations.Reward,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Task worker address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        },
            //        // Payload
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 100
            //        },
            //        // Task verificators array
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Array,
            //            Value = new[]
            //            {
            //                // Address
            //                new ContractParameter
            //                {
            //                    Type = ContractParameterType.ByteArray,
            //                    Value = Helper.HexToBytes(scriptHashAccount2)
            //                },
            //                // Address
            //                new ContractParameter
            //                {
            //                    Type = ContractParameterType.ByteArray,
            //                    Value = Helper.HexToBytes(scriptHashAccount3)
            //                }
            //            }
            //        },
            //        // Worker swaprate
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 10
            //        },
            //        // Verificators swaprate
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 5
            //        },
            //        // Task type
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 101
            //        },
            //    }
            //});

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Check balance of wallet 2
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount2)
            //        }
            //    }
            //});

            //// Check balance of wallet 3
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount3)
            //        }
            //    }
            //});

            #endregion

            #region -= TaskDispatcherSmartContract =-
            ////Add task number
            //int taskNumber = 28;

            //// Create task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.CreateTask,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Job number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 2
            //        },
            //        // Source
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.String,
            //            Value = "source"
            //        },
            //        // Worker swaprate
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 100
            //        },
            //        // Verificator swaprate
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 100
            //        },
            //        // Task type
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 101
            //        },
            //        // Verificators needed
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 2
            //        }
            //    }
            //});

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Check balance of wallet 2
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount2)
            //        }
            //    }
            //});

            //// Check balance of wallet 3
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount3)
            //        }
            //    }
            //});

            //// Get task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.GetTask,
            //    ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeTask(item)),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        }
            //    }
            //});

            //// Take task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.TakeTask,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        },
            //        // Caller address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            //// Get task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.GetTask,
            //    ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeTask(item)),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        }
            //    }
            //});

            //// Complete task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.CompleteTask,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        },
            //        // Caller address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        },
            //        // Destination
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.String,
            //            Value = "destination"
            //        },
            //        // Payload
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 10000
            //        }
            //    }
            //});

            //// Get task test (Destination must exist, status = 3)
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.GetTask,
            //    ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeTask(item)),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        }
            //    }
            //});

            //// Verify task test 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.VerifyTask,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        },
            //        // Caller address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount2)
            //        },
            //        // Result
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Boolean,
            //            Value = true
            //        },
            //        // Payload
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 2000
            //        }
            //    }
            //});

            //// Verify task test 2
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.VerifyTask,
            //    Invoke = true,
            //    ResultToStringHandler = item => item.GetBoolean().ToString(),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        },
            //        // Caller address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount3)
            //        },
            //        // Result
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Boolean,
            //            Value = true
            //        },
            //        // Payload
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = 2000
            //        }
            //    }
            //});

            //// Check balance of wallet 1
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount1)
            //        }
            //    }
            //});

            ////Check balance of wallet 2
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount2)
            //        }
            //    }
            //});

            ////Check balance of wallet 3
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTokenSmartContract,
            //    Operation = Operations.BalanceOf,
            //    ResultToStringHandler = item => item.GetBigInteger().ToString(),
            //    Parameters = new[]
            //    {
            //        // Address
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.ByteArray,
            //            Value = Helper.HexToBytes(scriptHashAccount3)
            //        }
            //    }
            //});

            //// Get task test
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashTaskDispatcherSmartContract,
            //    Operation = Operations.GetTask,
            //    ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeTask(item)),
            //    Parameters = new[]
            //    {
            //        // Task number
            //        new ContractParameter
            //        {
            //            Type = ContractParameterType.Integer,
            //            Value = taskNumber
            //        }
            //    }
            //});

            #endregion

            #region -= ClientSmartContract =-
            // Add client
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashClientSmartContract,
                Operation = Operations.ClientAdd,
                ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeClient(item)),
                Invoke = true,
                Parameters = new[]
                {
                    // Address
                    new ContractParameter
                    {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes("fe793c3f6202162d071e9cd9948b4d2de4ea4019")
                    }
                }
            });

            //// Get random client
            //operations.Enqueue(new NeoTransaction
            //{
            //    ContractScriptHash = scriptHashClientSmartContract,
            //    Operation = Operations.ClientGetRandom,
            //    ResultToStringHandler = item => JsonConvert.SerializeObject(SerializeClient(item))
            //});

            #endregion

            #region -= AnalysisDispatcherSmartContract =-

            #endregion

            #region -= MainSmartContract =-

            #endregion

            #region -= Old Tests =-

            /*
            // transfer
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = "transfer",
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Neo.Helper.HexToBytes("bb4b3659159122b242e0674131fe0656aff8c356")
                    },
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Neo.Helper.HexToBytes("eed16e9d93fe1e6a1b92125e9f4b3b0994842754")
                    },
                    new ContractParameter()
                    {
                        Type = ContractParameterType.Integer,
                        Value = 12000
                    }
                }
            });

            // check balance of bb4b3659159122b242e0674131fe0656aff8c356
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = "balanceOf",
                ResultToStringHandler = item => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes("bb4b3659159122b242e0674131fe0656aff8c356")
                    }
                }
            });

            // check balance of d05215e1c57d030e522883d9fe8608605f6a0a15
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = "balanceOf",
                ResultToStringHandler = item => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                     new ContractParameter
                     {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes("d05215e1c57d030e522883d9fe8608605f6a0a15")
                     }
                 }
            });
            
            // rewards
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashRewardsSmartContract,
                Operation = "parse",
                ResultToStringHandler = item => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                     new ContractParameter
                     {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes("d05215e1c57d030e522883d9fe8608605f6a0a15")
                     },
                     new ContractParameter
                     {
                         Type = ContractParameterType.Integer,
                         Value = 20000
                     },
                     new ContractParameter
                     {
                         Type = ContractParameterType.Array,
                         Value = new List<ContractParameter>()
                         {
                             new ContractParameter
                             {
                                 Type = ContractParameterType.ByteArray,
                                 Value = Helper.HexToBytes("80c83624072c311d3fbf555e596c040a006614eb")
                             }
                         }
                     }
                 }
            });
            
            */

            #endregion

            return operations;
        }

        private static Client SerializeClient(StackItem item)
        {
            ContractParameter[] args = item.ToParameter().Type == ContractParameterType.Array
                ? (ContractParameter[])item.ToParameter().Value
                : null;
            if (args == null)
            {
                return new Client();
            }

            Client client = new Client
            {
                ScriptHash = args[0].Type == ContractParameterType.ByteArray ? (byte[])args[0].Value : null,
                Ranking = args[1].Type == ContractParameterType.Integer ? (BigInteger)args[1].Value : 0,
                Status = (byte)(args[2].Type == ContractParameterType.Integer ? (BigInteger)args[2].Value : 0),
                StorageIndex = (int)(args[3].Type == ContractParameterType.Integer ? (BigInteger)args[3].Value : 0)
            };
            return client;
        }

        /// <summary>
        ///     Serialize StackItem to Task structure
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Task SerializeTask(StackItem item)
        {
            ContractParameter[] args = item.ToParameter().Type == ContractParameterType.Array
                ? (ContractParameter[])item.ToParameter().Value
                : null;
            if (args == null)
            {
                return new Task();
            }

            Task task = new Task
            {
                Number = args[0].Type == ContractParameterType.Integer ? (BigInteger)args[0].Value : 0
            };
            BigInteger statusBig = args[1].Type == ContractParameterType.Integer ? (BigInteger)args[1].Value : 0;
            task.Status = (byte)statusBig;
            task.Worker = args[2].Type == ContractParameterType.ByteArray ? (byte[])args[2].Value : null;
            task.Source = args[3].Type == ContractParameterType.ByteArray
                ? Encoding.UTF8.GetString((byte[])args[3].Value)
                : null;
            task.Destination = args[4].Type == ContractParameterType.ByteArray
                ? Encoding.UTF8.GetString((byte[])args[4].Value)
                : null;
            task.Payload = args[5].Type == ContractParameterType.ByteArray ? new BigInteger((byte[])args[5].Value) : 0;

            BigInteger verificationNeededBig = args[6].Type == ContractParameterType.Integer ? (BigInteger)args[6].Value : 0;
            task.VerificationNeeded = (int)verificationNeededBig;
            task.Verifications = args[7].Type == ContractParameterType.Array
                ? VerificationArrayParse(args[7])
                : null;
            task.IsSuccess = args[8].Type == ContractParameterType.Boolean && (bool)args[8].Value;

            return task;
        }

        /// <summary>
        ///     Serialize ContractParameter to Verification array
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static Verification[] VerificationArrayParse(ContractParameter arg)
        {
            if (arg.Type != ContractParameterType.Array)
            {
                return null;
            }

            List<Verification> result = new List<Verification>();
            foreach (ContractParameter param in (ContractParameter[])arg.Value)
            {
                if (param.Type != ContractParameterType.Array)
                {
                    result.Add(new Verification());
                }
                else
                {
                    ContractParameter[] verifyParams = (ContractParameter[])param.Value;
                    Verification verifyResult = new Verification
                    {
                        Owner = verifyParams[0].Type == ContractParameterType.ByteArray
                            ? (byte[])verifyParams[0].Value
                            : null
                    };
                    BigInteger resultVerify = verifyParams[1].Type == ContractParameterType.Integer
                        ? (BigInteger)verifyParams[1].Value
                        : 0;
                    verifyResult.Result = resultVerify == 1;

                    byte[] payload = verifyParams[2].Type == ContractParameterType.ByteArray
                        ? (byte[])verifyParams[2].Value
                        : null;
                    verifyResult.Payload = payload == null ? 0 : new BigInteger(payload);

                    result.Add(verifyResult);
                }
            }

            return result.ToArray();
        }
    }
}