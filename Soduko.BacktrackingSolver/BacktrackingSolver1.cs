using System;
using System.Collections.Generic;
using Microsoft;
using Google.Type;
using Sudoku.Shared;

namespace Soduko.BacktrackingSolver
{
    public class BacktrackingSolver1 : Sudoku.Shared.ISolverSudoku
    {

        public GridSudoku Solve(GridSudoku s)
        {
            int[,] sudoku;

            //Méthode pour utiliser un tableau format int[,] au lieu de [][] imposé par le format de base
            //On créer donc un tableau int[,] qui prend toutes les valeurs de la grille de sudoku en paramètre
            sudoku = Convertion(s);

            //Appel de la méthode de résolution
            _ = SolverBacktracking(sudoku, 9);

            //Boucle pour mettre à jour le tableau du suduko à retourner à partir du tableau sur lequel on a fait les modifications
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    s.Cellules[i][j] = sudoku[i, j];

            return s;
        }

        public int[,] Convertion(GridSudoku s)
        {
            int[,] sudok = new int[10, 10];

            //On remplace chaque case du nouveau tableau par la grille passée en paramètre
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    sudok[i, j] = s.Cellules[i][j];

            return sudok;
        }

        static bool SolverBacktracking(int[,] grid, int n)
        {
            int row = -1;
            int col = -1;
            bool isEmpty = true;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    if (grid[i, j] == 0){
                        row = i;
                        col = j;

                        isEmpty = false;
                        break;
                    }
                if (!isEmpty){
                    break;
                }
            }

            if (isEmpty)
                return true;

            for (int num = 1; num <= n; num++) if (IsSafe(grid, row, col, num))
                {
                    grid[row, col] = num;
                    if (SolverBacktracking(grid, n))
                    {
                        return true;
                    }
                    else
                        grid[row, col] = 0;
                }
            return false;
        }

        public static bool IsSafe(int[,] board, int row, int col, int num)
        {
            for (int d = 0; d < board.GetLength(0); d++)
                if (board[row, d] == num)
                    return false;

            // Column has the unique numbers (column-clash)
            for (int r = 0; r < board.GetLength(0); r++)
                if (board[r, col] == num)
                    return false;

            int sqrt = (int)Math.Sqrt(board.GetLength(0));
            int boxRowStart = row - (row % sqrt);
            int boxColStart = col - (col % sqrt);

            for (int r = boxRowStart; r < boxRowStart + sqrt; r++)
                for (int d = boxColStart; d < boxColStart + sqrt; d++)
                    if (board[r, d] == num)
                        return false;

            return true;
        }
    }
}