using Akka.Actor;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AkkaNetCancelableLongTask.Tests")]

namespace AkkaNetCancelableLongTask
{
    internal class CancelableActor : ReceiveActor
    {
        public CancelableActor()
        {
            ReceiveAsync<CancelableMessage>(Handle);
        }

        public static Props Props() => Akka.Actor.Props.Create(() => new CancelableActor());

        private async Task Handle(CancelableMessage msg)
        {
            var sender = Sender;
            var self = Self;

            await Task.Run(async () =>
            {
                // ActorContext 가 없으므로 캡쳐한 sender와 self를 사용해야 한다.
                sender.Tell(new RecievedMessage(), self);
                msg.Action?.Invoke();
                await Task.Delay(msg.LongTaskExcuteTime, msg.CancellationToken);
            }, msg.CancellationToken)
            .ContinueWith(async task =>
            {
                await Task.Delay(0).ConfigureAwait(false);
                switch (task)
                {
                    case var s when s.IsFaulted == true:
                        Sender.Tell(new TaskFault(s.Exception));
                        break;

                    case var s when s.IsCompletedSuccessfully == true:
                        Sender.Tell(new TaskComplete());
                        break;

                    case var s when s.IsCanceled == true && s.IsCompleted == true:
                        Sender.Tell(new TaskCanceled());
                        break;

                    default:
                        break;
                }
                
            });
        }
    }

    internal class CancelableMessage
    {
        public Action Action { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public TimeSpan LongTaskExcuteTime { get; set; }
    }

    internal class RecievedMessage { }

    internal class TaskCanceled { }

    internal class TaskComplete { }

    internal class TaskFault
    {
        public TaskFault(AggregateException exception)
        {
            Exception = exception;
        }

        public AggregateException Exception { get; set; }
    }
}