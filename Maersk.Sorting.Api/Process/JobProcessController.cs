using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api.Process
{
    public class JobProcessController : IJobProcessController
    {
        private ISortJobProcessor _sortJobProcessor;
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        ConcurrentQueue<SortJob> pendingQueue = new ConcurrentQueue<SortJob>();
        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        Dictionary<Guid, SortJob> allJobs = new Dictionary<Guid, SortJob>();

        public JobProcessController(ISortJobProcessor sortJobProcessor)
        {
            _sortJobProcessor = sortJobProcessor;
            Task.Factory.StartNew(() => Process());
        }

        private async void Process()
        {

            while (true)
            {
                if (pendingQueue.Count <= 0)
                {
                    autoResetEvent.WaitOne();
                }

                SortJob? pendingJob;
                if (pendingQueue.TryDequeue(out pendingJob))
                {
                    if (pendingJob != null)
                    {
                        var completedJob = await _sortJobProcessor.Process(pendingJob);

                        _lock.EnterWriteLock();
                        if (allJobs.ContainsKey(completedJob.Id))
                            allJobs[completedJob.Id] = completedJob;
                        _lock.ExitWriteLock();
                    }
                }
            }

        }


        public void AddJob(SortJob pendingJob)
        {
            _lock.EnterWriteLock();
            allJobs[pendingJob.Id] = pendingJob;
            _lock.ExitWriteLock();
            pendingQueue.Enqueue(pendingJob);
            autoResetEvent.Set();
        }



        public SortJob[] GetAllJobs()
        {
            return allJobs.Values.ToArray();
        }

        public SortJob? GetJobById(Guid Id)
        {
            return allJobs.ContainsKey(Id) ? allJobs[Id] : null;
        }
    }
}
