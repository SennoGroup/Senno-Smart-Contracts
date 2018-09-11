using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts.SBPSNSC
{
    public class BusinessProcessSocialNetworkContract : SmartContract
    {
        // Parse task swaprate for worker
        public static int ParseWorkerSwaprate() => 1000;
        // Parse task swaprate for verificator
        public static int ParseVerificatorSwaprate() => 3000;
        // Parse task verifications count
        public static int ParseVerificationNeeded() => 0;

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
            if (operation == Operations.NextStepJob)
            {
                return NextStepJob((BigInteger)args[0]);
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

            var parseTask = new Task()
            {
                JobNumber = jobNumber,
                Type = (byte)TaskTypeEnum.Parse,
                Source = parseTaskSource,
                WorkerSwaprate = ParseWorkerSwaprate(),
                VerificatorSwaprate = ParseVerificatorSwaprate(),
                VerificationNeeded = ParseVerificationNeeded()
            };
            var analysisTask = new Task()
            {
                JobNumber = jobNumber,
                Type = (byte)TaskTypeEnum.Analysis,
                WorkerSwaprate = AnalysisWorkerSwaprate(),
                VerificatorSwaprate = AnalysisVerificatorSwaprate(),
                VerificationNeeded = AnalysisVerificationNeeded()
            };


            // call TaskDispatcherSmartContract with method createtask
            // if task created return task number
            BigInteger taskNumber = (BigInteger)TaskDispatcherSmartContract(Operations.CompleteTask, jobNumber,
                parseTask.Source, parseTask.WorkerSwaprate, parseTask.VerificatorSwaprate, parseTask.Type,
                parseTask.VerificationNeeded);
            // if task was created
            if (taskNumber > 0)
            {
                parseTask.Number = taskNumber;
                parseTask.Status = (byte)TaskStatusEnum.Created;

                // create job
                Job newJob = new Job()
                {
                    Number = jobNumber,
                    Type = (byte)JobTypeEnum.ParseJob,
                    Status = (byte)JobStatusEnum.Parsing,
                    Tasks = new[]
                    {
                        parseTask, analysisTask
                    }
                };

                // save job to storage
                Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), newJob.Serialize());

                // event for platform
                JobNotification("create", jobNumber);

                return jobNumber;
            }
            return 0;
        }

        /// <summary>
        /// Call next step of job
        /// </summary>
        private static object NextStepJob(BigInteger jobNumber)
        {
            // get job from storage
            var storageJob = Storage.Get(Storage.CurrentContext, jobNumber.AsByteArray());
            if (storageJob == null || storageJob.Length == 0) return false;

            Job job = (Job)storageJob.Deserialize();
            if (job.Number == 0)
            {
                return false;
            }

            if ((JobStatusEnum)job.Status == JobStatusEnum.Parsing)
            {
                // change job status
                job.Status = (byte)JobStatusEnum.Analysing;

                // get job tasks
                Task parseTask = job.Tasks[0];
                Task analysisTask = job.Tasks[1];
                // finish parse task
                parseTask.Status = (byte)TaskStatusEnum.Finished;
                // set source of analysis task
                analysisTask.Source = parseTask.Destination;

                // call TaskDispatcherSmartContract with method createtask
                BigInteger taskNumber = (BigInteger)TaskDispatcherSmartContract("createtask");
                // if task was created
                if (taskNumber > 0)
                {
                    analysisTask.Number = taskNumber;
                    analysisTask.Status = (byte)TaskStatusEnum.Created;

                    job.Tasks = new[] { parseTask, analysisTask };
                    // save job to storage
                    Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), job.Serialize());

                    // event for platform
                    JobNotification("changestatus", jobNumber);

                    return true;
                }
                return false;
            }
            if ((JobStatusEnum)job.Status == JobStatusEnum.Analysing)
            {
                // change status
                job.Status = (byte)JobStatusEnum.Finished;

                // save to storage
                Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), job.Serialize());

                // event platform
                JobNotification("finishjob", jobNumber);

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
    }
}