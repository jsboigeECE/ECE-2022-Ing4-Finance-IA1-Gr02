using Sudoku.Shared;
using System;

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
