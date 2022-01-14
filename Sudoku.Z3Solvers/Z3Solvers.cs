using System;
using System.Collections.Generic;
using Microsoft.Z3;
using Sudoku.Shared;





namespace Sudoku.Z3Solvers
{

    public class Z3SubstitutionsSolver : Z3SolverBase
    {
        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolveWithSubstitutions(s);
        }

    }

    public class Z3ScopeSolver : Z3SolverBase
    {
        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolveWithScope(s);
        }

    }

    public class Z3AsumptionsSolver : Z3SolverBase
    {
        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolveWithAsumptions(s);
        }

    }


    public class Z3CleanSolver : Z3SolverBase
    {
        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
           return SolveOriginalCleanup(s);
        }

    }



    public class Z3InitialSolver:Z3SolverBase
    {

        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return SolveOriginalVersion(s);
            //  z3Context.MkTactic("smt");

        }

    }

    public abstract class Z3SolverBase : ISolverSudoku
    {
        public abstract GridSudoku Solve(Shared.GridSudoku s);

        protected static Context z3Context = new Context();

        // 9x9 matrix of integer variables
        static IntExpr[][] X = new IntExpr[9][];
        static BoolExpr _GenericContraints;

        private static Solver _reusableZ3Solver;

        static Z3SolverBase()
        {
            PrepareVariables();
        }

        public static BoolExpr GenericContraints
        {
            get
            {
                if (_GenericContraints == null)
                {
                    _GenericContraints = GetGenericConstraints();
                }
                return _GenericContraints;
            }
        }

        public static Solver ReusableZ3Solver
        {
            get
            {
                if (_reusableZ3Solver == null)
                {
                    _reusableZ3Solver = z3Context.MkSolver();
                    _reusableZ3Solver.Assert(GenericContraints);
                }
                return _reusableZ3Solver;

            }
        }

      



        BoolExpr GetPuzzleConstraint(Shared.GridSudoku instance)
        {
            BoolExpr instance_c = z3Context.MkTrue();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (instance.Cellules[i][j] != 0)
                    {
                        instance_c = z3Context.MkAnd(instance_c,
                            (BoolExpr)
                            z3Context.MkEq(X[i][j], z3Context.MkInt(instance.Cellules[i][j])));
                    }
            return instance_c;
        }

        static void PrepareVariables()
        {
            for (uint i = 0; i < 9; i++)
            {
                X[i] = new IntExpr[9];
                for (uint j = 0; j < 9; j++)
                    X[i][j] = (IntExpr)z3Context.MkConst(z3Context.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), z3Context.IntSort);
            }
        }

        static BoolExpr GetGenericConstraints()
        {

            // each cell contains a value in {1, ..., 9}
            BoolExpr[][] cells_c = new BoolExpr[9][];
            for (uint i = 0; i < 9; i++)
            {
                cells_c[i] = new BoolExpr[9];
                for (uint j = 0; j < 9; j++)
                    cells_c[i][j] = z3Context.MkAnd(z3Context.MkLe(z3Context.MkInt(1), X[i][j]),
                        z3Context.MkLe(X[i][j], z3Context.MkInt(9)));
            }

            // each row contains a digit at most once
            BoolExpr[] rows_c = new BoolExpr[9];
            for (uint i = 0; i < 9; i++)
                rows_c[i] = z3Context.MkDistinct(X[i]);


            // each column contains a digit at most once
            BoolExpr[] cols_c = new BoolExpr[9];
            for (uint j = 0; j < 9; j++)
            {
                Expr[] column = new Expr[9];
                for (uint i = 0; i < 9; i++)
                    column[i] = X[i][j];

                cols_c[j] = z3Context.MkDistinct(column);
            }

            // each 3x3 square contains a digit at most once
            BoolExpr[][] sq_c = new BoolExpr[3][];
            for (uint i0 = 0; i0 < 3; i0++)
            {
                sq_c[i0] = new BoolExpr[3];
                for (uint j0 = 0; j0 < 3; j0++)
                {
                    Expr[] square = new Expr[9];
                    for (uint i = 0; i < 3; i++)
                        for (uint j = 0; j < 3; j++)
                            square[3 * i + j] = X[3 * i0 + i][3 * j0 + j];
                    sq_c[i0][j0] = z3Context.MkDistinct(square);
                }
            }

            var toReturn = z3Context.MkTrue();
            foreach (BoolExpr[] t in cells_c)
                toReturn = z3Context.MkAnd(z3Context.MkAnd(t), toReturn);
            toReturn = z3Context.MkAnd(z3Context.MkAnd(rows_c), toReturn);
            toReturn = z3Context.MkAnd(z3Context.MkAnd(cols_c), toReturn);
            foreach (BoolExpr[] t in sq_c)
                toReturn = z3Context.MkAnd(z3Context.MkAnd(t), toReturn);
            return toReturn;
        }

        protected Shared.GridSudoku SolveWithSubstitutions(Shared.GridSudoku instance)
        {

            var substExprs = new List<Expr>();
            var substVals = new List<Expr>();

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (instance.Cellules[i][j] != 0)
                    {
                        substExprs.Add(X[i][j]);
                        substVals.Add(z3Context.MkInt(instance.Cellules[i][j]));
                    }
            BoolExpr instance_c = (BoolExpr)GenericContraints.Substitute(substExprs.ToArray(), substVals.ToArray());

            var z3Solver = GetSolver();
            z3Solver.Assert(instance_c);

            if (z3Solver.Check() == Status.SATISFIABLE)
            {
                instance = instance.CloneSudoku();
                Model m = z3Solver.Model;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        if (instance.Cellules[i][j] == 0)
                        {
                            instance.Cellules[i][j] =((IntNum)m.Evaluate(X[i][j])).Int;
                        }
                    }
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
            }
            return instance;
        }


        protected Shared.GridSudoku SolveWithAsumptions(Shared.GridSudoku instance)
        {

            BoolExpr instance_c = GetPuzzleConstraint(instance);
            var z3Solver = GetReusableSolver();
            if (z3Solver.Check(instance_c) == Status.SATISFIABLE)
            {
                instance = instance.CloneSudoku();
                Model m = z3Solver.Model;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        if (instance.Cellules[i][j] == 0)
                        {
                            instance.Cellules[i][j] =((IntNum)m.Evaluate(X[i][j])).Int;
                        }
                    }
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
            }
            return instance;
        }



        protected Shared.GridSudoku SolveWithScope(Shared.GridSudoku instance)
        {


            var z3Solver = GetReusableSolver();
            z3Solver.Push();
            BoolExpr instance_c = GetPuzzleConstraint(instance);
            z3Solver.Assert(instance_c);

            if (z3Solver.Check() == Status.SATISFIABLE)
            {
                instance = instance.CloneSudoku();
                Model m = z3Solver.Model;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        if (instance.Cellules[i][j] == 0)
                        {
                            instance.Cellules[i][j] =((IntNum)m.Evaluate(X[i][j])).Int;
                        }
                    }
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
            }
            z3Solver.Pop();
            return instance;
        }



        protected Shared.GridSudoku SolveOriginalCleanup(Shared.GridSudoku instance)
        {

            BoolExpr instance_c = GetPuzzleConstraint(instance);
            var z3Solver = GetSolver();
            z3Solver.Assert(GenericContraints);
            z3Solver.Assert(instance_c);

            if (z3Solver.Check() == Status.SATISFIABLE)
            {
                instance = instance.CloneSudoku();
                Model m = z3Solver.Model;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        if (instance.Cellules[i][j] == 0)
                        {
                            instance.Cellules[i][j] =((IntNum)m.Evaluate(X[i][j])).Int;
                        }
                    }
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
            }
            return instance;
        }

        protected virtual Solver GetSolver()
        {
            return z3Context.MkSolver();
        }

        protected virtual Solver GetReusableSolver()
        {
            return ReusableZ3Solver;
        }

        protected Shared.GridSudoku SolveOriginalVersion(Shared.GridSudoku instance)
        {
            BoolExpr instance_c = z3Context.MkTrue();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    instance_c = z3Context.MkAnd(instance_c,
                        (BoolExpr)
                        z3Context.MkITE(z3Context.MkEq(z3Context.MkInt(instance.Cellules[i][j]), z3Context.MkInt(0)),
                            z3Context.MkTrue(),
                            z3Context.MkEq(X[i][j], z3Context.MkInt(instance.Cellules[i][j]))));

            Solver s = z3Context.MkSolver();
            s.Assert(GenericContraints);
            s.Assert(instance_c);

            if (s.Check() == Status.SATISFIABLE)
            {
                instance = instance.CloneSudoku();
                Model m = s.Model;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        instance.Cellules[i][j] =((IntNum)m.Evaluate(X[i][j])).Int;
                    }
                return instance;


            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
                return instance;
            }
        }


    }
}
