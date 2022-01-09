using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Sudoku.Shared;

namespace Sudoku.Benchmark
{



    [Config(typeof(Config))]
    public class QuickBenchmarkSolvers: BenchmarkSolversBase
    {
        public QuickBenchmarkSolvers()
        {
            MaxSolverDuration = TimeSpan.FromSeconds(5);
        }
        private class Config : ManualConfig
        {
            public Config()
            {
#if DEBUG
                Options |= ConfigOptions.DisableOptimizationsValidator;
#endif
                this.AddColumn(new RankColumn(NumeralSystem.Arabic));
                AddJob(Job.Dry
                    .WithId("Solving Sudokus")
                    .WithPlatform(Platform.X64)
                    .WithJit(Jit.Default)
                    .WithRuntime(CoreRuntime.Core31)
                    //.WithLaunchCount(1)
                    //.WithWarmupCount(1)
                    .WithIterationCount(1)
                    .WithInvocationCount(1)

                );


            }
        }

        public override SudokuDifficulty Difficulty { get; set; } = SudokuDifficulty.Medium;

       
    }


    [Config(typeof(Config))]
    public class CompleteBenchmarkSolvers: BenchmarkSolversBase
    {

        public CompleteBenchmarkSolvers()
        {
            MaxSolverDuration = TimeSpan.FromMinutes(5);
        }

        private class Config : ManualConfig
        {
            public Config()
            {
#if DEBUG
                Options |= ConfigOptions.DisableOptimizationsValidator;
#endif
                this.AddColumn(new RankColumn(NumeralSystem.Arabic));
                AddJob(Job.Dry
                    .WithId("Solving Sudokus")
                    .WithPlatform(Platform.X64)
                    .WithJit(Jit.RyuJit)
                    .WithRuntime(CoreRuntime.Core31)
                    //.WithLaunchCount(1)
                    //.WithWarmupCount(1)
                    .WithIterationCount(3)
                    .WithInvocationCount(3)

                );


            }
        }

        [ParamsAllValues]
        public override SudokuDifficulty Difficulty { get; set; }


    }





    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public abstract class BenchmarkSolversBase
    {

        static BenchmarkSolversBase()
        {
            _Solvers = new[] { new EmptySolver() }.Concat(Shared.GridSudoku.GetSolvers().Select(s=>s.Value.Value).Where(s => s.GetType() != typeof(EmptySolver))).Select(s => new SolverPresenter() { Solver = s }).ToList();
            //_Solvers = SudokuGrid.GetSolvers().Where(s => s.GetType().Name.ToLowerInvariant().StartsWith("dl")).Select(s => new SolverPresenter() { Solver = s });
        }


        [GlobalSetup]
        public virtual void GlobalSetup()
        {
            AllPuzzles = new Dictionary<SudokuDifficulty, IList<Shared.GridSudoku>>();
            foreach (var difficulty in Enum.GetValues(typeof(SudokuDifficulty)).Cast<SudokuDifficulty>())
            {
                AllPuzzles[difficulty] = SudokuHelper.GetSudokus(Difficulty);
            }

        }

        [IterationSetup]
        public void IterationSetup()
        {
            IterationPuzzles = new List<Shared.GridSudoku>(NbPuzzles);
            for (int i = 0; i < NbPuzzles; i++)
            {
                IterationPuzzles.Add(AllPuzzles[Difficulty][i].CloneSudoku());
            }
            SolverPresenter.Solver.Solve(Shared.GridSudoku.ReadSudoku("483921657967345001001806400008102900700000008006708200002609500800203009005010300"));

        }

        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        public TimeSpan MaxSolverDuration;

        public int NbPuzzles { get; set; } = 10;

        public virtual SudokuDifficulty Difficulty { get; set; }

        public IDictionary<SudokuDifficulty, IList<Shared.GridSudoku>> AllPuzzles { get; set; }
        public IList<Shared.GridSudoku> IterationPuzzles { get; set; }

        [ParamsSource(nameof(GetSolvers))]
        public SolverPresenter SolverPresenter { get; set; }


        private static IEnumerable<SolverPresenter> _Solvers;



        public IEnumerable<SolverPresenter> GetSolvers()
        {
            return _Solvers;

        }


        [Benchmark(Description = "Benchmarking GrilleSudoku Solvers")]
        public void Benchmark()
        {
            foreach (var puzzle in IterationPuzzles)
            {
                try
                {
                    Console.WriteLine($"Solver {SolverPresenter} solving sudoku: \n {puzzle}");
                    var startTime = Clock.Elapsed;
                    var solution = SolverPresenter.SolveWithTimeLimit(puzzle, MaxSolverDuration);
                    if (!solution.IsValid(puzzle))
                    {
                        throw new ApplicationException($"sudoku has {solution.NbErrors(puzzle)} errors");
                    }

                    var duration = Clock.Elapsed - startTime;
                    var durationSeconds = (int)duration.TotalSeconds;
                    var durationMilliSeconds = duration.TotalMilliseconds - (1000 * durationSeconds);
                    Console.WriteLine($"Valid Solution found: \n {solution} \n Solver {SolverPresenter} found the solution  in {durationSeconds} s {durationMilliSeconds} ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

    }
}