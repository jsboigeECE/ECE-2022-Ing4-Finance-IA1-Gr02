using System;
using Sudoku.Shared;
using DlxLib;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sudoku.DancingLinksSolvers {

    public class DancingLinksSolverInit : DancingLinksSolversBase
    {
        public override GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolverDancingLinksBase(s);
        }
    }

    public class DancingLinksSolverBetter : DancingLinksSolversBase
    {
        public override GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolverDancingLinksBetter(s);
        }
    }
    public abstract class DancingLinksSolversBase : ISolverSudoku {

        public abstract GridSudoku Solve(Shared.GridSudoku s);
        protected GridSudoku SolverDancingLinksBase(Shared.GridSudoku s) {

            var internalRows = BuildInternalRowsForGrid(s);
            var dlxRows = BuildDlxRows(internalRows);
            var solutions = new Dlx()
                .Solve(dlxRows, d => d, r => r)
                .Where(solution => VerifySolution(internalRows, solution))
                .ToImmutableList();

            Console.WriteLine();

            if (solutions.Any())
            {
                Console.WriteLine($"First solution (of {solutions.Count}):");
                Console.WriteLine();
                return SolutionToGrid(internalRows, solutions.First());

            }
            else
            {
                Console.WriteLine("No solutions found!");
                return s;
            }

        }

        private static IEnumerable<int> Rows => Enumerable.Range(0, 9);
        private static IEnumerable<int> Cols => Enumerable.Range(0, 9);
        private static IEnumerable<Tuple<int, int>> Locations =>
            from row in Rows
            from col in Cols
            select Tuple.Create(row, col);
        private static IEnumerable<int> Digits => Enumerable.Range(1, 9);

        private static IImmutableList<IImmutableList<int>> BuildDlxRows(
            IEnumerable<Tuple<int, int, int, bool>> internalRows)
        {
            return internalRows.Select(BuildDlxRow).ToImmutableList();
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
                let value = s.Cellules[row][col]
                select BuildInternalRowsForCell(row, col, value);

            return rowsByCols.SelectMany(cols => cols).ToImmutableList();
        }
        private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForCell(int row, int col, int value)
        {
            if (value >= 1 && value <= 9)
                return ImmutableList.Create(Tuple.Create(row, col, value, true));

            return Digits.Select(v => Tuple.Create(row, col, v, false)).ToImmutableList();
        }

        private static int RowColToBox(int row, int col)
        {
            return row - (row % 3) + (col / 3);
        }

        private static IEnumerable<int> Encode(int major, int minor)
        {
            var result = new int[81];
            result[major * 9 + minor] = 1;
            return result.ToImmutableList();
        }

        private static bool VerifySolution(
            IReadOnlyList<Tuple<int, int, int, bool>> internalRows,
            Solution solution)
        {
            var solutionInternalRows = solution.RowIndexes
                .Select(rowIndex => internalRows[rowIndex])
                .ToImmutableList();

            var locationsGroupedByRow = Locations.GroupBy(t => t.Item1);
            var locationsGroupedByCol = Locations.GroupBy(t => t.Item2);
            var locationsGroupedByBox = Locations.GroupBy(t => RowColToBox(t.Item1, t.Item2));

            return
                CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByRow, "row") &&
                CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByCol, "col") &&
                CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByBox, "box");
        }

        private static bool CheckGroupsOfLocations(
            IEnumerable<Tuple<int, int, int, bool>> solutionInternalRows,
            IEnumerable<IGrouping<int, Tuple<int, int>>> groupedLocations,
            string tag)
        {
            return groupedLocations.All(grouping =>
                CheckLocations(solutionInternalRows, grouping, grouping.Key, tag));
        }

        private static bool CheckLocations(
            IEnumerable<Tuple<int, int, int, bool>> solutionInternalRows,
            IEnumerable<Tuple<int, int>> locations,
            int key,
            string tag)
        {
            var digits = locations.SelectMany(location =>
                solutionInternalRows
                    .Where(solutionInternalRow =>
                        solutionInternalRow.Item1 == location.Item1 &&
                        solutionInternalRow.Item2 == location.Item2)
                    .Select(t => t.Item3));
            return CheckDigits(digits, key, tag);
        }

        private static bool CheckDigits(
            IEnumerable<int> digits,
            int key,
            string tag)
        {
            var actual = digits.OrderBy(v => v);
            if (actual.SequenceEqual(Digits)) return true;
            var values = string.Concat(actual.Select(n => Convert.ToString(n)));
            Console.WriteLine($"{tag} {key}: {values} !!!");
            return false;
        }
        private static Shared.GridSudoku SolutionToGrid(
            IReadOnlyList<Tuple<int, int, int, bool>> internalRows,
            Solution solution)
        {
            var solutiongrid = solution.RowIndexes
                .Select(rowIndex => internalRows[rowIndex])
                .OrderBy(t => t.Item1)
                .ThenBy(t => t.Item2)
                .GroupBy(t => t.Item1, t => t.Item3)
                .Select(value => string.Concat(value))
                .ToImmutableList();

            var sol = new GridSudoku();
            for (int i = 0; i < solutiongrid.Count; i++)
            {
                for (int j = 0; j < solutiongrid.Count; j++)
                {
                    sol.Cellules[i][j] = solutiongrid[i][j] - 48;

                }
            }

            return sol;
        }



        private int[,] matrix;
        private const int NBCONSTRAIN = 9 * 9 * 4;

        private void matrixBuilder(Shared.GridSudoku s)
        {
            int nbCaseRemplie = 0; //= s.get.Aggregate(0, (acc, x) => acc + x.Aggregate(0, (a, b) => a + ((b == 0) ? 0 : 1)));
            for (int i = 0; i < s.Cellules.Length; i++)
            {
                for (int j = 0; j < s.Cellules.Length; j++)
                {
                    if (s.Cellules[i][j] > 0)
                    {
                        nbCaseRemplie += 1;
                    }
                }
            }
            matrix = new int[(81 - nbCaseRemplie) * 9 + nbCaseRemplie, NBCONSTRAIN];
            int imatrix = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    imatrix = buildLine(i, j, s.Cellules[i][j], imatrix);
                }
            }
        }

        private int buildLine(int i, int j, int value, int imatrix)
        {
            if (value == 0)
            {
                int RCC = calcRCConstrain(i, j);
                int RNC = calcRNConstrain(i, 1);
                int CNC = calcCNConstrain(j, 1);
                int BNC = calcBNConstrain(i, j, 1);
                int end = imatrix + 9;
                for (; imatrix < end; imatrix++)
                {
                    matrix[imatrix, RCC] = 1;
                    matrix[imatrix, RNC++] = 1;
                    matrix[imatrix, CNC++] = 1;
                    matrix[imatrix, BNC++] = 1;
                }
                return end;
            }
            else
            {
                matrix[imatrix, calcRCConstrain(i, j)] = 1;
                matrix[imatrix, calcRNConstrain(i, value)] = 1;
                matrix[imatrix, calcCNConstrain(j, value)] = 1;
                matrix[imatrix, calcBNConstrain(i, j, value)] = 1;
                return imatrix + 1;
            }
        }

        private int calcRCConstrain(int i, int j)
        {
            return 9 * i + j;
        }

        private int calcRNConstrain(int i, int value)
        {
            return 81 + 9 * i + value - 1;
        }

        private int calcCNConstrain(int j, int value)
        {
            return 162 + 9 * j + value - 1;
        }

        private int calcBNConstrain(int i, int j, int value)
        {
            return 243 + ((i / 3) * 3 + j / 3) * 9 + value - 1;
        }

        private void convertSolutionToSudoku(IEnumerable<int> r, int[,] m)//DlxLib.Solution s, int[,] m)
        {
            foreach (int row in r)
            {
                int x = 0, y = 0, nb = 0;
                for (int j = 0; j < 81; j++)
                {
                    if (m[row, j] == 1)
                    {
                        x = j % 9; y = j / 9;
                        break;
                    }
                }
                for (int j = 81; j < 162; j++)
                {
                    if (m[row, j] == 1)
                    {
                        nb = (j - 81) % 9 + 1;
                        break;
                    }
                }
                //s.setCaeSudokus(y, x, nb);
                //sudoku.setCaseSudoku((nb / 9), (nb % 9), (row % 10) + 1);
            }
        }
        protected GridSudoku SolverDancingLinksBetter(Shared.GridSudoku s) {

            MatrixList sudokuMat = new MatrixList(s.Cellules);
            sudokuMat.search();
            
            s.Cellules = sudokuMat.convertMatrixSudoku();

            for (int i = 0; i < s.Cellules.Length; i++)
            {
                for (int j = 0; j < s.Cellules.Length; j++)
                {

                    //s.Cellules[i][j] = sudokuFin[i][j];
                }
            }

            return s;

            
        }

    }
    

}

