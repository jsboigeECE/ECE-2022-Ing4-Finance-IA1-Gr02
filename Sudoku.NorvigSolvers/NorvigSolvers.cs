using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.NorvigSolvers
{
    public class NorvigSolver : ISolverSudoku
    {
        // Création de chaque case (carré) : A1, A2.... I8, I9
        static string[] Cross(string A, string B)
        {
            return (from a in A from b in B select "" + a + b).ToArray();
        }

        static string V = "";
        static string rows = "ABCDEFGHI";
        static string cols = "123456789";
        static string digits = "123456789";
        static string[] squares = Cross(rows, cols);
        static Dictionary<string, IEnumerable<string>> peers;
        static Dictionary<string, IGrouping<string, string[]>> units;

        static NorvigSolver()
        {
            // Création des "units" (unités) et "peers" (paires)
            var unitlist = ((from c in cols select Cross(rows, c.ToString()))
                               .Concat(from r in rows select Cross(r.ToString(), cols))
                               .Concat(from rs in (new[] { "ABC", "DEF", "GHI" }) from cs in (new[] { "123", "456", "789" }) select Cross(rs, cs)));
            // Unit = collection de 9 carrés/cases (colonne, ligne, ou bloc) 
            units = (from s in squares from u in unitlist where u.Contains(s) group u by s into g select g).ToDictionary(g => g.Key);
            // Peer = carrés qui partagent une unit          
            peers = (from s in squares from u in units[s] from s2 in u where s2 != s group s2 by s into g select g).ToDictionary(g => g.Key, g => g.Distinct());
        }

        static string[][] Zip(string[] A, string[] B)
        {
            var n = Math.Min(A.Length, B.Length);
            string[][] sd = new string[n][];
            for (var i = 0; i < n; i++)
            {
                sd[i] = new string[] { A[i].ToString(), B[i].ToString() };
            }
            return sd;
        }

        // Convertit la grille du sudoku en un long String pour la méthode de Norvig
        public String Conversion(GridSudoku s)
        {
            String sudoku = "";

            // On remplace chaque case du nouveau tableau par la grille passée en paramètre
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku = sudoku.Insert(sudoku.Length , Convert.ToString(s.Cellules[i][j]));
                }
            }
            return sudoku;
        }

        /*
         * Crée le dictionnaire de la grille du sudoku.
         * Assigne les valeurs déjà présente sur la grille.
         * Et assigne les valeurs potentielles que peut prendre un carré.
         * (ie toutes les valeurs sauf celles impossibles).
         */
        public static Dictionary<string, string> Parse_grid(string grid)
        {
            var grid2 = from c in grid where "0.-123456789".Contains(c) select c;
            var values = squares.ToDictionary(s => s, s => digits); //To start, every square can be any digit

            foreach (var sd in Zip(squares, (from s in grid select s.ToString()).ToArray()))
            {
                var s = sd[0];
                var d = sd[1];

                if (digits.Contains(d) && Assign(values, s, d) == null)
                {
                    return null;
                }
            }
            return values;
        }

        // Essaie toutes les valeurs possibles en utilisant "Using depth-first search and propagation"
        public static Dictionary<string, string> Search(Dictionary<string, string> values)
        {
            if (values == null)
            {
                return null; // Failed earlier
            }
            if (All(from s in squares select values[s].Length == 1 ? "" : null))
            {
                return values; // Solved!
            }

            // Chose the unfilled square s with the fewest possibilities
            var s2 = (from s in squares where values[s].Length > 1 orderby values[s].Length ascending select s).First();

            return Some(from d in values[s2]
                        select Search(Assign(new Dictionary<string, string>(values), s2, d.ToString())));
        }

        // La fonction assign(values, s, d) renverra les valeurs mises à jour (y compris les mises à jour de la propagation des contraintes)
        // mais s'il y a une contradiction - si l'affectation ne peut pas être effectuée de manière cohérente - alors assign renvoie False .
        static Dictionary<string, string> Assign(Dictionary<string, string> values, string s, string d)
        {
            if (All(
                    from d2 in values[s]
                    where d2.ToString() != d
                    select Eliminate(values, s, d2.ToString())))
            {
                return values;
            }
            return null;
        }

        //  Élimine toutes les autres valeurs (excepter d) de la valeur values[s] et la propage.
        static Dictionary<string, string> Eliminate(Dictionary<string, string> values, string s, string d)
        {
            if (!values[s].Contains(d))
            {
                return values;
            }
            values[s] = values[s].Replace(d, "");
            if (values[s].Length == 0)
            {
                return null; //Contradiction: removed last value
            }
            else if (values[s].Length == 1)
            {
                //If there is only one value (d2) left in square, remove it from peers
                var d2 = values[s];
                if (!All(from s2 in peers[s] select Eliminate(values, s2, d2)))
                {
                    return null;
                }
            }

            //Now check the places where d appears in the units of s
            foreach (var u in units[s])
            {
                var dplaces = from s2 in u where values[s2].Contains(d) select s2;
                if (dplaces.Count() == 0)
                {
                    return null;
                }
                else if (dplaces.Count() == 1)
                {
                    // d can only be in one place in unit; assign it there
                    if (Assign(values, dplaces.First(), d) == null)
                    {
                        return null;
                    }
                }
            }
            return values;
        }

        // Retourne Vrai si tous les éléments de l'input sont vrais
        static bool All<T>(IEnumerable<T> seq)
        {
            foreach (var e in seq)
            {
                if (e == null) return false;
            }
            return true;
        }
        static T Some<T>(IEnumerable<T> seq)
        {
            foreach (var e in seq)
            {
                if (e != null) return e;
            }
            return default(T);
        }
        static string Center(string s, int width)
        {
            var n = width - s.Length;
            if (n <= 0) return s;
            var half = n / 2;

            if (n % 2 > 0 && width % 2 > 0) half++;

            return new string(' ', half) + s + new String(' ', n - half);
        }

        // Retourne le sudoku résolu
        public GridSudoku Solve(GridSudoku sudo)
        {
            String grid = "";

            // Conversion de la grille en String
            grid = Conversion(sudo);

            // Résolution du sudoku par l'appel des fonctions 
            var values = Search(Parse_grid(grid));

            // Conversion du sudoku résolu dans le bon type GridSudoku
            foreach (var value in values)
            {
                int rowIndex = value.Key[0] - 'A';
                int colIndex = int.Parse(value.Key[1].ToString()) - 1;
                sudo.Cellules[rowIndex][colIndex] = int.Parse(value.Value.ToString());
            }
            return sudo;
        }
    }
}
