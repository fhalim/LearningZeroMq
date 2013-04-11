namespace LearningZeroMq.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using Xunit;
    using ZooKeeperNet;

    public class ZookeeperTests
    {
        [Fact]
        public void PersistantNodesShouldPersist()
        {
            var watcher = new CountdownWatcher();
            var random = new Random().Next();
            var path = "/foo" + random;
            using (var zk = ZookeeperFactory(watcher))
            {
                zk.Create(path, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
            }
            using (var zk = ZookeeperFactory(watcher))
            {
                Assert.NotNull(zk.Exists(path, false));
            }
        }

        [Fact]
        public void EphemeralNodesShouldNotPersist()
        {
            var watcher = new CountdownWatcher();
            var random = new Random().Next();
            var path = "/foo" + random;
            using (var zk = ZookeeperFactory(watcher))
            {
                zk.Create(path, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
                Assert.NotNull(zk.Exists(path, false));
            }
            using (var zk = ZookeeperFactory(watcher))
            {
                Assert.Null(zk.Exists(path, false));
            }
        }
        [Fact]
        public void TestLocking()
        {
            var watcher = new CountdownWatcher();
            var random = new Random().Next();
            var path = "/foo" + random;
            using (var zk = ZookeeperFactory(watcher))
            {
                zk.Create(path, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);
                Assert.NotNull(zk.Exists(path, false));
            }
            using (var zk = ZookeeperFactory(watcher))
            {
                Assert.Null(zk.Exists(path, false));
            }
        }

        private static ZooKeeper ZookeeperFactory(IWatcher watcher)
        {
            return new ZooKeeper("127.0.0.1:2181", TimeSpan.FromSeconds(3), watcher);
        }

        public class CountdownWatcher : IWatcher
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            public virtual void Process(WatchedEvent @event)
            {
                Console.WriteLine("Got Event " + @event);
            }
        } 
    }
}