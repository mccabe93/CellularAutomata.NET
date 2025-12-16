using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NET
{
    public static class AutomataVector
    {
        private static readonly int _simdSize = Vector<int>.Count;

        public static Vector<int> Create(params int[] vals)
        {
            if (vals.Length == 0)
            {
                throw new ArgumentException(
                    "At least one value must be provided to create a Vector."
                );
            }
            if (vals.Length > _simdSize)
            {
                throw new ArgumentException($"Exceeded CPU's SIMD size {_simdSize}.");
            }
            int[] dimensionValues = new int[_simdSize];
            for (int i = 0; i < vals.Length; i++)
            {
                dimensionValues[i] = vals[i];
            }
            return new Vector<int>(dimensionValues);
        }
    }

    public static class VectorExtensions
    {
        public static int GetX(this Vector<int> vector) => vector[0];

        public static int GetY(this Vector<int> vector) => vector[1];
    }
}
