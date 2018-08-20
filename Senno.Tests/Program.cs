using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Neo.Core;
using Neo.Manager;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Newtonsoft.Json;
using Senno.SmartContracts.Common;
using Helper = Neo.Helper;

namespace Senno.Tests
{
    class Program
    {
        private static NeoManager chainManager;

        private static readonly string walletPath = @"sw1.json";
        private static readonly string walletPassword = "1";

        private static readonly string scriptHashTokenSmartContract = "0x849f7fd094ac87bd3a797d2fdaddcd927050d686";
        private static readonly string scriptHashRewardsSmartContract = "0x983853e311a9b48777bf6bc40eabe21c31883c46";

        private static readonly string scriptHashParseDispatcherSmartContract = "0x357789484ecbf4d504a239ebbd06e3af4edfad3e";


        private static readonly string scriptHashAccount1 = "d05215e1c57d030e522883d9fe8608605f6a0a15";
        private static readonly string scriptHashAccount2 = "80c83624072c311d3fbf555e596c040a006614eb";
        private static readonly string scriptHashAccount3 = "b45f0323f5cfb1da6787ab460fac1d0bb8218cfe";

            

        //private static readonly string hexAcc1 = "7133992d36131c277e0472aa7c71c5eb39df91756791b1d28305a0208b437357";
        //private static readonly string hexAcc2 = "c9a44e94cc740d92e35a7037502a3faeb4f8c3ab9509aeae9caa3e0c2bf1162d";
        //private static readonly string hexAcc3 = "9369b840803bd3f195fd78b765d27a7d13fea5fbb91f8a6580f79246c769e8ad";


        static void Main(string[] args)
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
                    Wallet wallet = chainManager.OpenWallet(walletPath, walletPassword);

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
            var operations = new Queue<NeoTransaction>();

            #region -= TokenSmartContract =-
/*
            // deploy 
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.Deploy,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString()
            });

            // owner 
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.Owner,
                ResultToStringHandler = (StackItem item) => Neo.Helper.ToHexString(item.GetByteArray())
            });

            // check balance of owner
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Neo.Helper.HexToBytes(Configuration.TokenSmartContractOwner)
                    }
                }
            });
            */
            #endregion

            #region -= RewardsSmartContract =-

            /*
            // check balance of scriptHashAccount1
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                     new ContractParameter
                     {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes(scriptHashAccount1)
                     }
                 }
            });

            // check balance of scriptHashAccount2
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount2)
                    }
                }
            });

            // rewards
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashRewardsSmartContract,
                Operation = Operations.Parse,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString(),
                Parameters = new[]
                {
                     new ContractParameter
                     {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes(scriptHashAccount1)
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
                                 Value = Helper.HexToBytes(scriptHashAccount2)
                             }
                         }
                     }
                 }
            });

            // check balance of scriptHashAccount1
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                     new ContractParameter
                     {
                         Type = ContractParameterType.ByteArray,
                         Value = Helper.HexToBytes(scriptHashAccount1)
                     }
                 }
            });

            // check balance of scriptHashAccount2
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount2)
                    }
                }
            });
            */

            #endregion


            #region -= ParseDispatcherSmartContract =-

            // check balance of task owner
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount1)
                    }
                }
            });
            // check balance of verificator 1
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount2)
                    }
                }
            });
            // check balance of verificator 2
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount3)
                    }
                }
            });
            // create task
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.CreateTask,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.String,
                        Value = "test"
                    }
                }
            });
            // get task
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.GetTask,
                ResultToStringHandler = (StackItem item) => JsonConvert.SerializeObject(SerializeTask(item)),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    }
                }
            });
            // take task to execution
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.TakeTask,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount1)
                    }
                }
            });
            // get task (Owner must exist, Status = 2)
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.GetTask,
                ResultToStringHandler = (StackItem item) => JsonConvert.SerializeObject(SerializeTask(item)),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    }
                }
            });
            // complete work on task by taskOwner
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.CompleteTask,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount1)
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.String,
                        Value = "dest"
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 100000
                    },
                }
            });
            // get task (Destination must exist, Status = 3, )
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.GetTask,
                ResultToStringHandler = (StackItem item) => JsonConvert.SerializeObject(SerializeTask(item)),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    }
                }
            });
            // verify task by first verificator
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.VerifyTask,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount2)
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.Boolean,
                        Value = true
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 33000
                    },
                }
            });
            // get task (VerificationNeeded 1, Verificators[0] - exist)
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.GetTask,
                ResultToStringHandler = (StackItem item) => JsonConvert.SerializeObject(SerializeTask(item)),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    }
                }
            });
            // verify task by second verificator
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.VerifyTask,
                Invoke = true,
                ResultToStringHandler = (StackItem item) => item.GetBoolean().ToString(),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount3)
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.Boolean,
                        Value = true
                    },
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 34000
                    },
                }
            });
            // get task (VerificationNeeded 0, Verificators[1] - exist)
            operations.Enqueue(new NeoTransaction
            {
                ContractScriptHash = scriptHashParseDispatcherSmartContract,
                Operation = Operations.GetTask,
                ResultToStringHandler = (StackItem item) => JsonConvert.SerializeObject(SerializeTask(item)),
                Parameters = new[]
                {
                    new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = 1
                    }
                }
            });
            // check balance of task owner
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount1)
                    }
                }
            });
            // check balance of verificator 1
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount2)
                    }
                }
            });
            // check balance of verificator 2
            operations.Enqueue(new NeoTransaction()
            {
                ContractScriptHash = scriptHashTokenSmartContract,
                Operation = Operations.BalanceOf,
                ResultToStringHandler = (StackItem item) => item.GetBigInteger().ToString(),
                Parameters = new[]
                {
                    new ContractParameter()
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = Helper.HexToBytes(scriptHashAccount3)
                    }
                }
            });
            
            #endregion


            #region -= AnalysisDispatcherSmartContract =-

            #endregion


            #region -= MainSmartContract =-

            #endregion

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

            return operations;
        }

        /// <summary>
        /// Serialize StackItem to Task structure
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Task SerializeTask(StackItem item)
        {
            var args = item.ToParameter().Type == ContractParameterType.Array
                ? (ContractParameter[]) item.ToParameter().Value
                : null;
            if (args == null) return new Task();
            Task task = new Task();

            task.Number = args[0].Type == ContractParameterType.Integer ? (BigInteger) args[0].Value : 0;
            var statusBig = args[1].Type == ContractParameterType.Integer ? (BigInteger) args[1].Value : 0;
            task.Status = (byte) statusBig;
            task.Owner = args[2].Type == ContractParameterType.ByteArray ? (byte[]) args[2].Value : null;
            task.Source = args[3].Type == ContractParameterType.ByteArray
                ? Encoding.UTF8.GetString((byte[]) args[3].Value)
                : null;
            task.Destination = args[4].Type == ContractParameterType.ByteArray
                ? Encoding.UTF8.GetString((byte[]) args[4].Value)
                : null;
            task.Payload = args[5].Type == ContractParameterType.ByteArray ? new BigInteger((byte[])args[5].Value) : 0;

            var verificationNeededBig = args[6].Type == ContractParameterType.Integer ? (BigInteger) args[6].Value : 0;
            task.VerificationNeeded = (int) verificationNeededBig;
            task.Verifications = args[7].Type == ContractParameterType.Array
                ? (Verification[]) VerificationArrayParse(args[7])
                : null;
            task.IsSuccess = args[8].Type == ContractParameterType.Boolean && (bool) args[8].Value;


            return task;
        }

        /// <summary>
        /// Serialize ContractParameter to Verification array
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static Verification[] VerificationArrayParse(ContractParameter arg)
        {
            if (arg.Type != ContractParameterType.Array) return null;
            List<Verification> result = new List<Verification>();
            foreach (var param in (ContractParameter[]) arg.Value)
            {
                if (param.Type != ContractParameterType.Array) result.Add(new Verification());
                else
                {
                    var verifyParams = (ContractParameter[]) param.Value;
                    var verifyResult = new Verification();

                    verifyResult.Owner = verifyParams[0].Type == ContractParameterType.ByteArray
                        ? (byte[]) verifyParams[0].Value
                        : null;
                    var resultVerify = verifyParams[1].Type == ContractParameterType.Integer
                        ? (BigInteger) verifyParams[1].Value
                        : 0;
                    verifyResult.Result = resultVerify == 1;

                    var payload = verifyParams[2].Type == ContractParameterType.ByteArray
                        ? (byte[]) verifyParams[2].Value
                        : null;
                    verifyResult.Payload = payload == null ? 0 : new BigInteger(payload);

                    result.Add(verifyResult);
                }
            }

            return result.ToArray();
        }
    }
}