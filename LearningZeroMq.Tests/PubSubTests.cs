namespace LearningZeroMq.Tests
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using ZeroMQ;

    public class PubSubTests
    {
        [Fact]
        public void TestPubSub()
        {
            var maxClients = 5;
            var messagesPerClient = 10;
            var clientsStarted = new CountdownEvent(maxClients);
            var srv = Task.Factory.StartNew(() => {
                                                using (var ctx = ZmqContext.Create())
                                                using (var sock = ctx.CreateSocket(SocketType.PUB)) {
                                                    sock.Bind("tcp://*:5555");
                                                    clientsStarted.Wait(TimeSpan.FromSeconds(maxClients));
                                                    foreach (var i in Enumerable.Range(1, maxClients)) {
                                                        sock.Send(i.ToString(CultureInfo.CurrentCulture), Encoding.UTF8);
                                                        Console.WriteLine("Sent message " + i);
                                                    }
                                                }
                                            });
            var clientReceivedMessage = new CountdownEvent(maxClients * messagesPerClient);
            var subscribers = Enumerable.Range(1, maxClients).Select(e => CreateSubscriber(clientReceivedMessage, clientsStarted)).ToArray();
            srv.Wait(TimeSpan.FromSeconds(10));
            Assert.True(srv.IsCompleted, "Server should have completed");
            clientReceivedMessage.Wait(TimeSpan.FromSeconds(10));
            Assert.True(clientReceivedMessage.IsSet, "All subscribers should have gotten a copy of the message");
        }
        private static Task CreateSubscriber(CountdownEvent receivedMessage, CountdownEvent clientsStarted)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var context = ZmqContext.Create())
                using (var socket = context.CreateSocket(SocketType.SUB))
                {
                    socket.Subscribe(new byte[0]);
                    socket.Connect("tcp://localhost:5555");
                    clientsStarted.Signal();
                    Console.WriteLine("Connected");
                    while (true) {
                        var replyMsg = socket.Receive(Encoding.UTF8);
                        Console.WriteLine("[{0}] Received: {1}", Thread.CurrentThread.ManagedThreadId, replyMsg);
                        receivedMessage.Signal();
                    }
                }
            });
        }
    }
}