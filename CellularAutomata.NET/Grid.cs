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
        public readonly int Width;
        public readonly int Height;
        public readonly ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> State;

        public CellularAutomataGrid(int width, int height, T defaultState)
        {
            Width = width;
            Height = height;
            State = CreateState(width, height, defaultState);
        }

        public CellularAutomataGrid(
            int width,
            int height,
            ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> existingState
        )
        {
            Width = width;
            Height = height;
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
            if (
                position.GetX() < 0
                || position.GetY() < 0
                || position.GetX() >= Width
                || position.GetY() >= Height
            )
            {
                return null;
            }
            return State[position];
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
            return new CellularAutomataGrid<T>(Width, Height, State);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    sb.Append(State[AutomataVector.Create(y, x)].State);
                    if (y < Width - 1)
                    {
                        sb.Append(" ");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static ImmutableDictionary<Vector<int>, CellularAutomataCell<T>> CreateState(
            int width,
            int height,
            T defaultState
        )
        {
            var state = ImmutableDictionary.CreateBuilder<Vector<int>, CellularAutomataCell<T>>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector<int> position = AutomataVector.Create(x, y);
                    state.Add(
                        position,
                        new CellularAutomataCell<T>(defaultState) { Position = position }
                    );
                }
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
