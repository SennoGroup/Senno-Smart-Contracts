using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts.SADSC
{
    public class AnalysisDispatcherSmartContract : SmartContract
    {
        /// <summary>
        /// Initial Verification needed
        /// </summary>
        public static ushort InitialVerificationNeeded = 2;

        /// <summary>
        /// Name of the RewardsSmartContractScriptHash operation
        /// </summary>
        public static string RewardsOperationName = "analysis";

        // TODO set RewardsSmartContractScriptHash
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object RewardsSmartContract(string method, params object[] args);

        public delegate void TaskNotificationAction<in T, in T1>(T p0, T1 p1);

        [DisplayName("tasknotification")]
        public static event TaskNotificationAction<string, object> TaskNotification;

        public static object Main(string operation, params object[] args)
        {
            // get task by number
            if (operation == "gettask")
            {
                return GetTask(args);
            }
            // create new task
            if (operation == "createtask")
            {
                return CreateTask(args);
            }
            // take a task to perform
            if (operation == "taketask")
            {
                return TakeTask(args);
            }
            // complete a task
            if (operation == "completetask")
            {
                return CompleteTask(args);
            }
            // varify a task
            if (operation == "verifytask")
            {
                return VerifyTask(args);
            }

            return false;
        }

        /// <summary>
        /// Get task by number
        /// </summary>
        private static object GetTask(params object[] args)
        {
            // get task number from arguments
            string taskNumber = (string)args[0];

            // get task from storage
            StorageMap task_sm = Storage.CurrentContext.CreateMap("task");

            return task_sm.Get(taskNumber).Deserialize();
        }

        /// <summary>
        /// Create new task
        /// </summary>
        private static object CreateTask(params object[] args)
        {
            BigInteger taskNumber = GetTasksCounter() + 1;

            SetTasksCounter(taskNumber);

            // get source file hash from arguments
            string source = (string)args[0];

            // create new task
            var newTask = new Task()
            {
                Number = taskNumber,
                Status = (byte)TaskStatusEnum.Created,
                Source = source,
                VerificationNeeded = InitialVerificationNeeded,
                Verifications = new Verification[InitialVerificationNeeded]
            };

            // save task to storage
            StorageMap task_sm = Storage.CurrentContext.CreateMap("task");
            task_sm.Put(taskNumber.Serialize(), newTask.Serialize());

            // event for platform
            TaskNotification("create", taskNumber);

            return true;
        }

        /// <summary>
        /// Take a task to perform
        /// </summary>
        private static object TakeTask(params object[] args)
        {
            // get task number from arguments
            BigInteger taskNumber = (BigInteger)args[0];
            // get caller address from arguments
            byte[] caller = (byte[])args[1];

            // get task from storage
            StorageMap task_sm = Storage.CurrentContext.CreateMap("task");
            Task task = (Task)task_sm.Get(taskNumber.ToByteArray()).Deserialize();
            if (task.Number == 0)
            {
                return false;
            }

            // if task owner exists
            if (task.Owner != null)
            {
                // event for platform
                TaskNotification("ownerExists", task.Number);

                return false;
            }

            // set task owner address
            task.Owner = caller;

            // set task status
            task.Status = (byte)TaskStatusEnum.Working;

            // save task to storage
            task_sm.Put(taskNumber.AsByteArray(), task.Serialize());

            // event for platform
            TaskNotification("taketask", task.Number);

            return true;
        }

        /// <summary>
        /// The completion of the task
        /// </summary>
        private static object CompleteTask(params object[] args)
        {
            // get task number from arguments
            BigInteger taskNumber = (BigInteger)args[0];
            // get caller address from arguments
            byte[] caller = (byte[])args[1];
            // get destination file hash from arguments
            string destination = (string)args[2];

            // get task from storage
            StorageMap task_sm = Storage.CurrentContext.CreateMap("task");
            Task task = (Task)task_sm.Get(taskNumber.AsByteArray()).Deserialize();

            // Task not exist or caller is not task owner or tast not in working status
            if (task.Number == 0 || task.Owner != caller || task.Status != (byte)TaskStatusEnum.Working)
            {
                return false;
            }

            // set destination file hash
            task.Destination = destination;

            // if verification is needed
            if (task.VerificationNeeded > 0)
            {
                // set task status
                task.Status = (byte)TaskStatusEnum.Verifying;

                // event for platform
                TaskNotification("verificationneeded", task.Number);

                // save task to storage
                task_sm.Put(taskNumber.AsByteArray(), task.Serialize());
            }
            else
            {
                // set the success of a task
                task.IsSuccess = true;

                // finish
                FinishTask(task, null);

            }
            return true;
        }

        /// <summary>
        /// Verify task execution
        /// </summary>
        private static object VerifyTask(params object[] args)
        {
            // get task number from arguments
            string taskNumber = (string)args[0];
            // get caller address from arguments
            byte[] caller = (byte[])args[1];
            // get verify result from arguments
            bool result = (bool)args[2];

            // get task from storage
            StorageMap task_sm = Storage.CurrentContext.CreateMap("task");
            Task task = (Task)task_sm.Get(taskNumber).Deserialize();

            // Task not exist or verifiacation don't needed or caller is task owner or tast not in verifying status
            if (task.Number == 0 || task.VerificationNeeded == 0 || task.Owner == caller || task.Status != (byte)TaskStatusEnum.Verifying)
            {
                return false;
            }

            // save verify result
            task.Verifications[task.Verifications.Length - task.VerificationNeeded] = new Verification()
            {
                Owner = caller,
                Result = result
            };

            task.VerificationNeeded = task.VerificationNeeded - 1;

            // if the number of verify results is complete
            if (task.VerificationNeeded == 0)
            {
                byte[][] verificators = new byte[task.Verifications.Length][];
                // calculating the success of a task
                int successResults = 0;
                for (int i = 0; i < task.Verifications.Length; i++)
                {
                    var verification = task.Verifications[i];
                    verificators[i] = task.Owner;
                    if (verification.Result)
                    {
                        successResults = successResults + 1;
                    }
                }

                // set the success of a task
                task.IsSuccess = successResults > task.Verifications.Length;

                // finish
                FinishTask(task, verificators);
            }

            return true;
        }

        /// <summary>
        /// Task finished operation
        /// </summary>
        /// <param name="task"></param>
        private static void FinishTask(Task task, byte[][] verificators)
        {
            if (task.Status != (byte)TaskStatusEnum.Finished)
            {

                // set task status
                task.Status = (byte)TaskStatusEnum.Finished;

                // get task from storage
                StorageMap task_sm = Storage.CurrentContext.CreateMap("task");

                // save task to storage
                task_sm.Put(task.Number.AsByteArray(), task.Serialize());

                if (task.IsSuccess)
                {
                    // TODO calculate payload
                    BigInteger payload = 0;
                    // rewards
                    RewardsSmartContract(RewardsOperationName, task.Owner, payload, verificators);
                }

                // event for platform
                TaskNotification("finishtask", task.Number);
            }
        }

        /// <summary>
        /// Returns the total number of tasks
        /// </summary>
        private static BigInteger GetTasksCounter()
        {
            return Storage.Get(Storage.CurrentContext, "tasksCounter").AsBigInteger();
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
