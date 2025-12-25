using System.Numerics;
using CellularAutomata.NET;
using Conway3D.Rendering;

namespace Conway3D
{
    internal class Program
    {
        private const int _universeSize = 36;

        private static CancellationTokenSource _cancelToken = new CancellationTokenSource();
        private static Conway3DWindow _window;

        private static Queue<Vector<int>> _cellsToAdd = new Queue<Vector<int>>();
        private static Queue<Vector<int>> _cellsToRemove = new Queue<Vector<int>>();

        static async Task Main(string[] args)
        {
            _ = Task.Factory.StartNew(() => RunRenderer(), _cancelToken.Token);

            while (!_cancelToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        public static void RunRenderer()
        {
            _window = new Conway3DWindow(800, 800, 0.1f);
            _window.OnLoaded = () =>
            {
                Task.Factory.StartNew(() => RunAutomata(), _cancelToken.Token);
            };
            _window.Start();
            _cancelToken.Cancel();
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
                int livingNeighbors = neighbors.Sum(t => t.Value.State);
                if (cell.State == 1)
                {
                    if (livingNeighbors < 2 || livingNeighbors > 3)
                    {
                        _cellsToRemove.Enqueue(cell.Position);
                        cell.SetState(0);
                    }
                }
                else if (livingNeighbors == 3)
                {
                    _cellsToAdd.Enqueue(cell.Position);
                    cell.SetState(1);
                }
                else
                {
                    cell.SetState(0);
                }
            }
        );

        private static void RunAutomata()
        {
            CellularAutomataConfiguration<int> config = new CellularAutomataConfiguration<int>
            {
                Dimensions = new CellularAutomataDimension[]
                {
                    new CellularAutomataDimension()
                    {
                        Cells = _universeSize,
                        WrapEnd = true,
                        WrapStart = true,
                    },
                    new CellularAutomataDimension()
                    {
                        Cells = _universeSize,
                        WrapEnd = true,
                        WrapStart = true,
                    },
                    new CellularAutomataDimension()
                    {
                        Cells = _universeSize,
                        WrapEnd = true,
                        WrapStart = true,
                    },
                },
                DefaultState = 0,
            };

            List<Vector<int>> extrudedNeighborhood = new List<Vector<int>>();
            foreach (
                var vector in CellularAutomataNeighborhood<int>.MooreNeighborhood.NeighborLocations
            )
            {
                extrudedNeighborhood.AddRange(AutomataVector.Extrude(vector, -1, 1, 3));
            }
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
            Dictionary<System.Numerics.Vector<int>, int> initialState =
                new Dictionary<System.Numerics.Vector<int>, int>();
            for (int i = 0; i < _universeSize * _universeSize * _universeSize; i++)
            {
                if (Random.Shared.NextDouble() < 0.5d)
                {
                    Vector<int> pos = AutomataVector.Create(
                        Random.Shared.Next(_universeSize / 3, _universeSize / 3 * 2),
                        Random.Shared.Next(_universeSize / 3, _universeSize / 3 * 2),
                        Random.Shared.Next(_universeSize / 3, _universeSize / 3 * 2)
                    );
                    if (!initialState.ContainsKey(pos))
                    {
                        initialState.Add(pos, 1);
                    }
                }
            }

            automaton.InitializeGrid(initialState);
            _window.UpdateState(automaton.Grid);

            Console.WriteLine("Press a key to continue...");
            string? input = Console.ReadLine();

            int step = 0;
            while (input != null && input.ToLower() != "exit")
            {
                automaton.Step();
                while (_cellsToAdd.Count > 0)
                {
                    _window.AddWolframCube(_cellsToAdd.Dequeue());
                }
                while (_cellsToRemove.Count > 0)
                {
                    _window.RemoveWolframCube(_cellsToRemove.Dequeue());
                }
                Console.WriteLine("Press a key to continue to step " + (step + 1) + "...");
                input = Console.ReadLine();
                step++;
            }
        }

        ~Program()
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
        }
    }
}
