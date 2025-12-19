using System.Numerics;
using CellularAutomata.NET;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Wolfram3D.Rendering;

namespace Wolfram3D
{
    // Reference: https://github.com/dotnet/Silk.NET/tree/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%202.2%20-%20Camera
    // Textures: https://opengameart.org/content/tileable-bricks-ground-textures-set-1
    internal class Program
    {
        private static List<Vector<int>> aboveLeft = AutomataVector.Extrude(
            AutomataVector.Create(-1, -1, 0),
            -1,
            1,
            3
        );
        private static List<Vector<int>> above = AutomataVector.Extrude(
            AutomataVector.Create(0, -1, 0),
            -1,
            1,
            3
        );
        private static List<Vector<int>> aboveRight = AutomataVector.Extrude(
            AutomataVector.Create(1, -1, 0),
            -1,
            1,
            3
        );

        public static readonly Action<
            CellularAutomataCell<int>,
            Dictionary<Vector<int>, CellularAutomataCell<int>>,
            CellularAutomata<int>
        > Rules = new Action<
            CellularAutomataCell<int>,
            Dictionary<Vector<int>, CellularAutomataCell<int>>,
            CellularAutomata<int>
        >(
            (cell, neighbors, automata) =>
            {
                if (neighbors.Count == 0)
                {
                    return;
                }
                int leftNeighborValue = 0;
                foreach (var leftNeighbor in aboveLeft)
                {
                    leftNeighborValue = Math.Max(
                        leftNeighborValue,
                        neighbors.TryGetValue(leftNeighbor, out var leftCell) ? leftCell.State : 0
                    );
                }
                int rightNeighborValue = 0;
                foreach (var rightNeighbor in aboveRight)
                {
                    rightNeighborValue = Math.Max(
                        rightNeighborValue,
                        neighbors.TryGetValue(rightNeighbor, out var rightCell)
                            ? rightCell.State
                            : 0
                    );
                }

                int centerNeighborValue = 0;
                foreach (var centerNeighbor in above)
                {
                    centerNeighborValue = Math.Max(
                        centerNeighborValue,
                        neighbors.TryGetValue(centerNeighbor, out var centerCell)
                            ? centerCell.State
                            : 0
                    );
                }
                // https://en.wikipedia.org/wiki/Rule_30#Rule_set
                if (leftNeighborValue == 1 && centerNeighborValue == 0 && rightNeighborValue == 0)
                {
                    cell.SetState(1);
                }
                else if (
                    leftNeighborValue == 0
                    && centerNeighborValue == 1
                    && rightNeighborValue == 1
                )
                {
                    cell.SetState(1);
                }
                else if (
                    leftNeighborValue == 0
                    && centerNeighborValue == 1
                    && rightNeighborValue == 0
                )
                {
                    cell.SetState(1);
                }
                else if (
                    leftNeighborValue == 0
                    && centerNeighborValue == 0
                    && rightNeighborValue == 1
                )
                {
                    cell.SetState(1);
                }
                else
                {
                    cell.SetState(0);
                }
            }
        );

        static void Main(string[] args)
        {
            CellularAutomataConfiguration<int> config = new CellularAutomataConfiguration<int>
            {
                Dimensions = new CellularAutomataDimension[]
                {
                    new CellularAutomataDimension() { Cells = 128 },
                    new CellularAutomataDimension() { Cells = 32 },
                    new CellularAutomataDimension() { Cells = 128 },
                },
                DefaultState = 0,
            };

            List<Vector<int>> extrudedNeighborhood = new List<Vector<int>>();
            extrudedNeighborhood.AddRange(aboveLeft);
            extrudedNeighborhood.AddRange(above);
            extrudedNeighborhood.AddRange(aboveRight);
            CellularAutomataNeighborhood<int> neighborhood = new CellularAutomataNeighborhood<int>()
            {
                NeighborLocations = extrudedNeighborhood.ToArray(),
                ClearNeighborsAfterUse = true,
            };

            CellularAutomata<int> automaton = new CellularAutomata<int>(
                config,
                neighborhood,
                Rules
            );

            Dictionary<System.Numerics.Vector<int>, int> initialState = new Dictionary<
                System.Numerics.Vector<int>,
                int
            >()
            {
                { AutomataVector.Create(64, 0, 64), 1 },
            };

            automaton.InitializeGrid(initialState);
            for (int i = 0; i < 32; i++)
            {
                Console.Write($"\rStep {i + 1}/32");
                automaton.Step();
            }

            Wolfram3DWindow window = new Wolfram3DWindow(800, 800, automaton.Grid, 0.01f);
            window.Start();
        }
    }
}
