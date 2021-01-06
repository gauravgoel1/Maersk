using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api.Process
{
    public interface IJobProcessController
    {
        void AddJob(SortJob pendingJob);
        SortJob[] GetAllJobs();
        SortJob? GetJobById(Guid Id);
    }
}
