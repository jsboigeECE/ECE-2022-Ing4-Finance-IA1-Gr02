using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
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

    public abstract class SudokuChromosomeBase : ChromosomeBase, ISudokuChromosome
    {

        /// <summary>
        /// The target sudoku board to solve
        /// </summary>
        private readonly GridSudoku _targetGridSudoku;

        /// <summary>
        /// The cell domains updated from the initial mask for the board to solve
        /// </summary>
        private Dictionary<(int row, int column), List<int>> _extendedMask;


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
        public SudokuChromosomeBase(GridSudoku targetGridSudoku, Dictionary<(int row, int column), List<int>> extendedMask, int length) : base(length)
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
        public Dictionary<(int row, int column), List<int>> ExtendedMask
        {
            get
            {
                if (_extendedMask == null)
                {
                    // We generate 1 to 9 figures for convenience
                    var indices = Enumerable.Range(1, 9).ToList();
                    var extendedMask = new Dictionary<(int row, int column), List<int>>(81);
                    if (_targetGridSudoku != null)
                    {

                        

                        for (int row = 0; row < 9; row++)
                        {
                            for (int col = 0; col < 9; col++)
                            {
                                extendedMask[(row,col)] = _targetGridSudoku.GetAvailableNumbers(row, col).ToList();
                            }
                        }

                    }
                    else
                    {
                        //If we have no sudoku mask, 1-9 numbers are allowed for all cells
                        for (int row = 0; row < 9; row++)
                        {
                            for (int col = 0; col < 9; col++)
                            {
                                extendedMask[(row, col)] = indices;
                            }
                        }
                        //for (int i = 0; i < 81; i++)
                        //{
                        //    extendedMask.Add(i, indices);
                        //}
                    }
                    _extendedMask = extendedMask;

                }
                return _extendedMask;
            }
        }


        public abstract IList<GridSudoku> GetSudokus();

       
    }
}
