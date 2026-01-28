using System;
using System.Collections.Generic;
using System.Text;

namespace CellularAutomata.NETStandard
{
    public class AutomataConfiguration<T>
    {
        /// <summary>
        /// Dimensions of the cellular automata grid.
        /// </summary>
        public AutomataDimension[] Dimensions { get; set; }

        /// <summary>
        /// Default state for all cells in the grid.
        /// </summary>
        public T DefaultState { get; set; }

        /// <summary>
        /// Whether to keep all previous states in memory.
        /// </summary>
        public bool KeepAllGridStates { get; set; }

        /// <summary>
        /// Whether to keep any previous states in memory.
        /// </summary>
        public bool KeepAnyGridStates { get; set; }

        /// <summary>
        /// If KeepAllStates is false, the maximum number of previous states to keep in memory.
        /// </summary>
        public int GridStatesMemoryLimit { get; set; } = 10;
    }

    public class AutomataDimension
    {
        /// <summary>
        /// The number of cells in / size of the dimension.
        /// </summary>
        public int Cells { get; set; }

        /// <summary>
        /// Whether the dimension should wrap from end to start.
        /// </summary>
        public bool WrapEnd { get; set; }

        /// <summary>
        /// Whether the dimension should wrap from start to end.
        /// </summary>
        public bool WrapStart { get; set; }
    }
}
