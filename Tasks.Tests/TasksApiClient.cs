using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tasks.Api.DataTypes;

namespace Tasks.Tests
{
    public class TasksApiClient : IDisposable
    {
        private readonly HttpClient _client = new HttpClient {BaseAddress = new Uri("http://localhost:5000/")};

        public async Task<(HttpStatusCode, Guid?)> PostTask()
        {
            var result = await _client.PostAsync("task", null!).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (Guid.TryParse(content, out var taskId))
                return (result.StatusCode, taskId);

            return (result.StatusCode, null);
        }

        public async Task<(HttpStatusCode, TaskObject)> GetTask(string taskId)
        {
            var result = await _client.GetAsync($"task/{taskId}").ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            return (result.StatusCode, JsonConvert.DeserializeObject<TaskObject>(content));
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}