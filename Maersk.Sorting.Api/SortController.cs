using Maersk.Sorting.Api.Process;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api.Controllers
{
    [ApiController]
    [Route("sort")]
    public class SortController : ControllerBase
    {
        private readonly ISortJobProcessor _sortJobProcessor;
        private readonly IJobProcessController _jobProcessController;

        public SortController(ISortJobProcessor sortJobProcessor, IJobProcessController jobProcessController)
        {
            _sortJobProcessor = sortJobProcessor;
            _jobProcessController = jobProcessController;
        }

        [HttpPost("run")]
        [Obsolete("This executes the sort job asynchronously. Use the asynchronous 'EnqueueJob' instead.")]
        public async Task<ActionResult<SortJob>> EnqueueAndRunJob(int[] values)
        {
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);

            var completedJob = await _sortJobProcessor.Process(pendingJob);

            return Ok(completedJob);
        }

        [Route("EnqueueJob")]
        [HttpPost]
        public ActionResult<SortJob> EnqueueJob(int[] values)
        {
            var pendingJob = new SortJob(
              id: Guid.NewGuid(),
              status: SortJobStatus.Pending,
              duration: null,
              input: values,
              output: null);

            _jobProcessController.AddJob(pendingJob);

            return Ok(pendingJob);
        }

        [HttpGet]
        [Route("GetJobs")]
        public ActionResult<SortJob[]> GetJobs()
        {
            return Ok(_jobProcessController.GetAllJobs());
        }

        [HttpGet("{jobId}")]
        [Route("GetJob/{jobId}")]
        public ActionResult<SortJob> GetJob(Guid jobId)
        {
            var result = _jobProcessController.GetJobById(jobId);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }
    }
}
