using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Idunas.DanceMusicPlayer.Util
{
    /// <summary>
    /// Most parts of the code from:
    /// https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
    /// </summary>
    [DebuggerStepThrough]
    public static class AsyncHelper
    {
        /// <summary>
        /// Executes an async Task method concurrently (aka 'fire & forget')
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunAndContinue(Func<Task> task)
        {
            Task.Run(task);
        }

        /// <summary>
        /// Executes an async Task method concurrently (aka 'fire & forget')
        /// </summary>
        /// <param name="task">Task method to execute</param>
        /// <param name="onExceptionHandler">A method to call when an exception occurs</param>
        public static void RunAndContinue(Func<Task> task, Action<Exception> onExceptionHandler)
        {
            Task
                .Run(task)
                .ContinueWith(faultedTask =>
                {
                    foreach (var tmpException in faultedTask.Exception.InnerExceptions)
                    {
                        onExceptionHandler(tmpException);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunAndWait(Func<Task> task)
        {
            // Remember the current sync context
            var oldContext = SynchronizationContext.Current;

            try
            {
                // Switch to our new sync context
                var exclusiveContext = new TcExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(exclusiveContext);

                // Send the work that needs to be done to our context
                exclusiveContext.Post(async _ =>
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception ex)
                    {
                        exclusiveContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        exclusiveContext.EndMessageLoop();
                    }
                }, null);

                exclusiveContext.BeginMessageLoop();
            }
            finally
            {
                // Switch back to the previous context
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunAndWait<T>(Func<Task<T>> task)
        {
            // Remember the current sync context
            var oldContext = SynchronizationContext.Current;

            var result = default(T);

            try
            {
                // Switch to our new sync context
                var exclusiveContext = new TcExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(exclusiveContext);

                // Send the work that needs to be done to our context
                exclusiveContext.Post(async _ =>
                {
                    try
                    {
                        result = await task();
                    }
                    catch (Exception ex)
                    {
                        exclusiveContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        exclusiveContext.EndMessageLoop();
                    }
                }, null);

                exclusiveContext.BeginMessageLoop();
            }
            finally
            {
                // Switch back to the previous context
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }

            return result;
        }

        [DebuggerStepThrough]
        private class TcExclusiveSynchronizationContext : SynchronizationContext
        {
            private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            private readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();
            private bool mDone;

            public Exception InnerException { get; set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => mDone = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!mDone)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}