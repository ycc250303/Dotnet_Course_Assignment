using System;
using System.Collections.Generic;
using System.Threading;

using static System.Console;
using static System.Threading.Thread;

class BlockingQueueWithCondVar<T>
{
    private readonly object _syncLock = new object();
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly int _capacity;

    public BlockingQueueWithCondVar(int capacity)
    {
        _capacity = capacity;
    }

    public void Enqueue(T obj, string producerName)
    {
        lock (_syncLock)
        {
            // 条件变量 1：队列满 -> 生产者等待。
            while (_queue.Count >= _capacity)
            {
                WriteLine($"[{producerName}] 队列已满，进入等待...");
                Monitor.Wait(_syncLock);
            }

            _queue.Enqueue(obj);
            WriteLine($"[{producerName}] 生产 {obj}，当前队列数量：{_queue.Count}");

            // 通知等待线程“队列状态改变了”，让它们重新检查条件。
            Monitor.PulseAll(_syncLock);
        }
    }

    public T Dequeue(string consumerName)
    {
        lock (_syncLock)
        {
            // 条件变量 2：队列空 -> 消费者等待。
            while (_queue.Count == 0)
            {
                WriteLine($"[{consumerName}] 队列为空，进入等待...");
                Monitor.Wait(_syncLock);
            }

            T item = _queue.Dequeue();
            WriteLine($"[{consumerName}] 消费 {item}，当前队列数量：{_queue.Count}");

            // 消费后可能腾出了空间，唤醒等待中的生产者。
            Monitor.PulseAll(_syncLock);
            return item;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var queue = new BlockingQueueWithCondVar<int>(capacity: 3);

        var producer = new Thread(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                queue.Enqueue(i, "生产者");
                Sleep(200);
            }
        });

        var consumer = new Thread(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                queue.Dequeue("消费者");
                Sleep(500);
            }
        });

        producer.Start();
        consumer.Start();
        producer.Join();
        consumer.Join();

        WriteLine("演示结束。");
    }
}