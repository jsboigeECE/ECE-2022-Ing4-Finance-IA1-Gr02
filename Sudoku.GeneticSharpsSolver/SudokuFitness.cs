using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using Sudoku.Shared;

namespace Sudoku.GeneticSharpsSolver
{
    public class SudokuFitness : IFitness 
    {
        protected GridSudoku _targetSudokuBoard;

        public SudokuFitness (GridSudoku targetSudokuBoard)
        {
            _targetSudokuBoard = targetSudokuBoard;
        }

        public double Evaluate(IChromosome chromosome)
        {
            return Evaluate((ISudokuChromosome)chromosome);
        }

        public double Evaluate(ISudokuChromosome chromosome)
        {
            List<double> scores = new List<double>();

            var sudokus = chromosome.GetSudokus();
            foreach (var sudoku in sudokus)
            {
                scores.Add(Evaluate(sudoku));
            }

            return scores.Sum();
        }

        public double Evaluate(GridSudoku testSudokuBoard)
        {
            // We use a large lambda expression to count duplicates in rows, columns and boxes
            var cells = testSudokuBoard.Cellules.Select((c, i) => new { index = i, cell = c }).ToList();
            var toTest = cells.GroupBy(x => x.index / 9).Select(g => g.Select(c => c.cell)) // rows
              .Concat(cells.GroupBy(x => x.index % 9).Select(g => g.Select(c => c.cell))) //columns
              .Concat(cells.GroupBy(x => x.index / 27 * 27 + x.index % 9 / 3 * 3).Select(g => g.Select(c => c.cell))); //boxes
            var toReturn = -toTest.Sum(test => test.GroupBy(x => x).Select(g => g.Count() - 1).Sum()); // Summing over duplicates


            //toReturn -= cells.Count(x => _targetSudokuBoard.Cellules[x.index][] > 0 && _targetSudokuBoard.Cellules[x.index] != x.cell); // Mask




            return toReturn;
        }
    }
}
