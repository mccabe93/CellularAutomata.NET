using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NET
{
    public class CellularAutomataGrid<T>
        where T : notnull
    {
        public readonly CellularAutomataDimension[] Dimensions;
        public readonly ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> State;

        public CellularAutomataGrid(CellularAutomataDimension[] dimensions, T defaultState)
        {
            Dimensions = dimensions;
            State = CreateState(dimensions, defaultState);
        }

        public CellularAutomataGrid(
            CellularAutomataDimension[] dimensions,
            ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> existingState
        )
        {
            Dimensions = dimensions;
            State = CreateState(existingState);
        }

        public T GetCellValue(Vector<int> position)
        {
            return State[position].State;
        }

        public void SetCellValue(T value, Vector<int> position)
        {
            State[position].SetState(value);
        }

        public CellularAutomataCell<T>? GetCell(Vector<int> position)
        {
            if (AutomataVector.IsInBounds(position, Dimensions))
            {
                return State[position];
            }
            return null;
        }

        internal void UpdateGridStates()
        {
            foreach (var cell in State.Values)
            {
                cell.UpdateState();
            }
        }

        public CellularAutomataGrid<T> Copy()
        {
            return new CellularAutomataGrid<T>(Dimensions, State);
        }

        /// <summary>
        /// Note that this will flatten the output into a single line string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var orderedCells = State.Values.OrderBy(c =>
            {
                int magnitude = 0;
                for (int i = 0; i < Dimensions.Length; i++)
                {
                    magnitude +=
                        c.Position.GetElement(i) * (int)Math.Pow(AutomataVector.SIMD_SIZE, i);
                }
                return magnitude;
            });
            foreach (var cell in orderedCells)
            {
                sb.Append(cell.State.ToString());
            }
            return sb.ToString();
        }

        private static List<Vector<int>> CreateDimension(
            CellularAutomataDimension[] dimensions,
            int currentDimension,
            Vector<int> currentVector,
            List<Vector<int>> vectors
        )
        {
            if (currentDimension >= dimensions.Length)
            {
                vectors.Add(currentVector);
                return vectors;
            }
            for (int i = 0; i < dimensions[currentDimension].Cells; i++)
            {
                Vector<int> newVector = AutomataVector.Create(dimensions.Length);
                for (int d = 0; d < dimensions.Length; d++)
                {
                    if (d == currentDimension)
                    {
                        continue;
                    }
                    newVector = newVector.WithElement(d, currentVector.GetElement(d));
                }
                newVector = newVector.WithElement(currentDimension, i);
                CreateDimension(dimensions, currentDimension + 1, newVector, vectors);
            }
            return vectors;
        }

        private static ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> CreateState(
            CellularAutomataDimension[] dimensions,
            T defaultState
        )
        {
            if (dimensions == null || dimensions.Length == 0)
            {
                throw new ArgumentException("Dimensions must be greater than zero.");
            }
            // Algorithm for n-dimensional grid creation using x,y,z example...
            // Note: we replicate nested for loops with recursion, since we do not know the number of dimensions at compile time.
            // recursively call =>
            //                  if currentDimension >= dimensions.Length
            //                  for loop from 0 to longest dimension, filling with the last value of the current dimension if exceeded
            // [4, 3, 2]
            // Desired output:
            // x,y
            // [0,0,0], [1,0,0], [2,0,0], [3,0,0]
            // [0,1,0], [1,1,0], [2,1,0], [3,1,0]
            // [0,2,0], [1,2,0], [2,2,0], [3,2,0]
            // z(1)
            // [0,0,1], [1,0,1], [2,0,1], [3,0,1]
            // [0,1,1], [1,1,1], [2,1,1], [3,1,1]
            // [0,2,1], [1,2,1], [2,2,1], [3,2,1]
            // [0,3,1], [1,3,1], [2,3,1], [3,3,1]
            //
            // Recursive logic:
            // Longest dimension is 4
            // Start with dimension 0 (x)
            // -> r(x)
            //    for i = 0 ... 3
            //        create vector <0,0,0>
            //         -> r(y)
            //            for j = 0 ... 2
            //                create vector <0,1,0>
            //                 -> r(z)
            //                    for k = 0 ... 1
            //                        create vectors <0,1,0> .. <0,1,1> (if k >= 2, use 1)
            //                        r(a) -> a >= dimensions -> add to list
            //                        ...
            //                  <- r(z) <0,0,0>, <0,0,1>
            //          <- r(y) <0,0,0>, <0,1,0>, <0,2,0>, (and z values) <0,0,1>, <0,1,1>, <0,2,1>
            //        create vector <1,0,0>
            //          <- r(y) <1,0,0>, <1,1,0>, <1,2,0>, <1,0,1>, <1,1,1>, <1,2,1>
            //        ...
            var state = ImmutableDictionary.CreateBuilder<Vector<int>, CellularAutomataCell<T>>();
            List<Vector<int>> vectors = CreateDimension(
                dimensions,
                0,
                AutomataVector.Create(dimensions.Length),
                new List<Vector<int>>()
            );
            foreach (var vector in vectors)
            {
                state.Add(vector, new CellularAutomataCell<T>(defaultState) { Position = vector });
            }
            return state.ToImmutable();
        }

        private static ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> CreateState(
            ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> existingState
        )
        {
            var state = ImmutableDictionary.CreateBuilder<Vector<int>, CellularAutomataCell<T>>();
            foreach (var kvp in existingState)
            {
                state.Add(kvp.Key, new CellularAutomataCell<T>(kvp.Value.State));
            }
            return state.ToImmutable();
        }
    }
}
