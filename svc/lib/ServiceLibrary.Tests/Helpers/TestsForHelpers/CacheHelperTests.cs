using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Helpers.TestsForHelpers
{
    [TestClass]
    public class CacheHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitializationNegativeExpiration_Failed()
        {
            var value = FeatureTypes.BlueprintOpenApi;

            var helper = new CacheHelper<FeatureTypes>(
                TimeSpan.FromMilliseconds(-1),
                () => value
            );
            helper.Get();

            Assert.Fail("Must fail before");
        }

        [TestMethod]
        public void InitializationWithObjectType()
        {
            var value = new Dictionary<string, int> {{"test", 10}};

            var helper = new CacheHelper<IDictionary<string, int>>(
                TimeSpan.FromMilliseconds(10),
                () => value
            );

            Assert.IsNotNull(helper);
            Assert.AreSame(value, helper.Get());
        }

        [TestMethod]
        public void InitializationWithEnum()
        {
            var value = FeatureTypes.BlueprintOpenApi;

            var helper = new CacheHelper<FeatureTypes>(
                TimeSpan.FromMilliseconds(10),
                () => value
            );

            Assert.IsNotNull(helper);
            Assert.AreEqual((object)value, helper.Get());
        }

        [TestMethod]
        public void ExpiredCache_GetsNewValue()
        {
            int value = 0;
            Func<int> factory = () => ++value;
            var currentTime = DateTime.Now;
            var expirationTime = TimeSpan.FromMinutes(10);

            var helper = new CacheHelper<int>(
                expirationTime,
                factory,
                () => currentTime
            );
            helper.Get(); // don't care about result yet

            // Act
            currentTime += expirationTime + TimeSpan.FromMilliseconds(1); // expire cache
            var result = helper.Get();

            // Asserts
            Assert.AreEqual(2, value, "valueFactory must be called twice");
            Assert.AreEqual(value, result);
        }


        [TestMethod]
        public void GetCachedValueBeforeExpiration()
        {
            const int threadCount = 3;
            int value = 0;
            Func<int?> factory = () => ++value;

            var tasks = new List<Task>();
            var currentTime = DateTime.Now;

            var helper = new CacheHelper<int?>(
                TimeSpan.FromSeconds(5),
                factory,
                () => currentTime
            );

            var barrier = new Barrier(threadCount);

            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(
                    Task.Factory.StartNew(() =>
                    {
                        barrier.SignalAndWait(1000);
                        var result = helper.Get();
                        Assert.AreEqual(1, result);

                        Thread.Sleep(new Random(Thread.CurrentThread.ManagedThreadId).Next(10));
                    })
                );
            }

            tasks.ForEach(t => t.Wait(2000));

            Assert.AreEqual(1, value, "Value was not cached");
        }
    }
}
