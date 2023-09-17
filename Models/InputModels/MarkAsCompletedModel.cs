using System;

namespace TimesheetBE.Models.InputModels
{
    public class MarkAsCompletedModel
    {
        public TaskType Type { get; set; }
        public Guid TaskId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public enum TaskType
    {
        Project = 1,
        Task,
        Subtask
    }
}
