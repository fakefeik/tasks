using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Tasks.Api.DataTypes;
using TaskStatus = Tasks.Api.DataTypes.TaskStatus;

namespace Tasks.Api.Controllers
{
    [Route("in-memory-task")]
    public class InMemoryTasksApiController : ControllerBase
    {
        private static readonly ConcurrentDictionary<Guid, TaskObject> Tasks = new();

        [Route("{id}")]
        public IActionResult GetTask(string id)
        {
            if (!Guid.TryParse(id, out var taskId))
                return BadRequest();

            if (!Tasks.TryGetValue(taskId, out var task))
                return NotFound();

            return Ok(task);
        }

        [Route("")]
        public IActionResult PostTask()
        {
            var taskId = Guid.NewGuid();
            Tasks[taskId] = new TaskObject
            {
                Id = taskId,
                Status = TaskStatus.Created,
                Timestamp = DateTimeOffset.UtcNow,
            };

            Task.Run(() => ProcessTask(taskId));

            return new AcceptedResult
            {
                StatusCode = 202,
                Value = taskId.ToString(),
                ContentTypes = new MediaTypeCollection {"text/plain"}
            };
        }

        private async Task ProcessTask(Guid taskId)
        {
            Tasks[taskId].Status = TaskStatus.Running;
            Tasks[taskId].Timestamp = DateTimeOffset.UtcNow;

            // Thread.Sleep(TaskConstants.TaskExecutionTime);
            await Task.Delay(TaskConstants.TaskExecutionTime).ConfigureAwait(false);

            Tasks[taskId].Status = TaskStatus.Finished;
            Tasks[taskId].Timestamp = DateTimeOffset.UtcNow;
        }
    }
}