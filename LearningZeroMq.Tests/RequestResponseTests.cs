namespace LearningZeroMq.Tests
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using ZeroMQ;

    public class RequestResponseTests
    {
        [Fact]
        public void Example1()
        {
            var clientSent = new AutoResetEvent(false);
            var serverResponded = new AutoResetEvent(false);
            CreateReceivingServer(clientSent);
            CreateRequestingClient(serverResponded);
            Assert.True(clientSent.WaitOne(TimeSpan.FromSeconds(1)));
            Assert.True(serverResponded.WaitOne(TimeSpan.FromSeconds(1)));
        }

        private static Task CreateRequestingClient(EventWaitHandle serverResponded)
        {
            return Task.Factory.StartNew(() => {
                                             using (var context = ZmqContext.Create())
                                             using (var socket = context.CreateSocket(SocketType.REQ)) {
                                                 socket.Connect("tcp://localhost:9999");
                                                 Console.WriteLine("Sending : " + "Hello");
                                                 socket.Send("Hello", Encoding.UTF8);
                                                 var replyMsg = socket.Receive(Encoding.UTF8);
                                                 Console.WriteLine("Received: " + replyMsg +
                                                     Environment.NewLine);
                                                 serverResponded.Set();
                                             }
                                         });
        }

        private static Task CreateReceivingServer(EventWaitHandle clientSent)
        {
            return Task.Factory.StartNew(() => {
                                             using (var context = ZmqContext.Create())
                                             using (var socket = context.CreateSocket(SocketType.REP)) {
                                                 Console.WriteLine("Starting server with version " + ZmqVersion.Current);
                                                 socket.Bind("tcp://*:9999");
                                                 var rcvdMsg = socket.Receive(Encoding.UTF8);
                                                 Console.WriteLine("Received: " + rcvdMsg);
                                                 var replyMsg = "pong";
                                                 clientSent.Set();
                                                 Console.WriteLine("Sending : " + replyMsg +
                                                     Environment.NewLine);
                                                 socket.Send(replyMsg, Encoding.UTF8);
                                             }
                                         });
        }
    }
}