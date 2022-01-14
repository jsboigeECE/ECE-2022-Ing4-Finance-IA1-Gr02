using System;
using Sudoku;
using Sudoku.Shared;

namespace Sudoku.Graphecolore
{
    public class graphcoloringsolver1 : ISolverSudoku
    {
        GridSudoku ISolverSudoku.Solve(GridSudoku s)
        {
            return s;
        }
    }
}
