using System;

namespace Tasks.Api.DataTypes
{
    public class TaskObject
    {
        public Guid Id { get; set; }
        public TaskStatus Status { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}