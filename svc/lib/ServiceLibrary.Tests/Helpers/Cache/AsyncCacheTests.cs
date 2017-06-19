using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Runtime.Caching;
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
        public async Task AddOrGetExystingAsync_ItemNotCached_ItemFactoryCalledAndResultCached()
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
                //.Setup(c => c.AddOrGetExisting(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CacheItemPolicy>(), null))
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), cacheItemPolicy, null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExystingAsync(key, func, cacheItemPolicy);

            // Asserts
            Assert.AreEqual(value, result);
            Assert.IsTrue(isFactoryCalled);
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExystingAsync_AbsoluteExpiration_ItemNotCached_ItemFactoryCalledAndResultCached()
        {
            // Arrange
            var key = "this-is-the-key";
            var value = "this-is-the-value";
            var absoluteExpiration = DateTime.UtcNow.AddSeconds(30);
            Func<Task<string>> func = () => Task.FromResult(value);

            _cacheMock
                .Setup(c => c.AddOrGetExisting(key, It.IsAny<AsyncLazy<string>>(), It.Is<CacheItemPolicy>(policy => policy.AbsoluteExpiration == absoluteExpiration), null))
                .Returns(null);

            var cache = new AsyncCache(_cacheMock.Object);

            // Act
            var result = await cache.AddOrGetExystingAsync(key, func, absoluteExpiration);

            // Asserts
            Assert.AreEqual(value, result);
            _cacheMock.VerifyAll();
        }

        [TestMethod]
        public async Task AddOrGetExystingAsync_ItemCached_ItemFactoryNotCalled()
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
            var result = await cache.AddOrGetExystingAsync(key, func, cacheItemPolicy);

            // Asserts
            Assert.AreEqual(value, result);
            Assert.IsFalse(isFactoryCalled);
            _cacheMock.VerifyAll();
        }

        [TestMethod]        
        public async Task AddOrGetExystingAsync_ItemNotCached_ItemFactoryFailed_ExceptionRetrown()
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
            var result = cache.AddOrGetExystingAsync(key, func, cacheItemPolicy);

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
    }
}
