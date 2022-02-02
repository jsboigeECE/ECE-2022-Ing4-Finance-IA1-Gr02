using Sudoku.Shared;
using System.Linq;
using Microsoft.ML.Probabilistic.Algorithms;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Models.Attributes;
using System.Collections.Generic;
//using Sudoku.Core;


namespace Sudoku.Probabilistic
{

    public class ProbabilisticSolver : ISolverSudoku
    {
        

        private static NaiveSudokuModel naiveModel = new NaiveSudokuModel();
        private static RobustSudokuModel robustModel = new RobustSudokuModel();

        GridSudoku ISolverSudoku.Solve(GridSudoku s)
        {
            System.Console.WriteLine("\n1. Naive Solver \n2. Robust Solver");
            int choice = System.Console.Read();
            if (choice == 1)
            {
                naiveModel.SolveSudoku(s);
            }
            else if (choice == 2)
            {
                robustModel.SolveSudoku(s);
            }
            
            return s.CloneSudoku();
        }
    }


    public class RobustSudokuModel
    {

        public InferenceEngine InferenceEngine;

        private static List<int> CellDomain = Enumerable.Range(1, 9).ToList();
        private static List<int> CellIndices = Enumerable.Range(0, 81).ToList();

        public VariableArray<Dirichlet> CellsPrior;
        public VariableArray<Vector> ProbCells;
        public VariableArray<int> Cells;

        private const double EpsilonProba = 0.00000001;
        private static double FixedValueProba = 1.0 - ((CellDomain.Count - 1) * EpsilonProba);



        /*public RobustSudokuModel()
        {
            
        }*/

        public virtual void SolveSudoku(GridSudoku s)
        {
            Range valuesRange = new Range(CellDomain.Count).Named("valuesRange");
            Range cellsRange = new Range(CellIndices.Count).Named("cellsRange");

            CellsPrior = Variable.Array<Dirichlet>(cellsRange).Named("CellsPrior");
            ProbCells = Variable.Array<Vector>(cellsRange).Named("ProbCells");
            ProbCells[cellsRange] = Variable<Vector>.Random(CellsPrior[cellsRange]);
            ProbCells.SetValueRange(valuesRange);


            // Initialisation des distribution a priori de façon uniforme (les valeurs sont équiprobables pour chaque cellule)

            Dirichlet[] dirUnifArray = Enumerable.Repeat(Dirichlet.Uniform(CellDomain.Count), CellIndices.Count).ToArray();
            CellsPrior.ObservedValue = dirUnifArray;

            Cells = Variable.Array<int>(cellsRange);
            Cells[cellsRange] = Variable.Discrete(ProbCells[cellsRange]);

            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    foreach (var neighbourCellIndex in GridSudoku.CellNeighbours[rowIndex][colIndex])
                    {
                        var cellIndex80 = rowIndex * 9 + colIndex;
                        var neighborCellIndex80 = neighbourCellIndex.row * 9 + neighbourCellIndex.column;

                        if (neighborCellIndex80 > cellIndex80)
                        {
                            Variable.ConstrainFalse(s.Cellules[rowIndex][colIndex] == s.Cellules[neighbourCellIndex.row][neighbourCellIndex.column]);
                        }
                    }
                }

                /*Code original
                //Ajout des contraintes de Sudoku (all diff pour tous les voisinages)
                foreach (var cellIndex in GrilleSudoku.IndicesCellules)
                {
                    foreach (var neighbourCellIndex in GrilleSudoku.VoisinagesParCellule[cellIndex])
                    {
                        if (neighbourCellIndex > cellIndex)
                        {
                            Variable.ConstrainFalse(Cells[cellIndex] == Cells[neighbourCellIndex]);
                        }
                    }
                }*/


                IAlgorithm algo = new ExpectationPropagation();
                //IAlgorithm algo = new GibbsSampling();
                //IAlgorithm algo = new VariationalMessagePassing();
                //IAlgorithm algo = new MaxProductBeliefPropagation();
                //les algos ont ete teste cependant la difference lors du lancement du projet n'est pas facilement visible
                algo.DefaultNumberOfIterations = 50;
                //algo.DefaultNumberOfIterations = 200;


                InferenceEngine = new InferenceEngine(algo);
                //InferenceEngine.OptimiseForVariables = new IVariable[] { Cells };

            }

            Dirichlet[] dirArray = Enumerable.Repeat(Dirichlet.Uniform(CellDomain.Count), CellIndices.Count).ToArray();

            //On affecte les valeurs fournies par le masque à résoudre en affectant les distributions de probabilités initiales

            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    int cellIndex80 = (rowIndex * 9) + colIndex;
                    if (s.Cellules[rowIndex][colIndex] > 0)
                    {
                        Vector v = Vector.Constant(CellDomain.Count, EpsilonProba);
                        v[s.Cellules[rowIndex][colIndex] - 1] = FixedValueProba;
                        dirArray[cellIndex80] = Dirichlet.PointMass(v);
                    }
                }
            }
                        /*Code original
                        foreach (var cellIndex in GrilleSudoku.IndicesCellules)
                        {
                            if (s.Cellules[cellIndex] > 0)
                            {

                                //Vector v = Vector.Zero(CellDomain.Count);
                                //v[s.Cellules[cellIndex] - 1] = 1.0;


                                //Todo: Alternative: le fait de mettre une proba non nulle permet d'éviter l'erreur "zero probability" du Sudoku Easy-n°2, mais le Easy#3 n'est plus résolu
                                //tentative de changer la probabilite pour solver le sudoku 3 infructueuse
                                Vector v = Vector.Constant(CellDomain.Count, EpsilonProba);
                                v[s.Cellules[cellIndex] - 1] = FixedValueProba;

                                dirArray[cellIndex] = Dirichlet.PointMass(v);
                            }
                        } */

            CellsPrior.ObservedValue = dirArray;


            Dirichlet[] cellsProbsPosterior = InferenceEngine.Infer<Dirichlet[]>(ProbCells);


            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    int cellIndex80 = (rowIndex * 9) + colIndex;
                    if (s.Cellules[rowIndex][colIndex] == 0)
                    {
                        var mode = cellsProbsPosterior[cellIndex80].GetMode();
                        var value = mode.IndexOf(mode.Max()) + 1;
                        s.Cellules[rowIndex][colIndex] = value;
                    }
                }
            }

            /*code original
            foreach (var cellIndex in GrilleSudoku.IndicesCellules)
            {
                if (s.Cellules[cellIndex] == 0)
                {

                    //s.Cellules[cellIndex] = cellValues[cellIndex];


                    var mode = cellsProbsPosterior[cellIndex].GetMode();
                    var value = mode.IndexOf(mode.Max()) + 1;
                    s.Cellules[cellIndex] = value;
                }
            }*/


        }

    }



    /// <summary>
    /// Ce premier modèle est très faible: d'une part, il ne résout que quelques Sudokus faciles, d'autre part, le modèle est recompilé à chaque fois, ce qui prend beaucoup de temps
    /// </summary>
    public class NaiveSudokuModel
    {

        private static List<int> CellDomain = Enumerable.Range(1, 9).ToList();
        private static List<int> CellIndices = Enumerable.Range(0, 81).ToList();

        //protected Shared.GridSudoku SolveSudoku(Shared.GridSudoku s)
        public virtual void SolveSudoku(GridSudoku s)
        {

            var algo = new ExpectationPropagation();
            var engine = new InferenceEngine(algo);

            //Implémentation naïve: une variable aléatoire entière par cellule
            var cells = new List<Variable<int>>(CellIndices.Count);


            for (int cellIndex = 0; cellIndex < 80; cellIndex++)
            {
                //On initialise le vecteur de probabilités de façon uniforme pour les chiffres de 1 à 9
                var baseProbas = Enumerable.Repeat(1.0, CellDomain.Count).ToList();
                //Création et ajout de la variable aléatoire
                var cell = Variable.Discrete(baseProbas.ToArray());
                cells.Add(cell);

            }

            //Ajout des contraintes de Sudoku (all diff pour tous les voisinages)
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    foreach (var neighbourCellIndex in GridSudoku.CellNeighbours[rowIndex][colIndex])
                    {
                        var cellIndex80 = rowIndex * 9 + colIndex;
                        var neighborCellIndex80 = neighbourCellIndex.row * 9 + neighbourCellIndex.column;

                        if (neighborCellIndex80 > cellIndex80)
                        {
                            Variable.ConstrainFalse(s.Cellules[rowIndex][colIndex] == s.Cellules[neighbourCellIndex.row][neighbourCellIndex.column]);
                        }
                    }
                }

            }

            /* Code original
            foreach (var cellIndex in GrilleSudoku.IndicesCellules) 
            {
                foreach (var neighbourCellIndex in GridSudoku.CellNeighbours[rowIndex][colIndex])
                {
                    if (neighbourCellIndex > cellIndex)
                    {
                        Variable.ConstrainFalse(cells[cellIndex] == cells[neighbourCellIndex]);
                    }
                }
            }*/

            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    var cellIndex80 = rowIndex * 9 + colIndex;
                    if (s.Cellules[rowIndex][colIndex] > 0)
                    {
                        cells[cellIndex80].ObservedValue = s.Cellules[rowIndex][colIndex] - 1;
                    }
                }
            }

            /* Code original
            //On affecte les valeurs fournies par le masque à résoudre comme variables observées
            foreach (var cellIndex in GrilleSudoku.IndicesCellules)
            {
                if (s.Cellules[cellIndex] > 0)
                {
                    cells[cellIndex].ObservedValue = s.Cellules[cellIndex] - 1;
                }
            }*/

            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    int cellIndex80 = (rowIndex * 9) + colIndex;
                    if (s.Cellules[rowIndex][colIndex] == 0)
                    {
                        Discrete result = (Discrete)engine.Infer(cells[cellIndex80]);
                        s.Cellules[rowIndex][colIndex] = result.Point + 1;
                    }
                }
            }

            /* Code original
            foreach (var cellIndex in GrilleSudoku.IndicesCellules)
            {
                if (s.Cellules[cellIndex] == 0)
                {
                    var result = (Discrete)engine.Infer(cells[cellIndex]);
                    s.Cellules[cellIndex] = result.Point + 1;
                }
            }*/

        }
    }
}

            


