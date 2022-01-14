using Sudoku.Shared;
using System.Linq;
using Microsoft.ML.Probabilistic.Algorithms;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Models.Attributes;
using System.Collections.Generic;

namespace Sudoku.Probabilistic
{

    public class ProbabilisticSolver : Sudoku.Shared.ISolverSudoku
    {
        GridSudoku ISolverSudoku.Solve(GridSudoku s)
        { 
            return s;
        }
    }

}