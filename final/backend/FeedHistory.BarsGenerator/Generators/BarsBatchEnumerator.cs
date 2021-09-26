using System;
using System.Collections;
using System.Collections.Generic;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Generators
{
    public class BarsBatchEnumerator : IEnumerator<BarsBatch>
    {
        private readonly BarsEnumerator _baseEnumerator;
        private readonly int _batchSize;
        private bool _end;

        public BarsBatchEnumerator(BarsEnumerator baseEnumerator, int batchSize)
        {
            if (batchSize % 2 != 0) throw new ArgumentException("Batch size should be even", nameof(batchSize));

            _baseEnumerator = baseEnumerator;
            _batchSize = batchSize;
        }

        public bool MoveNext()
        {
            if (_end) return false;
            
            StartNewBatch();
            FillBatch();

            return true;
        }

        private void FillBatch()
        {
            for (int i = 0; i < _batchSize; i++)
            {
                if (!_baseEnumerator.MoveNext())
                {
                    _end = true;
                    break;
                }

                Current.Add(_baseEnumerator.Current);
            }
        }

        private void StartNewBatch()
        {
            if (Current == null)
            {
                Current = new BarsBatch(_batchSize, _baseEnumerator.Symbol, _baseEnumerator.Period);
            }
            else
            {
                Current.BeginNewBatch();
            }
        }

        public void Reset()
        {
            Current = new BarsBatch(_batchSize, _baseEnumerator.Symbol, _baseEnumerator.Period);
            _end = false;
        }

        public BarsBatch Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}