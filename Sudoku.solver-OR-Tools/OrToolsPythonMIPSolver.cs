using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.solver_OR_Tools
{
    public class OrToolsPythonMIPSolver : PythonSolverBase
    {
        public override GridSudoku Solve(GridSudoku s)
        {
            using (PyModule scope = Py.CreateScope())
            {
                // convert the sudoku cell array object to a pyobject
                PyObject pycells = s.Cellules.ToPython();
                // create a python variable with the array
                scope.Set("grid", pycells);

                //todo: il reste à personaliser votre script pour qu'après la définition de votre fonction, vous en fassiez usage à partir de la variable "grid" ici définie, et vous construisiez la variable "r" de résultat au même format récupérée après exécution 
                
                string code = Resource.Optimal_py;
                scope.Exec(code);
                //retrieve the r variable, which should hold the same format as the original grid variable
                var result = scope.Get("r");
                var managedresult = result.As<int[][]>();
                //var convertesdresult = managedresult.select(objlist => objlist.select(o => (int)o).toarray()).toarray();
                return new GridSudoku() { Cellules = managedresult };
            }
        }

        protected override void InitializePythonComponents()
        {
            InstallPipModule("ortools");
            InstallPipModule("numpy");
            base.InitializePythonComponents();
        }
    }
}