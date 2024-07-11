using System;
using System.Collections.Generic;
using System.IO;

namespace WatchTower
{
    public class DataEntropyCalculatorUtf8
    {
        // Stores the number of times each symbol appears
        private SortedList<byte, int> _distributionDict;

        // Stores the entropy for each character
        private SortedList<byte, double> _probabilityDict;

        // Stores the last calculated entropy
        private double _overallEntropy;

        // Used for preventing unnecessary processing
        private bool _isDirty;

        // Bytes of data processed
        private int _dataSize;


        public double Entropy => GetEntropy();


        public DataEntropyCalculatorUtf8(byte[] buff)
        {
            Clear();
            if (buff == null)
                return;


            ExamineChunk(buff);
            GetEntropy();
            GetSortedDistribution();

        }

        public DataEntropyCalculatorUtf8(string fileName)
        {
            Clear();

            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return;


            ExamineChunk(File.ReadAllBytes(fi.FullName));
            GetEntropy();
            GetSortedDistribution();
        }

        public DataEntropyCalculatorUtf8()
        {
            Clear();
        }


        public Dictionary<byte, int> GetSortedDistribution()
        {
            var entryList = new List<Tuple<int, byte>>();
            foreach (var entry in _distributionDict)
            {
                entryList.Add(new Tuple<int, byte>(entry.Value, entry.Key));
            }

            entryList.Sort();
            entryList.Reverse();

            var result = new Dictionary<byte, int>();
            foreach (var entry in entryList)
            {
                result.Add(entry.Item2, entry.Item1);
            }

            return result;
        }


        private double GetEntropy()
        {
            // If nothing has changed, dont recalculate
            if (!_isDirty)
            {
                return _overallEntropy;
            }

            // Reset values
            _overallEntropy = 0;
            _probabilityDict = new SortedList<byte, double>();

            foreach (var entry in _distributionDict)
            {
                // Probability = Freq of symbol / # symbols examined thus far
                _probabilityDict.Add(
                    entry.Key,
                    _distributionDict[entry.Key] / (double)_dataSize
                );
            }

            foreach (var entry in _probabilityDict)
            {
                // Entropy = probability * Log2(1/probability)
                _overallEntropy += entry.Value * Math.Log((1 / entry.Value), 2);
            }

            _isDirty = false;
            _overallEntropy = Math.Round(_overallEntropy, 3);
            return _overallEntropy;
        }

        private void ExamineChunk(byte[] chunk)
        {
            if (chunk.Length < 1)
            {
                return;
            }

            _isDirty = true;
            _dataSize += chunk.Length;

            foreach (var bite in chunk)
            {
                if (!_distributionDict.ContainsKey(bite))
                {
                    _distributionDict.Add(bite, 1);
                    continue;
                }

                _distributionDict[bite]++;
            }
        }


        private void Clear()
        {
            _isDirty = true;
            _overallEntropy = 0;
            _dataSize = 0;
            _distributionDict = new SortedList<byte, int>();
            _probabilityDict = new SortedList<byte, double>();
        }

    }
}
