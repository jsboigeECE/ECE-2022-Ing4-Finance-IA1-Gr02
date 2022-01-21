using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.GeneticSharpsSolver
{

    /// <summary>
    /// Each type of chromosome for solving a sudoku is simply required to output a list of candidate sudokus
    /// </summary>
    public interface ISudokuChromosome
    {
        IList<GridSudoku> GetSudokus();
    }
    /// <summary>
    /// This abstract chromosome accounts for the target mask if given, and generates an extended mask with cell domains updated according to original mask
    /// </summary>
    /// 




    public abstract class SudokuChromosomeBase : ChromosomeBase, ISudokuChromosome, ISolverSudoku
    {

        /// <summary>
        /// The target sudoku board to solve
        /// </summary>
        public abstract GridSudoku Solve(Shared.GridSudoku s);
        private readonly GridSudoku _targetGridSudoku;

        /// <summary>
        /// The cell domains updated from the initial mask for the board to solve
        /// </summary>
        private Dictionary<int, List<int>> _extendedMask;


        /// <summary>
        /// Constructor that accepts a Sudoku to solve
        /// </summary>
        /// <param name="targetGridSudoku">the target sudoku to solve</param>
        /// <param name="length">The number of genes for the sudoku chromosome</param>
        public SudokuChromosomeBase(GridSudoku targetGridSudoku, int length) : this(targetGridSudoku, null, length) { }

        /// <summary>
        /// Constructor that accepts an additional extended mask for quick cloning
        /// </summary>
        /// <param name="targetGridSudoku">the target sudoku to solve</param>
        /// <param name="extendedMask">The cell domains after initial constraint propagation</param>
        /// <param name="length">The number of genes for the sudoku chromosome</param>
        public SudokuChromosomeBase(GridSudoku targetGridSudoku, Dictionary<int, List<int>> extendedMask, int length) : base(length)
        {
            _targetGridSudoku = targetGridSudoku;
            _extendedMask = extendedMask;
            CreateGenes();
        }


        /// <summary>
        /// The target sudoku board to solve
        /// </summary>
        public GridSudoku TargetGridSudoku => _targetGridSudoku;

        /// <summary>
        /// The cell domains updated from the initial mask for the board to solve
        /// </summary>
        public Dictionary<int, List<int>> ExtendedMask
        {
            get
            {
                if (_extendedMask == null)
                {
                    // We generate 1 to 9 figures for convenience
                    var indices = Enumerable.Range(1, 9).ToList();
                    var extendedMask = new Dictionary<int, List<int>>(81);
                    if (_targetGridSudoku != null)
                    {
                        //If target sudoku mask is provided, we generate an inverted mask with forbidden values by propagating rows, columns and boxes constraints
                        var forbiddenMask = new Dictionary<int, List<int>>();
                        List<int> targetList = null;
                        for (var index = 0; index < _targetGridSudoku.Cells.Count; index++)
                        {
                            var targetCell = _targetGridSudoku.Cells[index];
                            if (targetCell != 0)
                            {
                                //We parallelize going through all 3 constraint neighborhoods
                                var row = index / 9;
                                var col = index % 9;
                                var boxStartIdx = (index / 27 * 27) + (index % 9 / 3 * 3);

                                for (int i = 0; i < 9; i++)
                                {
                                    //We go through all 9 cells in the 3 neighborhoods
                                    var boxtargetIdx = boxStartIdx + (i % 3) + ((i / 3) * 9);
                                    var targetIndices = new[] { (row * 9) + i, i * 9 + col, boxtargetIdx };
                                    foreach (var targetIndex in targetIndices)
                                    {
                                        if (targetIndex != index)
                                        {
                                            if (!forbiddenMask.TryGetValue(targetIndex, out targetList))
                                            {
                                                //If the current neighbor cell does not have a forbidden values list, we create it
                                                targetList = new List<int>();
                                                forbiddenMask[targetIndex] = targetList;
                                            }
                                            if (!targetList.Contains(targetCell))
                                            {
                                                // We add current cell value to the neighbor cell forbidden values
                                                targetList.Add(targetCell);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // We invert the forbidden values mask to obtain the cell permitted values domains
                        for (var index = 0; index < _targetGridSudoku.Cells.Count; index++)
                        {
                            extendedMask[index] = indices.Where(i => !forbiddenMask[index].Contains(i)).ToList();
                        }

                    }
                    else
                    {
                        //If we have no sudoku mask, 1-9 numbers are allowed for all cells
                        for (int i = 0; i < 81; i++)
                        {
                            extendedMask.Add(i, indices);
                        }
                    }
                    _extendedMask = extendedMask;

                }
                return _extendedMask;
            }
        }

        public abstract IList<GridSudoku> GetSudokus();

    }







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
        public SudokuCellsChromosome(GridSudoku targetGridSudoku, Dictionary<int, List<int>> extendedMask) : base(targetGridSudoku, extendedMask, 81)
        {
        }


        public override Gene GenerateGene(int geneIndex)
        {
            //If a target mask exist and has a digit for the cell, we use it.
            if (TargetGridSudoku != null && TargetGridSudoku.Cells[geneIndex] != 0)
            {
                return new Gene(TargetGridSudoku.Cells[geneIndex]);
            }
            // otherwise we use a random digit amongts those permitted.
            var rnd = RandomizationProvider.Current;
            var targetIdx = rnd.GetInt(0, ExtendedMask[geneIndex].Count);
            return new Gene(ExtendedMask[geneIndex][targetIdx]);
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
            var sudoku = new GridSudoku(GetGenes().Select(g => (int)g.Value));
            return new List<GridSudoku>(new[] { sudoku });
        }
    }


}
