using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Xunit;

namespace XUnitTests
{
    static class CancelExtention
    {
        public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
        {
            foreach (var item in en)
            {
                token.ThrowIfCancellationRequested();
                Debug.WriteLine(item);
                yield return item;
            }
        }
    }
    public class GenericsUnitTests
    {
        internal class ActionQueueAsync
        {
            public int SecondsTimeout { get; set; } = 10;
            public int MillisecondsTick { get; set; } = 50;
            public ActionQueueAsync() { }
            public Task Start()
            {
                return Task.Run(() =>
                {
                    if (cts == null)
                    {
                        cts = new CancellationTokenSource();
                        Task.Run(Executer);
                    }
                });
            }
            public Task Stop()
            {
                return Task.Run(() =>
                {
                    if (cts != null)
                    {
                        cts.Cancel();
                        while (cts != null) Task.Delay(MillisecondsTick).Wait();
                    }
                });
            }
            public Task<T> ExecuteInAsync<T>(string id, Func<T> action)
            {
                return Task.Run(() =>
                {
                    var sync = new ManualResetEvent(false);
                    T retorno = default;
                    queue.Add(() =>
                    {
                        try
                        {
                            retorno = action();
                        }
                        catch (Exception)
                        {
                            // todo... Grabar la excepción
                            throw;
                        }
                        finally
                        {
                            sync.Set();
                        }
                    });
                    sync.WaitOne(TimeSpan.FromSeconds(SecondsTimeout));
                    return retorno;
                });
            }
            public Task Enqueue(string id, Action action)
            {
                return Task.Run(() =>
                {
                    queue.Add(() =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception)
                        {
                            // todo... Grabar la excepción
                            throw;
                        }
                    });
                });
            }
            async void Executer()
            {
                // Borrar posibles datos anteriores
                Clear();
                while (cts.IsCancellationRequested == false)
                {
                    while (queue.Count > 0)
                    {
                        if (cts.IsCancellationRequested == true) break;
                        var action = queue.Take();
                        action();
                    }
                    await Task.Delay(MillisecondsTick);
                }
                cts = null;
            }
            void Clear()
            {
                while (queue.Count > 0)
                {
                    queue.TryTake(out _);
                }
            }
            readonly BlockingCollection<Action> queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            CancellationTokenSource cts = null;
        }
        [Fact]
        public async void ActionQueueAsyncTest1()
        {
            var Queue = new ActionQueueAsync();
            await Queue.Start();
            await Task.Delay(1000);
            await Queue.Stop();
        }
        [Fact]
        public async void ActionQueueAsyncTest2()
        {

            var Queue = new ActionQueueAsync();
            await Queue.Start();
            await Queue.Enqueue("Hola", () =>
            {
                Task.Delay(500).Wait();
                Debug.WriteLine($"Work1");
            });
            await Queue.Enqueue("Hola", () =>
            {
                Debug.WriteLine($"Work2");
            });
            await Task.Delay(1000);
            await Queue.Stop();
        }
        [Fact]
        public async void ActionQueueAsyncTest3()
        {
            var Queue = new ActionQueueAsync();
            await Queue.Start();
            await Queue.Enqueue("Hola", () =>
            {
                Task.Delay(500).Wait();
                Debug.WriteLine($"Work1");
            });
            var ret = await Queue.ExecuteInAsync<object>("Hola", () => {
                return new { txt = "Hola que tal" };
            });
            Debug.WriteLine($"{ret}");

            await Queue.Enqueue("Hola", () =>
            {
                Debug.WriteLine($"Work2");
            });
            await Task.Delay(1000);
            await Queue.Stop();
        }

        [Fact]
        public void LinqWithCancelTest01()
        {
            var input = Enumerable.Range(1, 100);
            var cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    var output = input
                        .WithCancellation(cts.Token)
                        .Select(i => DelayMult(i))
                        .Where(i => i%2 == 0)
                        .Select(i => new {a = DelayMult(i), b=DelayMult(i+2)})
                        .ToList();
                }
                catch (Exception)
                {
                    Debug.WriteLine("Operacion cancelada...");
                    //throw;
                }
            });
            Task.Delay(TimeSpan.FromSeconds(20)).Wait();
            cts.Cancel();
            Task.Delay(TimeSpan.FromSeconds(20)).Wait();
        }

        int DelayMult(int i)
        {
            Task.Delay(1000).Wait();
            return i + 2;
        }
    }
}