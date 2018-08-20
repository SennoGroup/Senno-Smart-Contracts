using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;
using System.Numerics;

namespace Senno.SmartContracts.SMSC
{

    public class MainSmartContract : SmartContract
    {
        // TODO set ParseDispatcherSmartContractScriptHash
        [Appcall("2d494be69c23b2393550a0f16a05f2b3484122d2")]
        public static extern object ParseDispatcherSmartContract(string method, params object[] args);

        // TODO set AnalysisDispatcherSmartContractScriptHash
        [Appcall("2d494be69c23b2393550a0f16a05f2b3484122d2")]
        public static extern object AnalysisDispatcherSmartContract(string method, params object[] args);

        public delegate void JobNotificationAction<in T, in T1>(T p0, T1 p1);

        [DisplayName("jobnotification")]
        public static event JobNotificationAction<string, object> JobNotification;

        public static object Main(string operation, params object[] args)
        {
            // create new job
            if (operation == "createjob")
            {
                return CreateJob(args);
            }
            // call next step of job
            if (operation == "nextstepjob")
            {
                return NextStepJob(args);
            }

            return false;
        }

        /// <summary>
        /// Create new job
        /// </summary>
        private static object CreateJob(params object[] args)
        {
            // Generate new unique job number
            BigInteger jobNumber = GetJobsCounter() + 1;
            SetJobsCounter(jobNumber);

            // create job
            Job newJob = new Job()
            {
                Number = jobNumber,
                Type = (byte)JobTypeEnum.ParseJob,
                Status = (byte)JobStatusEnum.Parsing
            };

            // save job to storage with prefix
            Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), newJob.Serialize());

            // event for platform
            JobNotification("create", jobNumber);

            // call ParseDispatcherSmartContract with method createtask
            return ParseDispatcherSmartContract("createtask", args);
        }

        /// <summary>
        /// Call next step of job
        /// </summary>
        private static object NextStepJob(params object[] args)
        {
            // get job number from arguments
            BigInteger jobNumber = (BigInteger)args[0];

            // get job from storage
            // get task from storage
            var storageJob = Storage.Get(Storage.CurrentContext, jobNumber.AsByteArray());
            if (storageJob == null || storageJob.Length == 0) return false;

            Job job = (Job)storageJob.Deserialize();
            if (job.Number == 0)
            {
                return false;
            }

            if ((JobStatusEnum)job.Status == JobStatusEnum.Parsing)
            {
                // change status
                job.Status = (byte)JobStatusEnum.Analysing;

                // save to storage
                Storage.Put(Storage.CurrentContext, jobNumber.AsByteArray(), job.Serialize());

                // event platform
                JobNotification("changestatus", jobNumber);

                // call AnalysisDispatcherSmartContract with method createtask
                return AnalysisDispatcherSmartContract("createtask", args);
            }
            else if ((JobStatusEnum)job.Status == JobStatusEnum.Analysing)
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
