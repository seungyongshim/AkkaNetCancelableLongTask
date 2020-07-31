using AkkaNetCancelableLongTask.Messages;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Threading;
using Xunit;

namespace AkkaNetCancelableLongTask.Tests
{
    public class OldSchoolCancelableActorSpec : Akka.TestKit.Xunit.TestKit
    {
        public OldSchoolCancelableActorSpec() : base(@"pinned-dispatcher {
                                                type = PinnedDispatcher
                                              }")
        { }

        [Fact]
        public void ShouldBeTaskCanceled()
        {
            // Arrange
            var targetActor = Sys.ActorOf(OldSchoolCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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
            var targetActor = Sys.ActorOf(OldSchoolCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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
            var targetActor = Sys.ActorOf(OldSchoolCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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
            ExpectMsg<TaskFaultOldSchool>(m => m.Exception
                                       .InnerExceptions
                                       .Should()
                                       .OnlyContain(x => x.Message == "4945A183-B63B-4533-B7A2-C918983BE394"),
            3.Seconds());
        }

        [Fact]
        public void ShouldBeMessageTwiceCompletes()
        {
            var targetActor = Sys.ActorOf(OldSchoolCancelableActor.Props().WithDispatcher("pinned-dispatcher"));
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