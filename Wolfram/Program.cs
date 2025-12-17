using System.Numerics;
using System.Text;
using CellularAutomata.NET;

namespace Wolfram
{
    internal class Program
    {
        private static Vector<int> aboveLeft = AutomataVector.Create(-1, -1);
        private static Vector<int> above = AutomataVector.Create(0, -1);
        private static Vector<int> aboveRight = AutomataVector.Create(1, -1);

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
                    new CellularAutomataDimension() { Cells = 1024 },
                    new CellularAutomataDimension() { Cells = 256 },
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
                { AutomataVector.Create(512, 0), 1 },
            };

            automaton.InitializeGrid(initialState);
            for (int i = 0; i < 256; i++)
            {
                Console.Write($"\rStep {i + 1}/256");
                automaton.Step();
            }
            Console.WriteLine(GetGridString(automaton));
        }

        private static string GetGridString(CellularAutomata<int> automaton)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < automaton.Grid.Dimensions[1].Cells; y++)
            {
                for (int x = 0; x < automaton.Grid.Dimensions[0].Cells; x++)
                {
                    Vector<int> pos = AutomataVector.Create(x, y);
                    CellularAutomataCell<int> cell = automaton.Grid.State[pos];
                    sb.Append(cell.State.ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
