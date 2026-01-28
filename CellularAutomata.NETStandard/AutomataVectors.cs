using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CellularAutomata.NETStandard
{
    public class AutomataVector
    {
        public static readonly int SIMD_SIZE = Vector<int>.Count;

        public static Vector<int> Create(params int[] vals)
        {
            if (vals.Length == 0)
            {
                throw new ArgumentException(
                    "At least one value must be provided to create a Vector."
                );
            }
            if (vals.Length > SIMD_SIZE)
            {
                throw new ArgumentException($"Exceeded CPU's SIMD size {SIMD_SIZE}.");
            }
            int[] dimensionValues = new int[SIMD_SIZE];
            for (int i = 0; i < vals.Length; i++)
            {
                dimensionValues[i] = vals[i];
            }
            return new Vector<int>(dimensionValues);
        }

        public static Vector<int> Clone(Vector<int> existingVector)
        {
            int[] dimensionValues = new int[SIMD_SIZE];
            existingVector.CopyTo(dimensionValues);
            return new Vector<int>(dimensionValues);
        }

        public static Vector<int> Modify(
            Vector<int> existingVector,
            int[] indicesToChange,
            int[] values
        )
        {
            int[] dimensionValues = new int[SIMD_SIZE];
            existingVector.CopyTo(dimensionValues);
            for (int i = 0; i < indicesToChange.Length; i++)
            {
                int index = indicesToChange[i];
                dimensionValues[index] = values[i];
            }
            return new Vector<int>(dimensionValues);
        }

        public static bool IsInBounds(Vector<int> vector, int[] dimensions)
        {
            for (int i = 0; i < dimensions.Length; i++)
            {
                int value = vector[i];
                if (value < 0 || value >= dimensions[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsInBounds(Vector<int> vector, AutomataDimension[] dimensions)
        {
            for (int i = 0; i < dimensions.Length; i++)
            {
                int value = vector[i];
                if (value < 0 || value >= dimensions[i].Cells)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds depth to specified dimensions of a vector over a range.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="startDepth">Start depth value for the dimension</param>
        /// <param name="endDepth">End depth value for the dimension</param>
        /// <param name="dimensionsToExtrude">1-based index. e.g 3rd dimension = 3</param>
        /// <returns></returns>
        public static List<Vector<int>> Extrude(
            Vector<int> vector,
            int startDepth,
            int endDepth,
            params int[] dimensionsToExtrude
        )
        {
            List<Vector<int>> extrudedVectors = new List<Vector<int>>();
            for (int i = startDepth; i <= endDepth; i++)
            {
                foreach (var d in dimensionsToExtrude)
                {
                    vector = WithElement(vector, d - 1, i);
                }
                extrudedVectors.Add(vector);
            }
            return extrudedVectors;
        }

        public static Vector<int> WithElement(Vector<int> vector, int elementIndex, int newValue)
        {
            return AutomataVector.Modify(
                vector,
                new int[] { elementIndex },
                new int[] { newValue }
            );
        }
    }
}
