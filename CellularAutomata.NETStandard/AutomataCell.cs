using System.Collections.Generic;
using System.Numerics;

namespace CellularAutomata.NETStandard
{
    public class AutomataCell<T>
        where T : notnull
    {
        public T State { get; private set; }
        public Vector<int> Position { get; set; }
        public Dictionary<Vector<int>, AutomataCell<T>> Neighbors { get; } =
            new Dictionary<Vector<int>, AutomataCell<T>>();
        public List<T> StateHistory { get; set; } = new List<T>();

        internal bool StateChanged { get; private set; }
        internal T NewState { get; private set; }

        public AutomataCell(T startState)
        {
            State = startState;
            NewState = startState;
        }

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
