using System.Numerics;
using System.Text;
using CellularAutomata.NET;

namespace Conway
{
    internal class Program
    {
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
                // https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life#Rules
                if (neighbors.Count == 0)
                {
                    return;
                }
                int livingNeighbors = neighbors.Sum(t => t.Value.State);
                if (cell.State == 1)
                {
                    if (livingNeighbors < 2 || livingNeighbors > 3)
                    {
                        cell.SetState(0);
                    }
                }
                else if (livingNeighbors == 3)
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
                    new CellularAutomataDimension { Cells = 48 },
                    new CellularAutomataDimension { Cells = 48 },
                },
                DefaultState = 0,
            };

            CellularAutomata<int> automaton = new CellularAutomata<int>(
                config,
                CellularAutomataNeighborhood<int>.MooreNeighborhood,
                Rules
            );

            Dictionary<System.Numerics.Vector<int>, int> initialState = new Dictionary<
                System.Numerics.Vector<int>,
                int
            >()
            {
                /*
                 * Glider at 0,0
                 * 1 0 0
                 * 0 1 1
                 * 1 1 0
                 */
                { AutomataVector.Create(0, 0), 1 },
                { AutomataVector.Create(1, 1), 1 },
                { AutomataVector.Create(2, 1), 1 },
                { AutomataVector.Create(0, 2), 1 },
                { AutomataVector.Create(1, 2), 1 },
            };

            // Glider moves one cell to the right every 4 steps.
            // We are moving width - the length of the glider, so 45 total along the x-axis.
            int stepsToEnd = 45 * 4;

            automaton.InitializeGrid(initialState);

            int step = 0;
            while (step <= stepsToEnd)
            {
                Console.WriteLine($"Step {step++}/{stepsToEnd}\n{GetGridString(automaton)}");
                automaton.Step();
            }
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
