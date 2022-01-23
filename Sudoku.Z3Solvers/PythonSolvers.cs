using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.Z3Solvers
{

   


    public class Z3PythonDotNetSolver : PythonSolverBase
    {

        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            //System.Diagnostics.Debugger.Break();

            //For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Person object to a PyObject
                PyObject pySudoku = s.ToPython();

                // create a Python variable "person"
                scope.Set("sudoku", pySudoku);

                // the person object may now be used in Python
                string code = Resources.SelfCallSolver_py;
                scope.Exec(code);
                var result = scope.Get("solvedSudoku");
                var toReturn = result.As<Shared.GridSudoku>();
                return toReturn;
            }
            //}

        }

    }

    public class Z3PythonNativeSolver : PythonSolverBase
    {


        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {

            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Person object to a PyObject
                PyObject pyCells = s.Cellules.ToPython();

                // create a Python variable "person"
                scope.Set("instance", pyCells);

                // the person object may now be used in Python
                string code = Resources.Z3Solver_py;
                scope.Exec(code);
                var result = scope.Get("r");
                var managedResult = result.As<int[][]>();
                //var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();
                return new Shared.GridSudoku() { Cellules = managedResult };
            }
            //}

        }

        protected override void InitializePythonComponents()
        {
            InstallPipModule("z3-solver");
            base.InitializePythonComponents();
        }



    }


}
