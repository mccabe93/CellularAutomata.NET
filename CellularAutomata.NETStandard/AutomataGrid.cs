using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NETStandard
{
    public class AutomataGrid<T>
        where T : notnull
    {
        public readonly AutomataDimension[] Dimensions;
        public readonly Dictionary<Vector<int>, AutomataCell<T>> State;

        public AutomataGrid(AutomataDimension[] dimensions, T defaultState)
        {
            Dimensions = dimensions;
            State = CreateState(dimensions, defaultState);
        }

        public AutomataGrid(
            AutomataDimension[] dimensions,
            Dictionary<Vector<int>, AutomataCell<T>> existingState
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

        public AutomataCell<T>? GetCell(Vector<int> position)
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

        public AutomataGrid<T> Copy()
        {
            return new AutomataGrid<T>(Dimensions, State);
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
                    magnitude += c.Position[i] * (int)Math.Pow(AutomataVector.SIMD_SIZE, i);
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
            AutomataDimension[] dimensions,
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
                    newVector = AutomataVector.WithElement(newVector, d, currentVector[d]);
                }
                newVector = AutomataVector.WithElement(newVector, currentDimension, i);
                CreateDimension(dimensions, currentDimension + 1, newVector, vectors);
            }
            return vectors;
        }

        private static Dictionary<Vector<int>, AutomataCell<T>> CreateState(
            AutomataDimension[] dimensions,
            T defaultState
        )
        {
            if (dimensions == null || dimensions.Length == 0)
            {
                throw new ArgumentException("Dimensions must be greater than zero.");
            }
            Dictionary<Vector<int>, AutomataCell<T>> state =
                new Dictionary<Vector<int>, AutomataCell<T>>();
            List<Vector<int>> vectors = CreateDimension(
                dimensions,
                0,
                AutomataVector.Create(dimensions.Length),
                new List<Vector<int>>()
            );
            foreach (var vector in vectors)
            {
                state.Add(vector, new AutomataCell<T>(defaultState) { Position = vector });
            }
            return state;
        }

        private static Dictionary<Vector<int>, AutomataCell<T>> CreateState(
            Dictionary<Vector<int>, AutomataCell<T>> existingState
        )
        {
            var state = new Dictionary<Vector<int>, AutomataCell<T>>();
            foreach (var kvp in existingState)
            {
                state.Add(kvp.Key, new AutomataCell<T>(kvp.Value.State));
            }
            return state;
        }
    }
}
