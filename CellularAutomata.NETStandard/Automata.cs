using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CellularAutomata.NETStandard
{
    public class Automata<T>
        where T : notnull
    {
        public readonly Action<
            AutomataCell<T>,
            Dictionary<Vector<int>, AutomataCell<T>>,
            Automata<T>
        > Rules;

        public readonly AutomataGrid<T> Grid;

        public readonly AutomataNeighborhood<T> Neighborhood;

        private readonly List<AutomataGrid<T>> GridHistory = new List<AutomataGrid<T>>();

        /// <summary>
        /// Cells are 'visible' when they are either a non-default cell or a neighbor of one.
        /// These are cells that can change.
        /// </summary>
        private readonly HashSet<AutomataCell<T>> _visibleCells = new HashSet<AutomataCell<T>>();

        private readonly HashSet<AutomataCell<T>> _visibleCellsCopy =
            new HashSet<AutomataCell<T>>();

        private readonly AutomataConfiguration<T> Configuration;

        public Automata(
            AutomataConfiguration<T> configuration,
            AutomataNeighborhood<T> neighborhood,
            Action<AutomataCell<T>, Dictionary<Vector<int>, AutomataCell<T>>, Automata<T>> rules
        )
        {
            Configuration = configuration;
            Rules = rules;
            Neighborhood = neighborhood;
            Grid = new AutomataGrid<T>(configuration.Dimensions, configuration.DefaultState);
        }

        public void InitializeGrid(Dictionary<Vector<int>, T> stateMap)
        {
            foreach (var kvp in stateMap)
            {
                Vector<int> pos = kvp.Key;
                T stateValue = kvp.Value;
                for (int i = 0; i < Configuration.Dimensions.Length; i++)
                {
                    if (kvp.Key[i] < 0 || kvp.Key[i] >= Configuration.Dimensions[i].Cells)
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

        public AutomataGrid<T> Step()
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
            if (Configuration.KeepAnyGridStates)
            {
                GridHistory.Add(Grid.Copy());
                if (
                    !Configuration.KeepAllGridStates
                    && GridHistory.Count > Configuration.GridStatesMemoryLimit
                )
                {
                    GridHistory.RemoveAt(0);
                }
            }
            else if (Configuration.KeepAllGridStates)
            {
                throw new InvalidOperationException(
                    "KeepAllStates is true but KeepAnyStates is false. Please ensure KeepAnyStates is true if KeepAllStates is true."
                );
            }
            return Grid;
        }

        private void AddVisibleCell(AutomataCell<T> cell)
        {
            if (!_visibleCells.Contains(cell))
            {
                _visibleCells.Add(cell);
            }
            foreach (Vector<int> position in Neighborhood.NeighborLocations)
            {
                AutomataCell<T>? neighbor = Grid.GetCell(
                    cell.GetNeighborGlobalPosition(position * -1) // Position inverse gives us the projected possible neighbor.
                );
                if (neighbor != null && !_visibleCells.Contains(neighbor))
                {
                    _visibleCells.Add(neighbor);
                }
            }
        }

        private void RemoveUnchangedVisibleCells(HashSet<AutomataCell<T>> visibleCells)
        {
            foreach (var cell in visibleCells)
            {
                if (!cell.StateChanged)
                {
                    bool canRemove = true;
                    foreach (Vector<int> position in Neighborhood.NeighborLocations)
                    {
                        // If I didn't change and none of my neighbors changed you can ignore me next step.
                        AutomataCell<T>? neighbor = Grid.GetCell(
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
