using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using Xunit;

namespace AkkaNetCancelableLongTask.Tests
{
    public class CancelableActorSpec : Akka.TestKit.Xunit.TestKit
    {
        public CancelableActorSpec() : base(@"pinned-dispatcher {
                                                type = PinnedDispatcher
                                              }")
        { }

           [Fact]
        public void ShouldBeTaskComplete()
        {
            // Arrange
            var targetActor = Sys.ActorOf(CancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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
            ,3.Seconds());
        }

        [Fact]
        public void ShouldBeTaskCanceled()
        {
            // Arrange
            var targetActor = Sys.ActorOf(CancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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
        public void ShouldBeTaskFault()
        {
            string guid = Guid.NewGuid().ToString();
            // Arrange
            var targetActor = Sys.ActorOf(CancelableActor.Props().WithDispatcher("pinned-dispatcher"));
            var cts = new CancellationTokenSource();
            var msg = new CancelableMessage
            {
                CancellationToken = cts.Token,
                LongTaskExcuteTime = 10.Seconds(),
                Action = () => throw new Exception(guid)
            };

            // Act
            targetActor.Tell(msg, TestActor);
            Thread.Sleep(500);
            cts.Cancel();

            // Assert
            ExpectMsg<RecievedMessage>();
            ExpectMsg<TaskFault>(m => m.Exception
                                       .InnerExceptions
                                       .Should()
                                       .OnlyContain(x => x.Message == guid),
            3.Seconds());
        }
    }
}
