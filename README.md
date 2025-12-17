# CellularAutomata.NET

Lightweight framework for generating cellular automata in .NET.

## Examples

There are two examples included in the project.

- Wolfram.csproj => Rule 30
- Conway.csproj => Game of Life.

## Usage

This framework allows for N-dimensional cellular automata. The limit varies based on your SIMD limitation for vectors.

Each automata has a Rule function which is applied to each cell each step. From Conway's Game of Life example:

```cs
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
```

The Func passes the cell being examined, a dictionary of its neighbors (with their relative positions) and the automata that called the function.

In this example, we look at the neighbors of our cell and sum up their State values.

The next piece of an automata is its configuration.

```cs
CellularAutomataConfiguration<int> config = new CellularAutomataConfiguration<int>
{
	new CellularAutomataDimension { Cells = 48 },
	new CellularAutomataDimension { Cells = 48 },
	DefaultState = 0
};
```

We specify the two dimensions of our grid and the default state of a cell. Dimensions contain properties for WrappedStart and WrappedEnd, but for this Game of Life we're not wrapping.

We'll also use the predefined [Moore neighborhood](https://en.wikipedia.org/wiki/Moore_neighborhood). This simplifies the process of starting up our automaton. You can reference the Wolfram example for a custom neighborhood configuration.

```cs
CellularAutomata<int> automaton = new CellularAutomata<int>(
	config,
	CellularAutomataNeighborhood<int>.MooreNeighborhood,
	Rules
	);
```

Now we set our initial state and start steppin'.

```cs
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
    Console.WriteLine($"Step {step++}/{stepsToEnd}\n{automaton.Grid}");
    automaton.Step();
}
```

![conway.gif](/_resources/conway.gif)

Voila!