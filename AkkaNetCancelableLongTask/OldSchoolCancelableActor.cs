using Akka.Actor;
using AkkaNetCancelableLongTask.Messages;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AkkaNetCancelableLongTask.Tests")]

namespace AkkaNetCancelableLongTask
{
    internal class OldSchoolCancelableActor : ReceiveActor
    {
        public OldSchoolCancelableActor()
        {
            ReceiveAsync<CancelableMessage>(Handle);
        }

        public static Props Props() => Akka.Actor.Props.Create(() => new OldSchoolCancelableActor());

        private async Task Handle(CancelableMessage msg)
        {
            var sender = Sender;
            var self = Self;

            await Task.Run(async () =>
            {
                // ActorContext 가 없으므로 캡쳐한 sender와 self를 사용해야 한다.
                sender.Tell(new RecievedMessage(), self);

                if (msg.MakeExeption)
                {
                    throw new Exception("4945A183-B63B-4533-B7A2-C918983BE394");
                }

                await Task.Delay(msg.LongTaskExcuteTime, msg.CancellationToken);
            }, msg.CancellationToken)
            .ContinueWith(task =>
            {
                switch (task)
                {
                    case var s when s.IsFaulted == true:
                        Sender.Tell(new TaskFaultOldSchool(s.Exception));
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
}