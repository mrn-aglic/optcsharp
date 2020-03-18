using System;
using System.Collections.Generic;
using System.Linq;
using TracingCore.Exceptions;

namespace TracingCore.TracingManagers
{
    public enum LoopType
    {
        For,
        While,
        DoWhile
    }

    public class LoopData
    {
        
    }

    public class LoopManager
    {
        private readonly Dictionary<string, int> _loopIterations;
        private const int MaxIter = 1000;

        public LoopManager()
        {
            _loopIterations = new Dictionary<string, int>();
        }

        public LoopType GetTypeFromKeyword(string keyword)
        {
            return keyword switch
            {
                "for" => LoopType.For,
                "while" => LoopType.While,
                "do" => LoopType.DoWhile,
                _ => throw new ArgumentException("Unsupported loop type")
            };
        }

        public string GetKey(string keyword, string location)
        {
            return $"{GetTypeFromKeyword(keyword)}-{location}";
        }

        public int GetLoopIteration(string keyword, string location)
        {
            return _loopIterations[GetKey(keyword, location)];
        }

        public void UpdateLoopIteration(string keyword, string location)
        {
            var key = GetKey(keyword, location);
            if (_loopIterations.ContainsKey(key))
            {
                _loopIterations[key]++;
                if (_loopIterations[key] > MaxIter)
                {
                    var range = location.Split("-");
                    var startLine = range.First().Split(',');
                    var line = int.Parse(startLine.First());
                    var @char = int.Parse(startLine.Last());
                    throw new NumIterationsException
                    (
                        _loopIterations[key],
                        MaxIter,
                        GetTypeFromKeyword(key),
                        line,
                        @char
                    );
                }
            }
            else
            {
                _loopIterations.Add(key, 1);
            }
        }
    }
}