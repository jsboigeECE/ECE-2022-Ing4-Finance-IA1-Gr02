namespace Sudoku.Shared
{
    public class EmptySolver : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {
            return s.CloneSudoku();
        }
    }
}