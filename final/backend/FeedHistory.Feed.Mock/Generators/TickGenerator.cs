﻿using System;
using System.Collections.Generic;
using System.Linq;
using FeedHistory.Common;

namespace FeedHistory.Feed.Mock.Generators
{
    public class TickGenerator
    {
        private readonly List<string> _symbols;
        private List<SymbolTickGenerator> _generators;

        public TickGenerator(List<string> symbols)
        {
            _symbols = symbols;

        }

        public event Action<Tick> Tick;

        public void Start()
        {
            _generators = _symbols.Select(symbol => new SymbolTickGenerator(symbol, new Random(), 100)).ToList();

            foreach (var generator in _generators)
            {
                generator.Tick += Tick;
                generator.Start();
            }
        }

        public void Stop()
        {
            foreach (var generator in _generators)
            {
                generator.Stop();
                generator.Tick -= Tick;
            }
        }
    }
}