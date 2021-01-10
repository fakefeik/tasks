using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Tasks.Api.DataTypes;
using TaskStatus = Tasks.Api.DataTypes.TaskStatus;

namespace Tasks.Tests
{
    public class TaskTests
    {
        [Test]
        public async Task TestNotGuidTaskId()
        {
            using var client = new TasksApiClient();

            var (badRequestCode, badRequestTask) = await client.GetTask("123").ConfigureAwait(false);
            Assert.That(badRequestCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(badRequestTask, Is.Null);
        }

        [Test]
        public async Task TestNotFoundTask()
        {
            using var client = new TasksApiClient();

            var (notFoundCode, notFoundTask) = await client.GetTask(Guid.NewGuid().ToString()).ConfigureAwait(false);
            Assert.That(notFoundCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(notFoundTask, Is.Null);
        }

        [Test]
        public async Task TestFinishSingleTask()
        {
            using var client = new TasksApiClient();

            var (code, taskId) = await client.PostTask().ConfigureAwait(false);
            Assert.That(code, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(taskId, Is.Not.Null);

            await Task.Delay(100).ConfigureAwait(false);

            var (runningCode, runningTask) = await client.GetTask(taskId.ToString()).ConfigureAwait(false);
            Assert.That(runningCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(runningTask, Is.Not.Null);
            Assert.That(runningTask.Id, Is.EqualTo(taskId));
            Assert.That(runningTask.Status, Is.EqualTo(TaskStatus.Running));

            await Task.Delay(TaskConstants.TaskExecutionTime + TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            var (finishedCode, finishedTask) = await client.GetTask(taskId.ToString()).ConfigureAwait(false);
            Assert.That(finishedCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(finishedTask, Is.Not.Null);
            Assert.That(finishedTask.Id, Is.EqualTo(taskId));
            Assert.That(finishedTask.Status, Is.EqualTo(TaskStatus.Finished));
        }

        [TestCase(1000)]
        [TestCase(10_000)]
        public async Task TestManyTasks(int tasksCount)
        {
            var sw = Stopwatch.StartNew();

            using var client = new TasksApiClient();
            var taskResults = await Task.WhenAll(Enumerable.Range(0, tasksCount).Select(_ => client.PostTask()))
                .ConfigureAwait(false);

            foreach (var (code, taskId) in taskResults)
            {
                Assert.That(code, Is.EqualTo(HttpStatusCode.Accepted));
                Assert.That(taskId, Is.Not.Null);
            }

            Assert.That(sw.Elapsed, Is.LessThan(TaskConstants.TaskExecutionTime));

            var tasks = await Task.WhenAll(taskResults.Select(x => client.GetTask(x.Item2.ToString())))
                .ConfigureAwait(false);
            foreach (var (code, task) in tasks)
            {
                Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));
            }

            await Task.Delay(TaskConstants.TaskExecutionTime + TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            var completedTasks = await Task.WhenAll(taskResults.Select(x => client.GetTask(x.Item2.ToString())))
                .ConfigureAwait(false);
            foreach (var (code, task) in completedTasks)
            {
                Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.Finished));
            }
        }
    }
}