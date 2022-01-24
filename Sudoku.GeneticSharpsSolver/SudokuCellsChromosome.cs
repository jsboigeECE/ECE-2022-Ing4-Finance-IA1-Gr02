using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.GeneticSharpsSolver
{
    public class SudokuCellsChromosome : SudokuChromosomeBase, ISudokuChromosome
    {
        public SudokuCellsChromosome() : this(null)
        {
        }

        /// <summary>
        /// Basic constructor with target sudoku to solve
        /// </summary>
        /// <param name="targetGridSudoku">the target sudoku to solve</param>
        public SudokuCellsChromosome(GridSudoku targetGridSudoku) : this(targetGridSudoku, null) { }

        /// <summary>
        /// Constructor with additional precomputed domains for faster cloning
        /// </summary>
        /// <param name="targetGridSudoku">the target sudoku to solve</param>
        /// <param name="extendedMask">The cell domains after initial constraint propagation</param>
        public SudokuCellsChromosome(GridSudoku targetGridSudoku, Dictionary<(int row, int column), List<int>> extendedMask) : base(targetGridSudoku, extendedMask, 81)
        {
        }


        public override Gene GenerateGene(int geneIndex)
        {

            var row = (int)geneIndex / 9;
            var col = 0;
            if (geneIndex > 8 && geneIndex < 18)
            {
                col = 1;
            }
            if (geneIndex > 17 && geneIndex < 27)
            {
                col = 2;
            }
            if (geneIndex > 26 && geneIndex < 36)
            {
                col = 3;
            }
            if (geneIndex > 35 && geneIndex < 45)
            {
                col = 4;
            }
            if (geneIndex > 44 && geneIndex < 54)
            {
                col = 5;
            }
            if (geneIndex > 53 && geneIndex < 63)
            {
                col = 6;
            }
            if (geneIndex > 62 && geneIndex < 72)
            {
                col = 7;
            }
            if (geneIndex > 71 && geneIndex < 81)
            {
                col = 8;
            }

            //If a target mask exist and has a digit for the cell, we use it.
            if (TargetGridSudoku != null && TargetGridSudoku.Cellules[row][col] != 0)
            {
                return new Gene(TargetGridSudoku.Cellules[row][col]);
            }
            // otherwise we use a random digit amongts those permitted.
            var rnd = RandomizationProvider.Current;
            var targetIdx = rnd.GetInt(0, ExtendedMask[(row, col)].Count);
            return new Gene(ExtendedMask[(row, col)][targetIdx]);

        }

        public override IChromosome CreateNew()
        {
            return new SudokuCellsChromosome(TargetGridSudoku, ExtendedMask);
        }

        /// <summary>
        /// Builds a single Sudoku from the 81 genes
        /// </summary>
        /// <returns>A Sudoku board built from the 81 genes</returns>
        public override IList<GridSudoku> GetSudokus()
        {
            var sudoku = new GridSudoku();
            return new List<GridSudoku>(new[] { sudoku });
        }

    }
}
