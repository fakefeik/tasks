using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Tasks.Api.DataTypes;
using Tasks.Api.Repository;
using TaskStatus = Tasks.Api.DataTypes.TaskStatus;

namespace Tasks.Api.Controllers
{
    [Route("task")]
    public class TasksApiController : ControllerBase
    {
        private readonly TasksRepository _tasksRepository = new();

        [Route("{id}")]
        public async Task<IActionResult> GetTask(string id)
        {
            if (!Guid.TryParse(id, out var taskId))
                return BadRequest();

            var task = await _tasksRepository.GetTask(taskId).ConfigureAwait(false);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [Route("")]
        public async Task<IActionResult> PostTask()
        {
            var taskId = await _tasksRepository.AddTask().ConfigureAwait(false);

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
            await _tasksRepository.UpdateTask(taskId, TaskStatus.Running).ConfigureAwait(false);
            await Task.Delay(TaskConstants.TaskExecutionTime).ConfigureAwait(false);
            await _tasksRepository.UpdateTask(taskId, TaskStatus.Finished).ConfigureAwait(false);
        }
    }
}