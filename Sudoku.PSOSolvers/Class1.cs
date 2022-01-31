using System;
using System.Collections;
using System.Collections.Generic;
using Sudoku.Shared;

namespace Sudoku.PSOSolver
{
    public class SudokuEvoProgram : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {
            Console.WriteLine("Begin solving Sudoku");
            Console.WriteLine("The problem is: ");
            int[][] problem = new int[9][];
            int tab[][] = s.Cellules();
            problem[0] = tab[0][];
            problem[1] = tab[1][];
            problem[2] = tab[2][];
            problem[3] = tab[3][];
            problem[4] = tab[4][];
            problem[5] = tab[5][];
            problem[6] = tab[6][];
            problem[7] = tab[7][];
            problem[8] = tab[8][];
            DisplayMatrix(problem);
            int numOrganisms = 200;
            int maxEpochs = 5000;
            int maxRestarts = 20;
            int[][] soln = Solve(problem, numOrganisms,
              maxEpochs, maxRestarts);
            Console.WriteLine("Best solution found: ");
            DisplayMatrix(soln);
            int err = Error(soln);
            if (err == 0)
                Console.WriteLine("Success \n");
            else
                Console.WriteLine("Did not find optimal solution \n");
            Console.WriteLine("End Sudoku demo");
            Console.ReadLine();
        }

        public static int[][] SudokuEvoProgram.Solve(int[][] problem, int numOrganisms, int maxEpochs, int maxRestarts)
        { . . }
        public static void DisplayMatrix(int[][] matrix) { . . }
        public static int[][] SolveEvo(int[][] problem,
          int numOrganisms, int maxEpochs)
        { . . }
        public static int[][] RandomMatrix(int[][] problem) { . . }
        public static int[] Corner(int block) { . . }
        public static int Block(int r, int c) { . . }
        public static int[][] NeighborMatrix(int[][] problem,
          int[][] matrix)
     public static int[][] MergeMatrices(int[][] m1,
      int[][] m2)
        { . . }
        public static int Error(int[][] matrix) { . . }
        public static int[][] DuplicateMatrix(int[][] matrix) { . . }
        public static int[][] CreateMatrix(int n) { . . }
            throw new NotImplementedException();
    }
} // Program
public class Organism
{
    public int type;  // 0 = worker, 1 = explorer
    public int[][] matrix;
    public int error;
    public int age;
    public Organism(int type, int[][] m, int error, int age)
    {
        this.type = type;
        this.matrix = SudokuEvoProgram.DuplicateMatrix(m);
        this.error = error;
        this.age = age;
    }
}
}
