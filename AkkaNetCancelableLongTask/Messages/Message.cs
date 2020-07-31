using System;
using System.Threading;

namespace AkkaNetCancelableLongTask.Messages
{
    internal class CancelableMessage
    {
        public Action Action { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public TimeSpan LongTaskExcuteTime { get; set; }
        public bool MakeExeption { get; internal set; }
    }

    internal class RecievedMessage { }

    internal class TaskCanceled { }

    internal class TaskComplete { }

    internal class TaskFaultModernStyle
    {
        public TaskFaultModernStyle(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; set; }
    }

    internal class TaskFaultOldSchool
    {
        public TaskFaultOldSchool(AggregateException exception)
        {
            Exception = exception;
        }

        public AggregateException Exception { get; set; }
    }
}