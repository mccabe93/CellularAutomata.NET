using System.Numerics;
using System.Text;
using CellularAutomata.NET;
using Spectre.Console;

namespace MNCA
{
    // Multi-neighborhood Cellular Automata
    // Based on the first example in this article by slackermanz:
    // https://slackermanz.com/understanding-multiple-neighborhood-cellular-automata/
    internal class Program
    {
        private static List<Vector<int>> InnerNeighborhood = new List<Vector<int>>();
        private static List<Vector<int>> OuterNeighborhood = new List<Vector<int>>();

        private static void InitializeInnerNeighborhood()
        {
            var neighborhoodBitmap = new CellularBitmap<int>(
                new CellularBitmapConfiguration<int>()
                {
                    File = "inner-neighborhood.bmp",
                    ColorToStateBitmap = CellularBitmapConfiguration<int>.DefaultNeighborhoodMap,
                    Type = CellularBitmapType.NeighborhoodMap,
                }
            );
            InnerNeighborhood = neighborhoodBitmap.LoadNeighborhood();
        }

        private static void InitializeOuterNeighborhood()
        {
            var neighborhoodBitmap = new CellularBitmap<int>(
                new CellularBitmapConfiguration<int>()
                {
                    File = "outer-neighborhood.bmp",
                    ColorToStateBitmap = CellularBitmapConfiguration<int>.DefaultNeighborhoodMap,
                    Type = CellularBitmapType.NeighborhoodMap,
                }
            );
            OuterNeighborhood = neighborhoodBitmap.LoadNeighborhood();
        }

        private static List<Vector<int>> CombineNeighborhoods()
        {
            List<Vector<int>> combined = new List<Vector<int>>();
            combined.AddRange(InnerNeighborhood);
            combined.AddRange(OuterNeighborhood);
            return combined;
        }

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
                int innerNeighborhoodSum = 0;
                int outerNeighborhoodSum = 0;
                foreach (var neighbor in neighbors)
                {
                    if (InnerNeighborhood.Contains(neighbor.Key))
                    {
                        innerNeighborhoodSum += neighbor.Value.State;
                    }
                    else if (OuterNeighborhood.Contains(neighbor.Key))
                    {
                        outerNeighborhoodSum += neighbor.Value.State;
                    }
                }
                float innerNeighborhoodAverage =
                    innerNeighborhoodSum / (float)InnerNeighborhood.Count;
                float outerNeighborhoodAverage =
                    outerNeighborhoodSum / (float)OuterNeighborhood.Count;
                if (outerNeighborhoodAverage >= 0.210 && outerNeighborhoodAverage <= 0.220)
                {
                    cell.SetState(1);
                }
                if (outerNeighborhoodAverage >= 0.350 && outerNeighborhoodAverage <= 0.500)
                {
                    cell.SetState(0);
                }
                if (outerNeighborhoodAverage >= 0.750 && outerNeighborhoodAverage <= 0.850)
                {
                    cell.SetState(0);
                }
                if (innerNeighborhoodAverage >= 0.100 && innerNeighborhoodAverage <= 0.280)
                {
                    cell.SetState(0);
                }
                if (innerNeighborhoodAverage >= 0.430 && innerNeighborhoodAverage <= 0.550)
                {
                    cell.SetState(1);
                }
                if (outerNeighborhoodAverage >= 0.120 && outerNeighborhoodAverage <= 0.150)
                {
                    cell.SetState(0);
                }
            }
        );

        static void Main(string[] args)
        {
            InitializeInnerNeighborhood();
            InitializeOuterNeighborhood();
            CellularAutomataConfiguration<int> config = new CellularAutomataConfiguration<int>
            {
                Dimensions = new CellularAutomataDimension[]
                {
                    new CellularAutomataDimension() { Cells = 192 },
                    new CellularAutomataDimension() { Cells = 32 },
                },
                DefaultState = 0,
            };
            CellularAutomataNeighborhood<int> neighborhood = new CellularAutomataNeighborhood<int>()
            {
                NeighborLocations = CombineNeighborhoods().ToArray(),
                ClearNeighborsAfterUse = true,
            };

            CellularAutomata<int> automaton = new CellularAutomata<int>(
                config,
                neighborhood,
                Rules
            );

            var rng = new Random(2);

            Dictionary<System.Numerics.Vector<int>, int> initialState =
                new Dictionary<System.Numerics.Vector<int>, int>();

            for (int i = 0; i < config.Dimensions[0].Cells; i++)
            {
                for (int j = 0; j < config.Dimensions[1].Cells; j++)
                {
                    if (rng.NextDouble() < 0.40)
                    {
                        initialState[AutomataVector.Create(i, j)] = 1;
                    }
                }
            }

            automaton.InitializeGrid(initialState);

            AnsiConsole.MarkupLine("[bold yellow]Multi-Neighborhood Cellular Automata[/]");
            AnsiConsole.MarkupLine("[dim]Press [green]ESC[/] to exit[/]\n");

            var panel = new Panel(GetGridString(automaton))
            {
                Header = new PanelHeader("[bold blue]Generation 0[/]"),
                Border = BoxBorder.Rounded,
            };

            int generation = 0;
            bool running = true;

            AnsiConsole
                .Live(panel)
                .AutoClear(false)
                .Start(ctx =>
                {
                    while (running)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(intercept: true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                running = false;
                                break;
                            }
                        }

                        automaton.Step();
                        generation++;

                        var updatedPanel = new Panel(GetGridString(automaton))
                        {
                            Header = new PanelHeader($"[bold blue]Generation {generation}[/]"),
                            Border = BoxBorder.Rounded,
                        };

                        ctx.UpdateTarget(updatedPanel);
                        ctx.Refresh();

                        Thread.Sleep(50);
                    }
                });

            AnsiConsole.MarkupLine("\n[bold green]Exited after {0} generations[/]", generation);
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
