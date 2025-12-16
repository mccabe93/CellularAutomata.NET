using System;
using System.Collections.Generic;
using System.Text;

namespace CellularAutomata.NET
{
    public class CellularAutomataConfiguration<T>
    {
        /// <summary>
        /// Width of the cellular automata grid.
        /// </summary>
        public int Width { get; set; } = 128;

        /// <summary>
        /// Height of the cellular automata grid.
        /// </summary>
        public int Height { get; set; } = 128;

        /// <summary>
        /// Default state for all cells in the grid.
        /// </summary>
        public required T DefaultState { get; set; }

        /// <summary>
        /// Whether the last cell in each row connects to the first cell.
        /// </summary>
        public bool WrappedRows { get; set; }

        /// <summary>
        /// Whether the last cell in each column connects to the first cell.
        /// </summary>
        public bool WrappedColumns { get; set; }

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
}
