using System.Collections.Generic;
using System.Numerics;

namespace CellularAutomata.NETStandard
{
    public class AutomataNeighborhood<T>
        where T : notnull
    {
        public static readonly AutomataNeighborhood<T> MooreNeighborhood =
            new AutomataNeighborhood<T>()
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

        public static readonly AutomataNeighborhood<T> VonNeumannNeighborhood =
            new AutomataNeighborhood<T>()
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

        public Vector<int>[] NeighborLocations { get; set; }
        public bool ClearNeighborsAfterUse { get; set; } = true;

        public void GetNeighbors(
            Vector<int> position,
            AutomataGrid<T> grid,
            Dictionary<Vector<int>, AutomataCell<T>> neighbors
        )
        {
            if (ClearNeighborsAfterUse)
            {
                neighbors.Clear();
            }

            for (int i = 0; i < NeighborLocations.Length; i++)
            {
                Vector<int> neighborPos = position + NeighborLocations[i];

                for (int d = 0; d < grid.Dimensions.Length; d++)
                {
                    AutomataDimension dimension = grid.Dimensions[d];
                    if (dimension.WrapStart && neighborPos[d] < 0)
                    {
                        neighborPos = AutomataVector.WithElement(
                            neighborPos,
                            d,
                            dimension.Cells - 1
                        );
                    }
                    if (dimension.WrapEnd && neighborPos[d] >= dimension.Cells)
                    {
                        neighborPos = AutomataVector.WithElement(neighborPos, d, 0);
                    }
                }
                if (IsValidPosition(neighborPos, grid))
                {
                    neighbors.Add(NeighborLocations[i], grid.State[neighborPos]);
                }
            }
        }

        private static bool IsValidPosition(Vector<int> pos, AutomataGrid<T> grid)
        {
            return AutomataVector.IsInBounds(pos, grid.Dimensions);
        }
    }
}
