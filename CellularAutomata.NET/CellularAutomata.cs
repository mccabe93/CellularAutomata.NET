using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NET
{
    public class CellularAutomata<T>(
        CellularAutomataConfiguration<T> configuration,
        CellularAutomataNeighborhood<T> neighborhood,
        Action<
            CellularAutomataCell<T>,
            Dictionary<Vector<int>, CellularAutomataCell<T>>,
            CellularAutomata<T>
        > rules
    )
        where T : notnull
    {
        public readonly Action<
            CellularAutomataCell<T>,
            Dictionary<Vector<int>, CellularAutomataCell<T>>,
            CellularAutomata<T>
        > Rules = rules;

        public readonly CellularAutomataGrid<T> Grid = new CellularAutomataGrid<T>(
            configuration.Dimensions,
            configuration.DefaultState
        );

        public readonly CellularAutomataNeighborhood<T> Neighborhood = neighborhood;

        private readonly List<CellularAutomataGrid<T>> GridHistory =
            new List<CellularAutomataGrid<T>>();

        /// <summary>
        /// Cells are 'visible' when they are either a non-default cell or a neighbor of one.
        /// These are cells that can change.
        /// </summary>
        private readonly HashSet<CellularAutomataCell<T>> _visibleCells =
            new HashSet<CellularAutomataCell<T>>();

        private readonly HashSet<CellularAutomataCell<T>> _visibleCellsCopy =
            new HashSet<CellularAutomataCell<T>>();

        public void InitializeGrid(Dictionary<Vector<int>, T> stateMap)
        {
            foreach (var kvp in stateMap)
            {
                Vector<int> pos = kvp.Key;
                T stateValue = kvp.Value;
                for (int i = 0; i < configuration.Dimensions.Length; i++)
                {
                    if (
                        kvp.Key.GetElement(i) < 0
                        || kvp.Key.GetElement(i) >= configuration.Dimensions[i].Cells
                    )
                    {
                        throw new ArgumentException(
                            $"Position {pos} is out of bounds for the grid."
                        );
                    }
                }
                Grid.State[pos].SetState(stateValue);
                AddVisibleCell(Grid.State[pos]);
            }
            Grid.UpdateGridStates();
        }

        public CellularAutomataGrid<T> Step()
        {
            _visibleCellsCopy.Clear();
            foreach (var cell in _visibleCells)
            {
                _visibleCellsCopy.Add(cell);
            }
            foreach (var cell in _visibleCellsCopy)
            {
                cell.Neighbors.Clear();
                Neighborhood.GetNeighbors(cell.Position, Grid, cell.Neighbors);
                cell.StateHistory.Add(cell.State);
                Rules.Invoke(cell, cell.Neighbors, this);
                AddVisibleCell(cell);
            }
            RemoveUnchangedVisibleCells(_visibleCellsCopy);
            Grid.UpdateGridStates();
            if (configuration.KeepAnyGridStates)
            {
                GridHistory.Add(Grid.Copy());
                if (
                    !configuration.KeepAllGridStates
                    && GridHistory.Count > configuration.GridStatesMemoryLimit
                )
                {
                    GridHistory.RemoveAt(0);
                }
            }
            else if (configuration.KeepAllGridStates)
            {
                throw new InvalidOperationException(
                    "KeepAllStates is true but KeepAnyStates is false. Please ensure KeepAnyStates is true if KeepAllStates is true."
                );
            }
            return Grid;
        }

        private void AddVisibleCell(CellularAutomataCell<T> cell)
        {
            if (!_visibleCells.Contains(cell))
            {
                _visibleCells.Add(cell);
            }
            foreach (Vector<int> position in Neighborhood.NeighborLocations)
            {
                CellularAutomataCell<T>? neighbor = Grid.GetCell(
                    cell.GetNeighborGlobalPosition(position * -1) // Position inverse gives us the projected possible neighbor.
                );
                if (neighbor != null && !_visibleCells.Contains(neighbor))
                {
                    _visibleCells.Add(neighbor);
                }
            }
        }

        private void RemoveUnchangedVisibleCells(HashSet<CellularAutomataCell<T>> visibleCells)
        {
            foreach (var cell in visibleCells)
            {
                if (!cell.StateChanged)
                {
                    bool canRemove = true;
                    foreach (Vector<int> position in Neighborhood.NeighborLocations)
                    {
                        // If I didn't change and none of my neighbors changed you can ignore me next step.
                        CellularAutomataCell<T>? neighbor = Grid.GetCell(
                            cell.GetNeighborGlobalPosition(position)
                        );
                        if (neighbor != null && neighbor.StateChanged)
                        {
                            canRemove = false;
                            break;
                        }
                    }
                    if (canRemove)
                    {
                        _visibleCells.Remove(cell);
                    }
                }
            }
        }
    }
}
