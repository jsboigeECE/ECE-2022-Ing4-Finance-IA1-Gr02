using System.Linq;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Sudoku.Shared;

namespace Sudoku.GeneticSharpsSolver
{
    public class CellsChromosomeSolver : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {
            GridSudoku toReturn = s;
            var populationSize = 500;
            do
            {
                var fitnessThreshold = 0;
                var generationNb = 50;

                var sudokuChromosome = new SudokuCellsChromosome(s);
                var fitness = new SudokuFitness(s);
                var selection = new EliteSelection();
                var crossover = new UniformCrossover();
                var mutation = new UniformMutation();

                var population = new Population(populationSize, populationSize, sudokuChromosome);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
                {
                    Termination = new OrTermination(new ITermination[]
                    {
                    new FitnessThresholdTermination(fitnessThreshold),
                    new GenerationNumberTermination(generationNb)
                    })
                };

                ga.Start();

                var bestIndividual = ((ISudokuChromosome)ga.Population.BestChromosome);
                var solutions = bestIndividual.GetSudokus();
                if (solutions.Count > 0)
                {
                    toReturn = solutions.First();
                    if (toReturn.NbErrors(s)==0)
                    {
                        break;
                    }
                }
                populationSize *= 5;
            } while (true);
            

            return toReturn;
        }
    }

    public class PermutationsChromosomeSolver : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {

            var populationSize = 1000;
            var fitnessThreshold = 0;
            var generationNb = 50;

            var sudokuChromosome = new SudokuPermutationsChromosome(s);
            var fitness = new SudokuFitness(s);
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation();

            var population = new Population(populationSize, populationSize, sudokuChromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new OrTermination(new ITermination[]
                {
                    new FitnessThresholdTermination(fitnessThreshold),
                    new GenerationNumberTermination(generationNb)
                })
            };

            ga.Start();

            var bestIndividual = ((ISudokuChromosome)ga.Population.BestChromosome);
            var solutions = bestIndividual.GetSudokus();
            if (solutions.Count > 0)
            {
                return solutions.First();
            }

            return s;
        }
    }

    public class RandomPermutationChromosomeSolver : ISolverSudoku
    {
        public GridSudoku Solve(GridSudoku s)
        {

            var populationSize = 1000;
            var fitnessThreshold = 0;
            var generationNb = 50;

            var sudokuChromosome = new SudokuRandomPermutationsChromosome(s, 10, 1);
            var fitness = new SudokuFitness(s);
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation();

            var population = new Population(populationSize, populationSize, sudokuChromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new OrTermination(new ITermination[]
                {
                    new FitnessThresholdTermination(fitnessThreshold),
                    new GenerationNumberTermination(generationNb)
                })
            };

            ga.Start();

            var bestIndividual = ((ISudokuChromosome)ga.Population.BestChromosome);
            var solutions = bestIndividual.GetSudokus();
            if (solutions.Count > 0)
            {
                return solutions.First();
            }

            return s;
        }
    }
}