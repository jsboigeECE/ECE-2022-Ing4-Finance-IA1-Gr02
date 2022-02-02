using System;
using System.Globalization;
using Sudoku.Shared;
using Python.Runtime;
using System.Linq;

namespace Sudoku.RecuitSimule
{

    public class RecuitSimuleSolvers : PythonSolverBase
    {

        public override Shared.GridSudoku Solve(Shared.GridSudoku s)
        {

            // Create Python scope
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Person object to a PyObject
                PyObject pyCells = s.Cellules.ToPython();

                // create a Python variable "person"
                scope.Set("instance", pyCells);

                // the person object may now be used in Python
                string code = Resources.RecuitSimuleSolver_py;
                scope.Exec(code);
                var result = scope.Get("solution");
                //var strResult = result.ToString();
                var managedResult = result.As<object[][]>()
                    .Select(row => row.Select(cell => int.Parse(cell.ToString(),CultureInfo.InvariantCulture )).ToArray()).ToArray(); 
                //strResult.Split("\n").Select(l=>l.Select(c => (int.Parse(c.ToString()))).ToArray()).ToArray(); //result.As < int[,]>();
                //var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();

                return new Shared.GridSudoku() { Cellules = managedResult};
            }
            //}

        }

        protected override void InitializePythonComponents()
        {
            InstallPipModule("numpy");
            base.InitializePythonComponents();
        }
    }
}