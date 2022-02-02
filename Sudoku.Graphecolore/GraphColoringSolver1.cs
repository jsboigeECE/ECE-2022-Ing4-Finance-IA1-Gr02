using System;
using Sudoku.Shared;

namespace Sudoku.Graphecolore
{
    public class GraphColoringSolver1 : ISolverSudoku
    {
        GridSudoku ISolverSudoku.Solve(GridSudoku grid)
        {
            try
            {
                // Chargement du réseau et affichage avant coloration
                Graphe graphe = new Graphe(grid);
                Console.WriteLine();
                Console.WriteLine("Affichage de la grille a completer");
                graphe.displayGrid();

                // Coloration algorithme naif optimisé
                Console.WriteLine("Parcours du graphe en utilisant un algorithme naif optimise");
                graphe.algoNaifOptimise();
                Console.WriteLine("Affichage du resultat");
                graphe.displayGrid();
                Console.WriteLine();
                Console.WriteLine("Verification du resultat");
                return graphe.getGrid();
            }
            catch (Exception e)
            {
                Console.Write("Attention ", e);
            }
            return grid;
        }
    }
}