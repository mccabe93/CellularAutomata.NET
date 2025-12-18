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
        private static Vector<int> aboveLeft = AutomataVector.Create(-1, -1, 0);
        private static Vector<int> above = AutomataVector.Create(0, -1, 0);
        private static Vector<int> aboveRight = AutomataVector.Create(1, -1, 0);

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
                int leftNeighbor = neighbors.TryGetValue(aboveLeft, out var leftCell)
                    ? leftCell.State
                    : 0;
                int centerNeighbor = neighbors.TryGetValue(above, out var centerCell)
                    ? centerCell.State
                    : 0;
                int rightNeighbor = neighbors.TryGetValue(aboveRight, out var rightCell)
                    ? rightCell.State
                    : 0;
                // https://en.wikipedia.org/wiki/Rule_30#Rule_set
                if (leftNeighbor == 1 && centerNeighbor == 0 && rightNeighbor == 0)
                {
                    cell.SetState(1);
                }
                else if (leftNeighbor == 0 && centerNeighbor == 1 && rightNeighbor == 1)
                {
                    cell.SetState(1);
                }
                else if (leftNeighbor == 0 && centerNeighbor == 1 && rightNeighbor == 0)
                {
                    cell.SetState(1);
                }
                else if (leftNeighbor == 0 && centerNeighbor == 0 && rightNeighbor == 1)
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
            CellularAutomataNeighborhood<int> neighborhood = new CellularAutomataNeighborhood<int>()
            {
                NeighborLocations = new System.Numerics.Vector<int>[]
                {
                    aboveLeft,
                    above,
                    aboveRight,
                },
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
                { AutomataVector.Create(64, 0, 0), 1 },
                { AutomataVector.Create(0, 0, 64), 1 },
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
