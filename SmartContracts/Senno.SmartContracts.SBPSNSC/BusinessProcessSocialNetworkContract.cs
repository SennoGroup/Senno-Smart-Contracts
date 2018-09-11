using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts.SBPSNSC
{
    public class BusinessProcessSocialNetworkContract : SmartContract
    {
        // task type for parse
        public static byte ParseTaskType() => 101;
        // Parse task swaprate for worker
        public static int ParseWorkerSwaprate() => 1000;
        // Parse task swaprate for verificator
        public static int ParseVerificatorSwaprate() => 3000;
        // Parse task verifications count
        public static int ParseVerificationNeeded() => 0;

        // task type for analysis
        public static byte AnalysisTaskType() => 102;
        // Analysis task swaprate for worker
        public static int AnalysisWorkerSwaprate() => 500;
        // Analysis task swaprate for verificator
        public static int AnalysisVerificatorSwaprate() => 2000;
        // Analysis task verifications count
        public static int AnalysisVerificationNeeded() => 2;


        // TODO set TaskDispatcherSmartContractScriptHash
        [Appcall("2d494be69c23b2393550a0f16a05f2b3484122d2")]
        public static extern object TaskDispatcherSmartContract(string method, params object[] args);

        public delegate void JobNotificationAction<in T, in T1>(T p0, T1 p1);

        [DisplayName("jobnotification")]
        public static event JobNotificationAction<string, object> JobNotification;

        public static object Main(string operation, params object[] args)
        {
            // create new job
            if (operation == Operations.CreateJob)
            {
                return CreateJob((string)args[0]);
            }
            // call next step of job
            if (operation == Operations.CompleteTaskJob)
            {
                return CompleteTask((BigInteger)args[0]);
            }

            return false;
        }

        /// <summary>
        /// Create new job
        /// </summary>
        private static object CreateJob(string parseTaskSource)
        {
            // Generate new unique job number
            BigInteger jobNumber = GetJobsCounter() + 1;
            SetJobsCounter(jobNumber);
            // create tasks for job
            var parseTask = new Task()
            {
                JobNumber = jobNumber,
                Type = ParseTaskType(),
                Source = parseTaskSource,
                WorkerSwaprate = ParseWorkerSwaprate(),
                VerificatorSwaprate = ParseVerificatorSwaprate(),
                VerificationNeeded = ParseVerificationNeeded(),
            };
            var analysisTask = new Task()
            {
                JobNumber = jobNumber,
                Type = AnalysisTaskType(),
                WorkerSwaprate = AnalysisWorkerSwaprate(),
                VerificatorSwaprate = AnalysisVerificatorSwaprate(),
                VerificationNeeded = AnalysisVerificationNeeded()
            };

            // create job
            Job newJob = new Job()
            {
                Number = jobNumber,
                Status = (byte)JobStatusEnum.Created,
                CurrentTaskIndex = 0,
                Tasks = new[]
                {
                    parseTask, analysisTask
                }
            };

            // save job to storage
            Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), newJob.Serialize());

            // event for platform
            JobNotification("create", jobNumber);

            // if create task is success
            if (NextStepJob(newJob))
            {
                return jobNumber;
            }

            return 0;
        }

        /// <summary>
        /// Compleate current executing task by job number
        /// </summary>
        /// <param name="jobNumber"></param>
        /// <returns></returns>
        private static object CompleteTask(BigInteger jobNumber)
        {
            var storageJob = Storage.Get(Storage.CurrentContext, jobNumber.AsByteArray());
            if (storageJob == null || storageJob.Length == 0) return false;
            var job = (Job)storageJob.Deserialize();

            return NextStepJob(job);
        }

        /// <summary>
        /// Call next step of job
        /// </summary>
        private static bool NextStepJob(Job job)
        {
            if ((JobStatusEnum)job.Status == JobStatusEnum.Created)
            {
                if (job.CurrentTaskIndex >= 0 && job.Tasks.Length > job.CurrentTaskIndex)
                {
                    // get current executing task
                    var currentTask = job.Tasks[job.CurrentTaskIndex];

                    // if task was created
                    if (currentTask.Status == (byte)TaskStatusEnum.Created)
                    {
                        // get task from task dispatcher SC
                        currentTask = (Task)TaskDispatcherSmartContract(Operations.GetTask, currentTask.Number);

                        // save task from tdsc to job
                        // job.Tasks[job.CurrentTaskIndex] = currentTask;

                        job.CurrentTaskIndex++;

                        // if next task doesn't exist finish job
                        if (job.CurrentTaskIndex == job.Tasks.Length)
                        {
                            return FinishJob(job);
                        }

                        var nextTask = job.Tasks[job.CurrentTaskIndex];
                        
                        // Destination of current task it's source of next task
                        nextTask.Source = currentTask.Destination;
                    }
                    return CreateAndStoreTask(job);
                }
            }

            return false;
        }

        /// <summary>
        /// Create task in Task Dispatcher Smart Contract
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static bool CreateAndStoreTask(Job job)
        {
            var task = job.Tasks[job.CurrentTaskIndex];

            // call TaskDispatcherSmartContract with method createtask
            // if task created return task number
            BigInteger taskNumber = (BigInteger)TaskDispatcherSmartContract(Operations.CreateTask,
                job.Number,
                task.Source,
                task.WorkerSwaprate,
                task.VerificatorSwaprate,
                task.Type,
                task.VerificationNeeded);
            // if task was created
            if (taskNumber > 0)
            {
                task.Number = taskNumber;
                task.Status = (byte)TaskStatusEnum.Created;

                //save job to storage
                Storage.Put(Storage.CurrentContext, job.Number.AsByteArray(), job.Serialize());

                return true;
            }
            return false;

        }

        /// <summary>
        /// Returns the total number of jobs
        /// </summary>
        private static BigInteger GetJobsCounter()
        {
            byte[] jobsCount = Storage.Get(Storage.CurrentContext, "jobsCounter");
            if (jobsCount == null || jobsCount.Length == 0) return 0;
            return jobsCount.AsBigInteger();
        }

        /// <summary>
        /// Sets the total number of jobs
        /// </summary>
        /// <param name="jobsCounter">Total number of jobs</param>
        private static void SetJobsCounter(BigInteger jobsCounter)
        {
            Storage.Put(Storage.CurrentContext, "jobsCounter", jobsCounter);
        }

        /// <summary>
        /// Finish job if all tasks done
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static bool FinishJob(Job job)
        {

            // change status
            job.Status = (byte)JobStatusEnum.Finished;

            // save to storage
            Storage.Put(Storage.CurrentContext, job.Number.AsByteArray(), job.Serialize());

            // event platform
            JobNotification("finishjob", job.Number);

            return true;
        }

    }
}