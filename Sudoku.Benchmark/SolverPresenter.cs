﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Sudoku.Shared;

namespace Sudoku.Benchmark
{
    public class SolverPresenter
    {


        static SolverPresenter()
        {
            var sw = Stopwatch.StartNew();
            var start = sw.Elapsed;
            Task task = Task.Factory.StartNew(() => Console.WriteLine("Task Warmup Start"));
            task.Wait(TimeSpan.FromMilliseconds(1000));
            Console.WriteLine($"Task Warmup End - {sw.Elapsed - start}");
        }

        public ISolverSudoku Solver { get; set; }

        public override string ToString()
        {
            return Solver.GetType().Name;
        }

        public Shared.GridSudoku SolveWithTimeLimit(Shared.GridSudoku puzzle, TimeSpan maxDuration)
        {
            try
            {
                Shared.GridSudoku toReturn = puzzle.CloneSudoku();

                Task task = Task.Factory.StartNew(() => toReturn = Solver.Solve(toReturn));
                task.Wait(maxDuration);
                if (!task.IsCompleted)
                {
                    throw new ApplicationException($"Solver {ToString()} has exceeded the maximum allowed duration {maxDuration.TotalSeconds} seconds");
                }
                return toReturn;

            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

    }
}