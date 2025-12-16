using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CellularAutomata.NET
{
    public record CellularAutomataNeighborhood<T>
        where T : notnull
    {
        public static readonly CellularAutomataNeighborhood<T> MooreNeighborhood =
            new CellularAutomataNeighborhood<T>()
            {
                NeighborLocations = new Vector<int>[8]
                {
                    // Above
                    AutomataVector.Create(-1, -1),
                    AutomataVector.Create(0, -1),
                    AutomataVector.Create(1, -1),
                    // Sides
                    AutomataVector.Create(-1, 0),
                    AutomataVector.Create(1, 0),
                    // Below
                    AutomataVector.Create(-1, 1),
                    AutomataVector.Create(0, 1),
                    AutomataVector.Create(1, 1),
                },
            };

        public static readonly CellularAutomataNeighborhood<T> VonNeumannNeighborhood =
            new CellularAutomataNeighborhood<T>()
            {
                NeighborLocations = new Vector<int>[4]
                {
                    // Up
                    AutomataVector.Create(0, -1),
                    // Down
                    AutomataVector.Create(-1, 0),
                    // Left
                    AutomataVector.Create(1, 0),
                    // Right
                    AutomataVector.Create(0, 1),
                },
            };

        public required Vector<int>[] NeighborLocations { get; set; }
        public bool WrapColumns { get; set; } = false;
        public bool WrapRows { get; set; } = false;
        public bool ClearNeighborsAfterUse { get; set; } = true;

        public void GetNeighbors(
            Vector<int> position,
            CellularAutomataGrid<T> grid,
            Dictionary<Vector<int>, CellularAutomataCell<T>> neighbors
        )
        {
            if (ClearNeighborsAfterUse)
            {
                neighbors.Clear();
            }

            for (int i = 0; i < NeighborLocations.Length; i++)
            {
                Vector<int> neighborPos = position + NeighborLocations[i];
                if (WrapColumns && (neighborPos.GetX() >= grid.Width || neighborPos.GetX() < 0))
                {
                    neighborPos = AutomataVector.Create(
                        neighborPos.GetX() % grid.Width,
                        neighborPos.GetY()
                    );
                }
                if (WrapRows && (neighborPos.GetY() >= grid.Height || neighborPos.GetY() < 0))
                {
                    neighborPos = AutomataVector.Create(
                        neighborPos.GetX(),
                        neighborPos.GetY() % grid.Height
                    );
                }
                if (IsValidPosition(neighborPos, grid))
                {
                    neighbors.Add(NeighborLocations[i], grid.State[neighborPos]);
                }
            }
        }

        private static bool IsValidPosition(Vector<int> pos, CellularAutomataGrid<T> grid)
        {
            return pos.GetX() >= 0
                && pos.GetY() >= 0
                && pos.GetX() < grid.Width
                && pos.GetY() < grid.Height;
        }
    }
}
