using Akka.Actor;
using AkkaNetCancelableLongTask.Messages;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AkkaNetCancelableLongTask.Tests")]

namespace AkkaNetCancelableLongTask
{
    internal class ModernStyleCancelableActor : ReceiveActor
    {
        public ModernStyleCancelableActor()
        {
            ReceiveAsync<CancelableMessage>(Handle);
        }

        public static Props Props() => Akka.Actor.Props.Create(() => new ModernStyleCancelableActor());

        private async Task Handle(CancelableMessage msg)
        {
            // ActorContext 가 없으므로 캡쳐한 sender와 self를 사용해야 한다.
            var capturedSender = Sender;
            var capturedSelf = Self;

            // Task 선언
            var longTask = Task.Run(async () =>
            {
                capturedSender.Tell(new RecievedMessage(), capturedSelf);

                if (msg.MakeExeption)
                {
                    throw new Exception("84A5F0CD-05CC-49D4-9FD2-A530A15A8A60");
                }

                await Task.Delay(msg.LongTaskExcuteTime, msg.CancellationToken);
            }, msg.CancellationToken);

            // 실행
            try
            {
                await longTask;
                Sender.Tell(new TaskComplete(), Self);
            }
            catch (TaskCanceledException)
            {
                Sender.Tell(new TaskCanceled(), Self);
            }
            catch (Exception ex)
            {
                Sender.Tell(new TaskFaultModernStyle(ex), Self);
            }
        }
    }
}