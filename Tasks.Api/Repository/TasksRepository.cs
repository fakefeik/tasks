using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Api.DataTypes;
using TaskStatus = Tasks.Api.DataTypes.TaskStatus;

namespace Tasks.Api.Repository
{
    public class TasksRepository
    {
        private static readonly SemaphoreSlim DbSemaphore = new(900);

        public async Task<Guid> AddTask()
        {
            var taskId = Guid.NewGuid();

            await DbSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                await using var context = new AppDbContext();
                context.Tasks.Add(new TaskObject
                {
                    Id = taskId,
                    Status = TaskStatus.Created,
                    Timestamp = DateTimeOffset.UtcNow,
                });
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            finally
            {
                DbSemaphore.Release();
            }

            return taskId;
        }

        public async Task UpdateTask(Guid taskId, TaskStatus status)
        {
            await DbSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                await using var context = new AppDbContext();
                context.Tasks.Update(new TaskObject
                {
                    Id = taskId,
                    Status = status,
                    Timestamp = DateTimeOffset.UtcNow,
                });
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            finally
            {
                DbSemaphore.Release();
            }
        }

        public async Task<TaskObject> GetTask(Guid taskId)
        {
            await DbSemaphore.WaitAsync().ConfigureAwait(false);

            TaskObject task;
            try
            {
                await using var context = new AppDbContext();
                task = await context.Tasks.FindAsync(taskId).ConfigureAwait(false);
            }
            finally
            {
                DbSemaphore.Release();
            }

            return task;
        }
    }
}