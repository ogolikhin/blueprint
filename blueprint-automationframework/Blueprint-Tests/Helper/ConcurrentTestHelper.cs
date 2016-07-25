using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using NUnit.Framework;
using Utilities.Factories;

namespace Helper
{
    public class ConcurrentTestHelper
    {
        public TestHelper Helper { get; }
        public List<Thread> Threads { get; } = new List<Thread>();
        public List<Exception> Exceptions { get; } = new List<Exception>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="helper">A TestHelper object which is initialized and Disposed outside of this object.</param>
        public ConcurrentTestHelper(TestHelper helper)
        {
            Helper = helper;
        }

        /// <summary>
        /// Adds a new thread to be run with the specified function and the specified number of iterations.
        /// </summary>
        /// <param name="function">The function to run in a separate thread.</param>
        /// <param name="iterations">(optional) The number of times to execute the function.</param>
        /// <param name="maxSleepMilliseconds">(optional) The maximum number of random milliseconds to sleep before executing this thread.</param>
        public void AddTestFunctionToThread(Action function, int iterations = 1, int maxSleepMilliseconds = 1000)
        {
            Assert.That(iterations > 0, "You must specify a positive number of iterations!");
            Threads.Add(CreateTestThread(function, iterations, maxSleepMilliseconds));
        }

        /// <summary>
        /// Runs all the threads that were added and waits until they complete, or until any of them fail.
        /// </summary>
        public void RunThreadsAndWaitToCompletion()
        {
            // Now run the threads.
            Threads.ForEach(t => t.Start());

            // Wait for threads to finish.
            Threads.ForEach(t => t.Join());

            if (Exceptions.Count > 0)
            {
                Logger.WriteError("=============================  Exceptions  ============================");
                foreach (var e in Exceptions)
                {
                    Logger.WriteError("Test failed with error: '{0}'.", e.Message);
                }
                Logger.WriteError("=======================================================================");

                throw new AggregateException(Exceptions);
            }
        }

        #region Private functions

        /// <summary>
        /// Creates a new thread to run the specified function.
        /// </summary>
        /// <param name="function">The function to run in the thread.</param>
        /// <param name="iterations">(optional) The number of iterations for each thread.</param>
        /// <param name="maxSleepMilliseconds">(optional) The maximum number of random milliseconds to sleep before executing this thread.</param>
        /// <returns>The new thread.</returns>
        private Thread CreateTestThread(Action function, int iterations = 1, int maxSleepMilliseconds = 1000)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    if (maxSleepMilliseconds > 0)
                    {
                        // Sleep for a random number of milliseconds so threads aren't all doing the same thing at the same time.
                        int sleepTime = RandomGenerator.RandomNumber(maxSleepMilliseconds);
                        Logger.WriteTrace("Thread [{0}] sleeping for {1}ms...", Thread.CurrentThread.ManagedThreadId, sleepTime);
                        Thread.Sleep(sleepTime);
                    }

                    for (int i = 0; i < iterations; ++i)
                    {
                        function();
                    }
                }
                catch (ThreadAbortException)
                {
                    Logger.WriteTrace("Thread [{0}] was aborted.", Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception e)
                {
                    lock (this)
                    {
                        Exceptions.Add(e);

                        if (Threads.Count > 0)
                        {
                            Logger.WriteError("*** Thread caught exception:  {0}", e.Message);

                            // If one thread fails, kill all other threads immediately (fail fast).
                            Logger.WriteInfo("One thread failed.  Killing all other threads...");
                            Threads.ForEach(KillThreadIfNotCurrentThread);
                            Threads.Clear();
                            Logger.WriteDebug("Finished killing all threads.");
                        }
                    }

                    throw;
                }
            });

            return thread;
        }

        /// <summary>
        /// Kills the specified thread, unless the specified thread is this thread.
        /// </summary>
        /// <param name="thread">The thread to kill.</param>
        private static void KillThreadIfNotCurrentThread(Thread thread)
        {
            if (Thread.CurrentThread != thread)
            {
                Logger.WriteTrace("Killing thread [{0}]...", Thread.CurrentThread.ManagedThreadId);
                thread.Abort();
            }
        }

        #endregion Private functions
    }
}
