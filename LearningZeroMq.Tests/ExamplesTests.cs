namespace LearningZeroMq.Tests
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using ZeroMQ;

    public class ExamplesTests
    {
        [Fact]
        public void Example1()
        {
            var clientSent = false;
            var serverResponded = false;
            var srvtask = Task.Factory.StartNew(() => {
                using (var context = ZmqContext.Create())
                using (var socket = context.CreateSocket(SocketType.REP)) {
                    socket.Bind("tcp://*:9999");
                    var rcvdMsg = socket.Receive(Encoding.UTF8);
                    Console.WriteLine("Received: " + rcvdMsg);
                    var replyMsg = "pong";
                    clientSent = true;
                    Console.WriteLine("Sending : " + replyMsg +
                        Environment.NewLine);
                    socket.Send(replyMsg, Encoding.UTF8);
                }
            });
            var cltTask = Task.Factory.StartNew(() => {
                using (var context = ZmqContext.Create())
                using (var socket = context.CreateSocket(SocketType.REQ)) {
                    socket.Connect("tcp://localhost:9999");
                    Console.WriteLine("Sending : " + "Hello");
                    socket.Send("Hello", Encoding.UTF8);
                    var replyMsg = socket.Receive(Encoding.UTF8);
                    Console.WriteLine("Received: " + replyMsg +
                        Environment.NewLine);
                    serverResponded = true;
                }
            });
            var success = Task.WaitAll(new[] {srvtask, cltTask}, TimeSpan.FromSeconds(2));
            //Assert.True(success);
            Assert.True(clientSent);
            Assert.True(serverResponded);
        }
    }
}