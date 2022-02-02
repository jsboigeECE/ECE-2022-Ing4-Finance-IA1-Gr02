using System.Linq;
using Microsoft.ML.Probabilistic.Algorithms;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Models.Attributes;
using System.Collections.Generic;
using Sudoku.Shared;

namespace Sudoku.Probabilistic
{
    using System;
    using Sudoku.Shared;

    public abstract class GraphicalModelSudokuSolverBase : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {
            int[] sCells = s.Cellules.Flatten();

            SolveSudoku(sCells);

            var toReturn = new GridSudoku() { Cellules = sCells.ToJaggedArray(9) };

            return toReturn;

        }

        protected abstract void SolveSudoku(int[] sCells);


    }




    public class GraphicalModelRobustSolver : GraphicalModelSudokuSolverBase
    {

        public InferenceEngine InferenceEngine;

        protected static List<int> CellDomain = Enumerable.Range(1, 9).ToList();
        protected static List<int> CellIndices = Enumerable.Range(0, 81).ToList();


        // Cf https://en.wikipedia.org/wiki/Categorical_distribution et https://en.wikipedia.org/wiki/Categorical_distribution#Bayesian_inference_using_conjugate_prior pour le choix des distributions
        // et le chapitre 6 de https://dotnet.github.io/infer/InferNet101.pdf pour l'implémentation dans Infer.Net

        public VariableArray<Dirichlet> CellsPrior;
        public VariableArray<Vector> ProbCells;
        public VariableArray<int> Cells;

        protected const double EpsilonProba = 0.00000001;
        protected static double FixedValueProba = 1.0 - ((CellDomain.Count - 1) * EpsilonProba);

        public GraphicalModelRobustSolver()
        {

            var valuesRange = new Microsoft.ML.Probabilistic.Models.Range(CellDomain.Count).Named("valuesRange");
            var cellsRange = new Microsoft.ML.Probabilistic.Models.Range(CellIndices.Count).Named("cellsRange");


            CellsPrior = Variable.Array<Dirichlet>(cellsRange).Named("CellsPrior");
            ProbCells = Variable.Array<Vector>(cellsRange).Named("ProbCells");
            ProbCells[cellsRange] = Variable<Vector>.Random(CellsPrior[cellsRange]);


            ProbCells.SetValueRange(valuesRange);


            // Initialisation des distribution a priori de façon uniforme (les valeurs sont équiprobables pour chaque cellule)

            Dirichlet[] dirUnifArray =
                Enumerable.Repeat(Dirichlet.Uniform(CellDomain.Count), CellIndices.Count).ToArray();
            CellsPrior.ObservedValue = dirUnifArray;

            Cells = Variable.Array<int>(cellsRange);
            Cells[cellsRange] = Variable.Discrete(ProbCells[cellsRange]);



            //Ajout des contraintes de Sudoku (all diff pour tous les voisinages)
            foreach (var cellIndex in CellIndices)
            {
                int row = cellIndex / 9;
                int col = cellIndex - row * 9;
                foreach (var neighbourCellIndex in GridSudoku.CellNeighbours[row][col])
                {
                    int neighbourCellId = neighbourCellIndex.row * 9 + neighbourCellIndex.column;
                    if (neighbourCellId > cellIndex)
                    {
                        Variable.ConstrainFalse(Cells[cellIndex] == Cells[neighbourCellId]);
                    }
                }
            }


            //Todo: tester d'autres algo et paramétrages associés

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


        protected override void SolveSudoku(int[] sudokuCells)
        {
            Dirichlet[] dirArray = Enumerable.Repeat(Dirichlet.Uniform(CellDomain.Count), CellIndices.Count).ToArray();

            //On affecte les valeurs fournies par le masque à résoudre en affectant les distributions de probabilités initiales
            foreach (var cellIndex in CellIndices)
            {
                if (sudokuCells[cellIndex] > 0)
                {
                    //Vector v = Vector.Zero(CellDomain.Count);
                    //v[s.Cellules[cellIndex] - 1] = 1.0;


                    //Todo: Alternative: le fait de mettre une proba non nulle permet d'éviter l'erreur "zero probability" du Sudoku Easy-n°2, mais le Easy#3 n'est plus résolu
                    //tentative de changer la probabilite pour solver le sudoku 3 infructueuse
                    Vector v = Vector.Constant(CellDomain.Count, EpsilonProba);
                    v[sudokuCells[cellIndex] - 1] = FixedValueProba;

                    dirArray[cellIndex] = Dirichlet.PointMass(v);
                }
            }
            CellsPrior.ObservedValue = dirArray;


            DoInference(dirArray, sudokuCells);

        }

        protected virtual void DoInference(Dirichlet[] dirArray, int[] sudokuCells)
        {

            //Autre possibilité de variable d'inférence (bis)
            Dirichlet[] cellsProbsPosterior = InferenceEngine.Infer<Dirichlet[]>(ProbCells);

            foreach (var cellIndex in CellIndices)
            {
                if (sudokuCells[cellIndex] == 0)
                {
                    //s.Cellules[cellIndex] = cellValues[cellIndex];

                    var mode = cellsProbsPosterior[cellIndex].GetMode();
                    var value = mode.IndexOf(mode.Max()) + 1;
                    sudokuCells[cellIndex] = value;
                }
            }
        }


    }





    public class GraphicalModelNaiveSolver : GraphicalModelSudokuSolverBase
    {

        private static List<int> CellDomain = Enumerable.Range(1, 9).ToList();
        private static List<int> CellIndices = Enumerable.Range(0, 81).ToList();


        protected override void SolveSudoku(int[] sCells)
        {

            var algo = new ExpectationPropagation();
            var engine = new InferenceEngine(algo);

            //Implémentation naïve: une variable aléatoire entière par cellule
            var cells = new List<Variable<int>>(CellIndices.Count);

            foreach (var cellIndex in CellIndices)
            {
                //On initialise le vecteur de probabilités de façon uniforme pour les chiffres de 1 à 9
                var baseProbas = Enumerable.Repeat(1.0, CellDomain.Count).ToList();
                //Création et ajout de la variable aléatoire
                var cell = Variable.Discrete(baseProbas.ToArray());
                cells.Add(cell);
            }

            //Ajout des contraintes de Sudoku (all diff pour tous les voisinages)
            foreach (var cellIndex in CellIndices)
            {
                foreach (var neighbourCellIndex in GridSudoku.CellNeighbours[cellIndex / 9][cellIndex % 9])
                {
                    int neighbourCellId = neighbourCellIndex.row * 9 + neighbourCellIndex.column;
                    if (neighbourCellId > cellIndex)
                    {
                        Variable.ConstrainFalse(cells[cellIndex] == cells[neighbourCellId]);
                    }
                }
            }



            //On affecte les valeurs fournies par le masque à résoudre comme variables observées
            foreach (var cellIndex in CellIndices)
            {
                if (sCells[cellIndex] > 0)
                {
                    cells[cellIndex].ObservedValue = sCells[cellIndex] - 1;
                }
            }

            foreach (var cellIndex in CellIndices)
            {
                if (sCells[cellIndex] == 0)
                {
                    var result = (Discrete)engine.Infer(cells[cellIndex]);
                    sCells[cellIndex] = result.Point + 1;
                }
            }

        }



    }



    public class GraphicalModelIterativeSolver : GraphicalModelRobustSolver
    {

        public int NbIterationCells { get; set; } = 2;

        protected override void DoInference(Dirichlet[] dirArray, int[] sudokuCells)
        {

            int cellDiscovered = sudokuCells.Count(c => c > 0);

            // Iteration tant que l'on a pas découvert toutes les cases
            while (cellDiscovered < CellIndices.Count - 1)
            {

                Dirichlet[] cellsProbsPosterior = InferenceEngine.Infer<Dirichlet[]>(ProbCells);

                int[] bestCellsProbsPosteriorIndex = getBestDirichletSubArrayIndex(cellsProbsPosterior, NbIterationCells, sudokuCells);

                foreach (var index in bestCellsProbsPosteriorIndex)
                {
                    var mode = cellsProbsPosterior[index].GetMode();
                    var value = mode.IndexOf(mode.Max()) + 1;

                    Vector v = Vector.Constant(CellDomain.Count, EpsilonProba);
                    v[value - 1] = FixedValueProba;

                    dirArray[index] = Dirichlet.PointMass(v);

                    sudokuCells[index] = value;
                    cellDiscovered++;
                }

                CellsPrior.ObservedValue = dirArray;
            }
        }

        private int[] getBestDirichletSubArrayIndex(Dirichlet[] dirichletArray, int N, int[] sudokuCells)
        {
            // Initialise la liste des N meilleurs index avec les N premiers index de dirichletArray pour les cellules vides
            int[] bestDirIndex = sudokuCells.Select((cell, index) => cell == 0 ? index : -1).Where(index => index != -1).Take(N).ToArray();

            // Pour chaque cellule == 0 du sudoku
            foreach (var cellIndex in CellIndices)
            {
                if (sudokuCells[cellIndex] == 0)
                {
                    var currentMode = dirichletArray[cellIndex].GetMode();

                    int minDirIndex = bestDirIndex[0];

                    // Récupère l'index du Dirichlet le plus petit de la liste d'index des meilleurs Dirichlet
                    foreach (var index in bestDirIndex)
                    {
                        var currentDirMode = dirichletArray[index].GetMode();
                        var minDirMode = dirichletArray[minDirIndex].GetMode();

                        if (currentDirMode.Max() < minDirMode.Max())
                        {
                            minDirIndex = index;
                        }
                    }
                    // Remplace ce Dirichlet si la valeurs max du Dirichlet de la cellule actuelle est supèrieur
                    if (dirichletArray[minDirIndex].GetMode().Max() < currentMode.Max())
                    {
                        bestDirIndex[Array.IndexOf(bestDirIndex, minDirIndex)] = cellIndex;
                    }
                }
            }
            return bestDirIndex;
        }

    }


}