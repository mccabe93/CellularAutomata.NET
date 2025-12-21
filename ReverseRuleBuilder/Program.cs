using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CellularAutomata.NET;

namespace ReverseRuleBuilder
{
    internal class Program
    {
        private static List<Vector<int>> above = AutomataVector.Extrude(
            AutomataVector.Create(0, -1),
            -2,
            2,
            1
        );

        static void Main(string[] args)
        {
            // csharpier-ignore
            int[][] targetState = new int[][] {
                new int[] { 0, 1, 0, 1, 0 },
                new int[] { 1, 0, 1, 0, 1 }
            };

            CellularAutomataConfiguration<int> config = new CellularAutomataConfiguration<int>
            {
                Dimensions = new CellularAutomataDimension[]
                {
                    new CellularAutomataDimension() { Cells = 5 },
                    new CellularAutomataDimension() { Cells = 2 },
                },
                DefaultState = 0,
            };
            CellularAutomataNeighborhood<int> neighborhood = new CellularAutomataNeighborhood<int>()
            {
                NeighborLocations = above.ToArray(),
                ClearNeighborsAfterUse = true,
            };

            Dictionary<System.Numerics.Vector<int>, int> initialState = new Dictionary<
                System.Numerics.Vector<int>,
                int
            >
            {
                { AutomataVector.Create(1, 0), 1 },
                { AutomataVector.Create(3, 0), 1 },
            };

            uint mostSuccessfulRule = 0;
            int mostCorrectStepsTaken = 0;
            int ruleConfigurations = (int)Math.Pow(2, 5);
            for (uint rule = 8; rule < ruleConfigurations; rule++)
            {
                Action<
                    CellularAutomataCell<int>,
                    Dictionary<Vector<int>, CellularAutomataCell<int>>,
                    CellularAutomata<int>
                > rules = GetRules(rule);
                CellularAutomata<int> automaton = new CellularAutomata<int>(
                    config,
                    neighborhood,
                    rules
                );
                automaton.InitializeGrid(initialState);
                bool success = false;
                CellularAutomataGrid<int> grid;
                Console.Write($"Testing rule: {rule}");
                Console.WriteLine(
                    $"Most successful rule: {mostSuccessfulRule}, Most correct steps taken: {mostCorrectStepsTaken})"
                );
                for (int step = 0; step < config.Dimensions[1].Cells; step++)
                {
                    grid = automaton.Step();
                    success = CheckCorrectness(step, config, targetState[step], grid);
                    Console.WriteLine(GetGridString(automaton));
                    if (!success)
                        break;
                    if (step > mostCorrectStepsTaken)
                    {
                        mostCorrectStepsTaken = step;
                        mostSuccessfulRule = rule;
                    }
                }
                if (!success)
                    continue;
                if (success)
                {
                    Console.WriteLine("Found matching rule: " + rule);
                    Console.WriteLine(GetGridString(automaton));
                    break;
                }
            }
        }

        private static bool CheckCorrectness(
            int step,
            CellularAutomataConfiguration<int> config,
            int[] targetState,
            CellularAutomataGrid<int> grid
        )
        {
            for (int x = 0; x < config.Dimensions[0].Cells; x++)
            {
                Vector<int> pos = AutomataVector.Create(x, step);
                if (targetState[x] != grid.State[pos].State)
                {
                    return false;
                }
            }
            return true;
        }

        /*
         * Generalized Elementary Automata Rules
         */
        private static Action<
            CellularAutomataCell<int>,
            Dictionary<Vector<int>, CellularAutomataCell<int>>,
            CellularAutomata<int>
        > GetRules(uint rule)
        {
            return (cell, neighbors, automaton) =>
            {
                // Which 5 cell config are we in
                // e.g 1 1 1 0 0 -> 11100 -> 28
                int index = 0;
                for (int i = 0; i < above.Count; i++)
                {
                    if (neighbors.TryGetValue(above[i], out var neighbor) && neighbor != null)
                    {
                        if (neighbor.State == 1)
                        {
                            int neighborBitIndex = 2 + above[i][0];
                            index |= neighborBitIndex == 0 ? 1 : 1 << neighborBitIndex;
                        }
                    }
                }
                // Is this config part of our rule?
                // Check index against bit value at index
                long mask = index & rule;
                if (mask == 0)
                {
                    return;
                }
                // index = int value of neighbor
                // index xor (index mask rule) == 0 = All bits in index are also set in rule
                bool ruleApplies = (index ^ (index & rule)) == 0;

                if (ruleApplies && cell.State == 0)
                {
                    cell.SetState(1);
                }
            };
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
