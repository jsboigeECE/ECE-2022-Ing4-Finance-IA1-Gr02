using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Sudoku.Shared;

namespace Sudoku.solver_OR_Tools
{
    public class OrToolsDotNetCPSolver : ISolverSudoku
    {

        public Shared.GridSudoku Solve(Shared.GridSudoku sudoku)
        {
            Solver solver = new Solver("Sudoku");

            int cell_size = 3;
            IEnumerable<int> CELL = Enumerable.Range(0, cell_size);
            int n = cell_size * cell_size;
            IEnumerable<int> RANGE = Enumerable.Range(0, n);


            int[][] grille = sudoku.Cellules;

            int[,] initial_grid = grille.To2D();

            IntVar[,] grid = solver.MakeIntVarMatrix(n, n, 1, 9, "grid");
            IntVar[] grid_flat = grid.Flatten();

            foreach (int i in RANGE)
            {
                foreach (int j in RANGE)
                {
                    if (initial_grid[i, j] > 0)
                    {
                        solver.Add(grid[i, j] == initial_grid[i, j]);
                    }
                }
            }
            foreach (int i in RANGE)
            {

                // rows
                solver.Add((from j in RANGE
                            select grid[i, j]).ToArray().AllDifferent());

                // cols
                solver.Add((from j in RANGE
                            select grid[j, i]).ToArray().AllDifferent());

            }

            // cells
            foreach (int i in CELL)
            {
                foreach (int j in CELL)
                {
                    solver.Add((from di in CELL
                                from dj in CELL
                                select grid[i * cell_size + di, j * cell_size + dj]
                                 ).ToArray().AllDifferent());
                }
            }


            DecisionBuilder db = solver.MakePhase(grid_flat,
                                                  Solver.INT_VAR_SIMPLE,
                                                  Solver.INT_VALUE_SIMPLE);

            solver.NewSearch(db);

            while (solver.NextSolution())
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        //Console.Write("{0} ", grid[i, j].Value());
                        sudoku.Cellules[i][ j] = (int) grid[i, j].Value();
                    }
                    //Console.WriteLine();
                }

                //Console.WriteLine();
            }

            //Console.WriteLine("\nSolutions: {0}", solver.Solutions());
            //Console.WriteLine("WallTime: {0}ms", solver.WallTime());
            //Console.WriteLine("Failures: {0}", solver.Failures());
            //Console.WriteLine("Branches: {0} ", solver.Branches());

            //sudoku.Cellules = grid.ToJaggedArray().Select(varrow => varrow.Select(varcells => (int)varcells.Value()).ToArray()).ToArray();
            //solver.EndSearch();

          

            return sudoku;
        }


    }
}

