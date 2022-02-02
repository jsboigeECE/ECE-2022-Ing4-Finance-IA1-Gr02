using System;
using System.Collections;
using System.Collections.Generic;
using Sudoku.Shared;
using SudokuCombinatorialEvolutionSolver;

namespace Sudoku.PSOSolver
{
    public class ParticleSwarmSimpleSolver : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {
            var converted = s.Cellules.To2D();
            var sudoku = SudokuCombinatorialEvolutionSolver.Sudoku.New(converted);

            var solver = new SudokuSolver();

            var solvedSudoku = solver.Solve(sudoku, 200, 5000, 40);

            var numOrganisms = 200;
            //On augmente le nombre d'organismes jusqu'à ce qu'une solution soit trouvée
            do
            {
                solvedSudoku = solver.Solve(sudoku, numOrganisms, 5000, 1);
                numOrganisms *= 2;
            } while (solvedSudoku.Error > 0);

            var solvedCells = solvedSudoku.CellValues.ToJaggedArray();
            s.Cellules = solvedCells;
            return s;
        }


    }
} // Program

