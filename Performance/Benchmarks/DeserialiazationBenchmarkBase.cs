﻿using System;
using System.Diagnostics;
using ProtoSharp.Performance.Messages;

namespace ProtoSharp.Performance.Benchmarks
{
    abstract class DeserializationBenchmarkBase<T> : IBenchmark
    {
        public DeserializationBenchmarkBase(int iterations)
        {
            _iterations = iterations;
        }

        public BenchmarkResult Run(IBenchmarkAdapter target)
        {
            target.Reset();
            SerializeExemplar(target);
            int length = target.BytesUsed;
            var min = TimeSpan.MaxValue;
            for(int i = 0; i != _iterations; ++i)
            {
                target.Reset(length);
                T item;
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    Deserialize(target, out item);
                }
                catch(Exception)
                {
                    return new BenchmarkResult(Name, TimeSpan.Zero, 0);
                }
                stopwatch.Stop();
                if(stopwatch.Elapsed < min)
                    min = stopwatch.Elapsed;
            }
            return new BenchmarkResult(Name, min, target.BytesUsed);
        }

        protected abstract string Name { get; }

        protected abstract void SerializeExemplar(IBenchmarkAdapter target);
        protected abstract void Deserialize(IBenchmarkAdapter target, out T item);

        int _iterations;
    }
}
