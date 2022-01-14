using Sudoku.Shared;
using DlxLib;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sudoku.DancingLinksSolvers{
    public class DancingLinksSolvers1 : ISolverSudoku{
        public Shared.GridSudoku Solve(Shared.GridSudoku s)
            {   
                var internalRows = BuildInternalRowsForGrid(s);
                var dlxRows = BuildDlxRows(internalRows);
                var solutions = new Dlx()
                    .Solve(dlxRows, d => d, r => r);
                    //.Where(solution => VerifySolution(internalRows, solution))
                    //.ToImmutableList();

                Console.WriteLine();

                if (solutions.Any())
                {
                    Console.WriteLine($"First solution (of {solutions.Count}):");
                    return s;
                }
                else
                {
                    Console.WriteLine("No solutions found!");
                    return s;
                }
                
                //  z3Context.MkTactic("smt");

        }
        private static IImmutableList<int> BuildDlxRow(Tuple<int, int, int, bool> internalRow)
        {
            var row = internalRow.Item1;
            var col = internalRow.Item2;
            var value = internalRow.Item3;
            var box = RowColToBox(row, col);

            var posVals = Encode(row, col);
            var rowVals = Encode(row, value - 1);
            var colVals = Encode(col, value - 1);
            var boxVals = Encode(box, value - 1);

            return posVals.Concat(rowVals).Concat(colVals).Concat(boxVals).ToImmutableList();
        }

        private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForGrid(Shared.GridSudoku s)
        {
            var rowsByCols =
                from row in Rows
                from col in Cols
                let value = s.ValueAt(row, col)
                select BuildInternalRowsForCell(row, col, value);

            return rowsByCols.SelectMany(cols => cols).ToImmutableList();
        }
        private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForCell(int row, int col, int value)
        {
            if (value >= 1 && value <= 9)
                return ImmutableList.Create(Tuple.Create(row, col, value, true));

            return Digits.Select(v => Tuple.Create(row, col, v, false)).ToImmutableList();
        }
    }

}
