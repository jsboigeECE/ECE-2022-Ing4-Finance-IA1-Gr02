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
            var col = geneIndex%9;

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
            var genes = GetGenes().ToArray();
            var index = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku.Cellules[i][j] = (int)genes[index].Value;
                    index++;
                }
            }
            return new List<GridSudoku>(new[] { sudoku });
        }

    }
}
