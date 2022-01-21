using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.NorvigSolvers
{
    public class NorvigSolvers : ISolverSudoku
    {
        public Shared.GridSudoku Solve(Shared.GridSudoku s)
        {
            return s;
            //  z3Context.MkTactic("smt");
        }

        // Création de chaque case (carré) : A1, A2.... I8, I9
        static string[] cross(string A, string B)
        {
            return (from a in A from b in B select "" + a + b).ToArray();
        }

        static string rows = "ABCDEFGHI";
        static string cols = "123456789";
        static string digits = "123456789";
        static string[] squares = cross(rows, cols);
        static Dictionary<string, IEnumerable<string>> peers;
        static Dictionary<string, IGrouping<string, string[]>> units;

        static NorvigSolvers()
        {
            // Création des "units" (unités) et "peers" (paires)
            var unitlist = ((from c in cols select cross(rows, c.ToString()))
                               .Concat(from r in rows select cross(r.ToString(), cols))
                               .Concat(from rs in (new[] { "ABC", "DEF", "GHI" }) from cs in (new[] { "123", "456", "789" }) select cross(rs, cs)));
            // Unit = collection de 9 carrés/cases (colonne, ligne, ou bloc) 
            units = (from s in squares from u in unitlist where u.Contains(s) group u by s into g select g).ToDictionary(g => g.Key);
            // Peer = carrés qui partagent une unit          
            peers = (from s in squares from u in units[s] from s2 in u where s2 != s group s2 by s into g select g).ToDictionary(g => g.Key, g => g.Distinct());
        }
    }

}
