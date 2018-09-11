﻿using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;

namespace Senno.SmartContracts.STDSC
{
    /// <summary>
    /// Senno Controll Network.
    /// Task Dispatcher Smart Contract.
    /// </summary>
    public class TaskDispatcherSmartContract : SmartContract
    {
        // TODO set RewardsSmartContractScriptHash
        [Appcall("983853e311a9b48777bf6bc40eabe21c31883c46")]
        public static extern object RewardsSmartContract(string method, params object[] args);

        // TODO set ClientsSmartContractScriptHash
        [Appcall("3b2d0d3240bfdea5fabe1833d90271f4c519b974")]
        public static extern object ClientsSmartContract(string method, params object[] args);

        public delegate void TaskNotificationAction<in T, in T1, in T2, in T3>(T p0, T1 p1, T2 p2, T3 p3);

        [DisplayName("tasknotification")]
        public static event TaskNotificationAction<string, object, object, object> TaskNotification;

        public static object Main(string operation, params object[] args)
        {
            // get task by number
            if (operation == Operations.GetTask)
            {
                return GetTask((BigInteger)args[0]);
            }
            // create new task
            if (operation == Operations.CreateTask)
            {
                return CreateTask((BigInteger)args[3], (string)args[0], (int)args[1], (int)args[2], (byte)args[4], (int)args[5]);
            }
            // take a task to perform
            if (operation == Operations.TakeTask)
            {
                return TakeTask((BigInteger)args[0], (byte[])args[1]);
            }
            // complete a task
            if (operation == Operations.CompleteTask)
            {
                return CompleteTask((BigInteger)args[0], (byte[])args[1], (string)args[2], (BigInteger)args[3]);
            }
            // varify a task
            if (operation == Operations.VerifyTask)
            {
                return VerifyTask((BigInteger)args[0], (byte[])args[1], (bool)args[2], (BigInteger)args[3]);
            }

            return false;
        }

        /// <summary>
        /// Get task by number
        /// </summary>
        private static object GetTask(BigInteger taskNumber)
        {
            var task = Storage.Get(Storage.CurrentContext, taskNumber.AsByteArray());
            if (task == null || task.Length == 0) return false;
            return (Task)task.Deserialize();
        }

        /// <summary>
        /// Create new task
        /// </summary>
        private static object CreateTask(BigInteger jobNumber, string source, int workerSwaprate, int verificatorSwaprate, byte taskType, int verificationNeeded)
        {
            BigInteger taskNumber = GetTasksCounter() + 1;

            SetTasksCounter(taskNumber);

            // create new task
            var task = new Task()
            {
                Number = taskNumber,
                Status = (byte)TaskStatusEnum.Created,
                Source = source,
                CandidatesInWorker = new byte[Configuration.TaskCandidatesInWorkerNumber][],
                VerificationNeeded = verificationNeeded,
                Verifications = new Verification[verificationNeeded],
                WorkerSwaprate = workerSwaprate,
                VerificatorSwaprate = verificatorSwaprate,
                JobNumber = jobNumber,
                Type = taskType
            };

            // save task to storage
            Storage.Put(Storage.CurrentContext, taskNumber.AsByteArray(), task.Serialize());

            for (byte notificationIndex = 0; notificationIndex < Configuration.TaskCandidatesInWorkerNumber; notificationIndex++)
            {
                // event for platform
                TaskNotification("create", task.Number, task.JobNumber, task.CandidatesInWorker[notificationIndex]);
            }

            return taskNumber;
        }

        /// <summary>
        /// Take a task to perform
        /// </summary>
        private static object TakeTask(BigInteger taskNumber, byte[] caller)
        {
            // get task from storage
            var storageTask = Storage.Get(Storage.CurrentContext, taskNumber.AsByteArray());
            if (storageTask == null || storageTask.Length == 0) return false;

            Task task = (Task)storageTask.Deserialize();
            if (task.Number == 0)
            {
                return false;
            }

            // if task owner exists
            if (task.Worker != null)
            {
                // event for platform
                TaskNotification("ownerExists", task.JobNumber, task.Number, caller);
                return false;
            }

            bool isCandidate = false;
            for (byte notificationIndex = 0; notificationIndex < Configuration.TaskCandidatesInWorkerNumber; notificationIndex++)
            {
                // event for platform
                if (task.CandidatesInWorker[notificationIndex] == caller)
                {
                    isCandidate = true;
                    break;
                }
            }

            if (!isCandidate)
            {
                // event for platform
                TaskNotification("isNotCandidate", task.JobNumber, task.Number, caller);
                return false;
            }

            // set task owner address
            task.Worker = caller;

            // set task status
            task.Status = (byte)TaskStatusEnum.Working;

            // save task to storage
            Storage.Put(Storage.CurrentContext, taskNumber.AsByteArray(), task.Serialize());

            // event for platform
            TaskNotification("taketask", task.JobNumber, task.Number, task.Worker);

            return true;
        }

        /// <summary>
        /// The completion of the task
        /// </summary>
        private static object CompleteTask(BigInteger taskNumber, byte[] caller, string destination, BigInteger payload)
        {
            // get task from storage
            var storageTask = Storage.Get(Storage.CurrentContext, taskNumber.AsByteArray());
            if (storageTask == null || storageTask.Length == 0) return false;

            Task task = (Task)storageTask.Deserialize();
            // Task not exist or caller is not task owner or tast not in working status
            if (task.Number == 0 || task.Worker != caller || task.Status != (byte)TaskStatusEnum.Working)
            {
                return false;
            }

            // set destination file hash
            task.Destination = destination;
            // set task payload
            task.Payload = payload;

            // if verification is needed
            if (task.VerificationNeeded > 0)
            {
                // set task status
                task.Status = (byte)TaskStatusEnum.Verifying;

                // event for platform
                TaskNotification("verificationneeded", task.JobNumber, task.Number, caller);

            }
            else
            {
                // set the success of a task
                task.IsSuccess = true;

                // finish
                FinishTask(task, null, task.Payload);
            }
            // save task to storage
            Storage.Put(Storage.CurrentContext, taskNumber.AsByteArray(), task.Serialize());

            return true;
        }

        /// <summary>
        /// Verify task execution
        /// </summary>
        private static object VerifyTask(BigInteger taskNumber, byte[] caller, bool result, BigInteger payload)
        {
            // get task from storage
            var storageTask = Storage.Get(Storage.CurrentContext, taskNumber.AsByteArray());
            if (storageTask == null || storageTask.Length == 0) return false;

            Task task = (Task)storageTask.Deserialize();

            // Task not exist or verifiacation don't needed or caller is task owner or task not in verifying status
            if (task.Number == 0 || task.VerificationNeeded == 0 || task.Worker == caller ||
                task.Status != (byte)TaskStatusEnum.Verifying)
            {
                return false;
            }

            // save verify result
            task.Verifications[task.Verifications.Length - task.VerificationNeeded] = new Verification()
            {
                Owner = caller,
                Result = result,
                Payload = payload
            };

            task.VerificationNeeded = task.VerificationNeeded - 1;

            // if the number of verify results is complete
            if (task.VerificationNeeded == 0)
            {
                byte[][] verificators = new byte[task.Verifications.Length][];
                // calculating the success of a task
                int successResults = 0;
                BigInteger sumPayload = task.Payload;
                for (int i = 0; i < task.Verifications.Length; i++)
                {
                    var verification = task.Verifications[i];
                    verificators[i] = verification.Owner;
                    if (verification.Result)
                    {
                        successResults++;
                        sumPayload += verification.Payload;
                    }
                }
                BigInteger avgPayload = 0;
                if (successResults > 0)
                {
                    avgPayload = sumPayload / successResults + 1;
                }
                // set the success of a task
                task.IsSuccess = successResults >= task.Verifications.Length / 2;
                // finish
                FinishTask(task, verificators, avgPayload);
            }
            else
            {
                // save task to storage
                Storage.Put(Storage.CurrentContext, taskNumber.AsByteArray(), task.Serialize());

                // event for platform
                TaskNotification("verifyTask", task.JobNumber, task.Number, caller);
            }
            return true;
        }

        /// <summary>
        /// Task finished operation
        /// </summary>
        /// <param name="task"></param>
        /// <param name="verificators"></param>
        /// <param name="payload"></param>
        private static void FinishTask(Task task, byte[][] verificators, BigInteger payload)
        {
            if (task.Status != (byte)TaskStatusEnum.Finished)
            {
                // set task status
                task.Status = (byte)TaskStatusEnum.Finished;

                // save task to storage
                Storage.Put(Storage.CurrentContext, task.Number.AsByteArray(), task.Serialize());
                if (task.IsSuccess)
                {
                    // rewards
                    RewardsSmartContract(Operations.Reward, task.Worker, payload, verificators, task.WorkerSwaprate, task.VerificatorSwaprate, task.Type);
                }

                // event for platform
                TaskNotification("finishtask", task.JobNumber, task.Number, task.Worker);
            }
        }

        /// <summary>
        /// Returns the total number of tasks
        /// </summary>
        private static BigInteger GetTasksCounter()
        {
            byte[] counter = Storage.Get(Storage.CurrentContext, "tasksCounter");

            if (counter == null || counter.Length == 0)
            {
                return 0;
            }
            return counter.AsBigInteger();
        }

        /// <summary>
        /// Sets the total number of tasks
        /// </summary>
        /// <param name="tasksCounter">Total number of tasks</param>
        private static void SetTasksCounter(BigInteger tasksCounter)
        {
            Storage.Put(Storage.CurrentContext, "tasksCounter", tasksCounter);
        }
    }
}