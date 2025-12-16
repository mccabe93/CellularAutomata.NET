using System.Numerics;
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
                Width = 1024,
                Height = 256,
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
                WrapColumns = false,
                WrapRows = false,
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
            Console.WriteLine(automaton.Grid);
        }
    }
}
