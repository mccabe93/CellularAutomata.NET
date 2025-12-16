using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NET
{
    public class CellularAutomataCell<T>(T startState)
        where T : notnull
    {
        public T State { get; private set; } = startState;
        public Vector<int> Position { get; set; }
        public int X => Position.GetX();
        public int Y => Position.GetY();
        public Dictionary<Vector<int>, CellularAutomataCell<T>> Neighbors { get; } =
            new Dictionary<Vector<int>, CellularAutomataCell<T>>();
        public List<T> StateHistory { get; set; } = new List<T>();

        internal bool StateChanged { get; private set; }
        internal T NewState { get; private set; } = startState;

        public Vector<int> GetNeighborGlobalPosition(Vector<int> relativePosition)
        {
            return Position + relativePosition;
        }

        public void SetState(T newState)
        {
            NewState = newState;
            if (!NewState.Equals(State))
            {
                StateChanged = true;
            }
        }

        internal void UpdateState()
        {
            State = NewState;
            StateChanged = false;
        }
    }
}
