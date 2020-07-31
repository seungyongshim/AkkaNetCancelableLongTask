using AkkaNetCancelableLongTask.Messages;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Threading;
using Xunit;

namespace AkkaNetCancelableLongTask.Tests
{
    public class ModernStyleCancelableActorSpec : Akka.TestKit.Xunit.TestKit
    {
        public ModernStyleCancelableActorSpec() : base(@"
pinned-dispatcher {
    type = PinnedDispatcher
}")
        { }

        [Fact]
        public void ShouldBeTaskCanceled()
        {
            // Arrange
            var targetActor = Sys.ActorOf(ModernStyleCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
            var cts = new CancellationTokenSource();
            var msg = new CancelableMessage
            {
                CancellationToken = cts.Token,
                LongTaskExcuteTime = 10.Seconds(),
            };

            // Act
            targetActor.Tell(msg, TestActor);
            Thread.Sleep(500);
            cts.Cancel();

            // Assert
            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskCanceled>(3.Seconds());
        }

        [Fact]
        public void ShouldBeTaskComplete()
        {
            // Arrange
            var targetActor = Sys.ActorOf(ModernStyleCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
            var cts = new CancellationTokenSource();
            var msg = new CancelableMessage
            {
                CancellationToken = cts.Token,
                LongTaskExcuteTime = 2.Seconds(),
            };

            // Act
            targetActor.Tell(msg, TestActor);

            // Assert
            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskComplete>((m, s) =>
                s.Path.Should().Be(targetActor.Path)
            , 3.Seconds());
        }

        [Fact]
        public void ShouldBeTaskFault()
        {
            // Arrange
            var targetActor = Sys.ActorOf(ModernStyleCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
            var cts = new CancellationTokenSource();
            var msg = new CancelableMessage
            {
                CancellationToken = cts.Token,
                LongTaskExcuteTime = 10.Seconds(),
                MakeExeption = true
            };

            // Act
            targetActor.Tell(msg, TestActor);

            // Assert
            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskFaultModernStyle>(m => m.Exception
                                                  .Message
                                                  .Should()
                                                  .Be("84A5F0CD-05CC-49D4-9FD2-A530A15A8A60"),
            3.Seconds());
        }

        [Fact] 
        public void ShouldBeMessageTwiceCompletes()
        {
            var targetActor = Sys.ActorOf(ModernStyleCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
            var cts = new CancellationTokenSource();
            var msg = new CancelableMessage
            {
                CancellationToken = cts.Token,
                LongTaskExcuteTime = 2.Seconds(),
            };

            targetActor.Tell(msg, TestActor);
            targetActor.Tell(msg, TestActor);

            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskComplete>();
            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskComplete>();
        }
    }
}