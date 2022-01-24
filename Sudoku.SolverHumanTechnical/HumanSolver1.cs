using Sudoku.Shared;
using System;
using SudokuSolver;

namespace Sudoku.SolverHumanTechnical
{
    public class HumanSolver1 : ISolverSudoku
    {
        GridSudoku ISolverSudoku.Solve(GridSudoku s)
        {
            return s;
        } 
    }
}
