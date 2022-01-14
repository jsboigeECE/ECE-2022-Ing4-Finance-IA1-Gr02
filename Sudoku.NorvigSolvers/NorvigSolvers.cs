using Sudoku.Shared;
using System;

namespace Sudoku.NorvigSolvers
{
    public class NorvigSolvers : ISolverSudoku
    {
        public Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return s;
            //  z3Context.MkTactic("smt");

        }
    }
}
