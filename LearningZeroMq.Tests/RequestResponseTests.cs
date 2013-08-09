namespace LearningZeroMq.Tests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using ZeroMQ;

    public class RequestResponseTests
    {
        [Fact]
        public void SelfEchoes()
        {
            using (var context = ZmqContext.Create())
            {
                var messageCount = 10000;
                var port = 9999;
                var clientSent = new AutoResetEvent(false);
                var sw = Stopwatch.StartNew();
                CreateReceivingServer(context, clientSent, messageCount, 0, port);
                var countdown = new CountdownEvent(messageCount);
                CreateRequestingClient(context, messageCount, port, countdown);
                Assert.True(clientSent.WaitOne(TimeSpan.FromSeconds(60)));
                sw.Stop();
                Console.WriteLine("{0} messages echoed in {1} at {2}/s", messageCount, sw.Elapsed,
                    (1.0*messageCount*1000/sw.ElapsedMilliseconds));
            }
        }

        [Fact(Timeout = 120000)]
        public void EchoClient()
        {
            using (var context = ZmqContext.Create())
            {
                var messageCount = 100000;
                var remotePort = 9998;
                var sw = Stopwatch.StartNew();
                var clientCount = 1;
                var countdown = new CountdownEvent(messageCount);
                for (var idx = 0; idx < clientCount; idx++)
                {
                    Task.Run(() => CreateRequestingClient(context, 1 + messageCount / clientCount, remotePort, countdown));
                }
                countdown.Wait();
                sw.Stop();
                Console.WriteLine("{0} messages echoed against foreign server on port {3} in {1} at {2}/s with {4} clients", messageCount, sw.Elapsed, (1.0 * messageCount * 1000 / sw.ElapsedMilliseconds), remotePort, clientCount);
            }
            
        }

        private static void CreateRequestingClient(ZmqContext context, int messageCount, int port, CountdownEvent countdown)
        {
            using (var socket = context.CreateSocket(SocketType.REQ)) {
                var sent = 0;
                socket.Connect("tcp://localhost:" + port);
                while (sent++ < messageCount) {
                    socket.Send("Hello", Encoding.UTF8);
                    socket.Receive(Encoding.UTF8, TimeSpan.FromSeconds(3));
                    countdown.Signal();
                }
            }
        }

        private static Task CreateReceivingServer(ZmqContext context, EventWaitHandle clientSent, int messageCount, int idx, int port)
        {
            return Task.Factory.StartNew(() => {
                                             using (var socket = context.CreateSocket(SocketType.REP)) {
                                                 Console.WriteLine("Starting server with version " + ZmqVersion.Current);
                                                 socket.Bind("tcp://*:" + port);
                                                 var received = 0;
                                                 while (received++ < messageCount) {
                                                     socket.Receive(Encoding.UTF8);
                                                     const string replyMsg = "pong";
                                                     socket.Send(replyMsg, Encoding.UTF8);
                                                 }
                                                 clientSent.Set();
                                             }
                                         });
        }
    }
}