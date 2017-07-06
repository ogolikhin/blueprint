using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers.Cache
{
    [TestClass]
    public class AsyncCacheTests
    {
        private Mock<ObjectCache> _cacheMock;

        [TestInitialize]
        public void Initialize()
        {
            _cacheMock = new Mock<ObjectCache>();
        }

        [TestMethod]
        public async Task AddOrGetExistingAsync_ItemNotCached_ItemFactoryCalledAndResultCached()
        {
            // Arrange
            var key = "this-is-the-key";
            var value = "this-is-the-value";
            var cacheItemPolicy = new CacheItemPolicy() { };
            var isFactoryCalled = false;
            Func<Task<string>> func = () =>
            {
                isFactoryCalled = true;
                return Task.FromResult(value);
            };

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), cacheItemPolicy, null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExistingAsync(key, func, cacheItemPolicy);

            // Asserts
            Assert.AreEqual(value, result);
            Assert.IsTrue(isFactoryCalled);
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExistingAsync_AbsoluteExpiration_ItemNotCached_ItemFactoryCalledAndResultCached()
        {
            // Arrange
            var key = "this-is-the-key";
            var absoluteExpiration = DateTime.UtcNow.AddSeconds(30);

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), It.Is<CacheItemPolicy>(policy => policy.AbsoluteExpiration == absoluteExpiration), null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExistingAsync(key, () => Task.FromResult("this-is-the-value"), absoluteExpiration);

            // Asserts
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExistingAsync_SlidingExpiration_ItemNotCached_ItemFactoryCalledAndResultCached()
        {
            // Arrange
            var key = "this-is-the-key";
            var slidingExpiration = TimeSpan.FromMinutes(1);

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), It.Is<CacheItemPolicy>(policy => policy.SlidingExpiration == slidingExpiration), null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExistingAsync(key, () => Task.FromResult("this-is-the-value"), slidingExpiration);

            // Asserts
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExistingAsync_ItemCached_ItemFactoryNotCalled()
        {
            // Arrange
            var key = "this-is-the-key";
            var value = "this-is-the-value";
            var cacheItemPolicy = new CacheItemPolicy() { };
            var isFactoryCalled = false;
            var cachedLazy = new AsyncLazy<string>(() => Task.FromResult(value));
            Func<Task<string>> func = () =>
            {
                isFactoryCalled = true;
                return Task.FromResult(value);
            };

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), cacheItemPolicy, null))
                .Returns(cachedLazy);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExistingAsync(key, func, cacheItemPolicy);

            // Asserts
            Assert.AreEqual(value, result);
            Assert.IsFalse(isFactoryCalled);
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExistingAsync_ItemNotCached_ItemFactoryFailed_ExceptionRetrown()
        {
            // Arrange
            var key = "this-is-the-key";
            var cacheItemPolicy = new CacheItemPolicy() { };
            Func<Task<string>> func = async () => {
                await Task.Delay(1);
                throw new ArgumentNullException();
            };

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), cacheItemPolicy, null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = cache.AddOrGetExistingAsync(key, func, cacheItemPolicy);

            // Asserts
            try
            {
                await result;

                Assert.Fail("ItemFactory fail is not raised");
            }
            catch (ArgumentNullException)
            {

            }

            _cacheMock.Verify(c => c.Remove(key, null), Times.Once);
        }

        // Disabling the test because of the sporadic failures in build environments
        // which requires further investigation
        [Ignore]
        [TestMethod]
        public void AddOrGetExistingAsync_ItemNotCached_ParallelRequests()
        {
            const int parallelRequestsCount = 10;
            int factoryCalls = 0;

            using (var barrier = new Barrier(parallelRequestsCount + 1))
            using (var memoryCache = new MemoryCache(nameof(AddOrGetExistingAsync_ItemNotCached_ParallelRequests)))
            {
                var asyncCache = new AsyncCache(memoryCache);
                var key = "this-is-key";
                
                for (int i = 0; i < parallelRequestsCount; i++)
                {
                    string value = "Value " + i;
                    Task.Factory.StartNew(async () =>
                    {
                        barrier.SignalAndWait(500);
                        var resultValue = await asyncCache.AddOrGetExistingAsync(
                            key,
                            () =>
                            {
                                Interlocked.Increment(ref factoryCalls);
                                return Task.FromResult(value);
                            },
                            DateTime.UtcNow.AddSeconds(30)
                        );

                        Assert.IsTrue(resultValue.StartsWith("Value", StringComparison.InvariantCulture));

                        barrier.SignalAndWait(500);
                    });
                }

                barrier.SignalAndWait(500);

                // Threads will call Cache at that time

                barrier.SignalAndWait(500);
            }

            Assert.AreEqual(1, factoryCalls);
        }
    }
}
