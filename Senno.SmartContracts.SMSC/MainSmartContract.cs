using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Senno.SmartContracts.Common;
using System.ComponentModel;

namespace Senno.SmartContracts.SMSC
{

    public class MainSmartContract : SmartContract
    {
        // TODO set ParseDispatcherSmartContractScriptHash
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
        public static extern object ParseDispatcherSmartContract(string method, params object[] args);

        // TODO set AnalysisDispatcherSmartContractScriptHash
        [Appcall("d31b0b6440ecebe0861f4683831c04a0cd497943")]
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
            // TODO generate new unique job number
            string jobNumber = "first";

            // create job
            Job newJob = new Job()
            {
                Number = jobNumber,
                Type = (byte)JobTypeEnum.ParseJob,
                Status = (byte)JobStatusEnum.Parsing
            };

            // save job to storage with prefix
            StorageMap job_sm = Storage.CurrentContext.CreateMap("job");
            job_sm.Put(jobNumber.AsByteArray(), newJob.Serialize());

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
            string jobNumber = (string)args[0];

            // get job from storage
            StorageMap job_sm = Storage.CurrentContext.CreateMap("job");
            Job job = (Job)job_sm.Get(jobNumber).Deserialize();
            if (job.Number == null || job.Number.Length == 0)
            {
                return false;
            }

            if ((JobStatusEnum)job.Status == JobStatusEnum.Parsing)
            {
                // change status
                job.Status = (byte)JobStatusEnum.Analysing;

                // save to storage
                job_sm.Put(jobNumber.AsByteArray(), job.Serialize());

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
                job_sm.Put(jobNumber.AsByteArray(), job.Serialize());

                // event platform
                JobNotification("finishjob", jobNumber);

                return true;
            }

            return false;
        }
    }
}
