using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.Graphecolore
{
    /// <summary>
    /// La classe Graphe représente un graphe dans son ensemble
    /// </summary>
    public class Graphe
    {
        /// Le réseau est constitué d'une collection de sommets
        List<Sommet> m_sommets;
        GridSudoku m_grid;
        public const int m_ordre = 81;

        /// La construction du réseau se fait à partir d'une grille Sudoku
        public Graphe(GridSudoku grid)
        {
            m_grid = grid.CloneSudoku();
            initGraphe();
        }

        // Ajout de l'ensemble des sommets dans le graphe
        void initGraphe()
        {
            m_sommets = new List<Sommet>();
            int ligne, colonne;
            for (int i=0; i<m_ordre; i++)
            {
                ligne = (int)(i/9);
                colonne = i%9;
                m_sommets.Add(new Sommet(i, m_grid.Cellules[ligne][colonne]));
            }
            foreach (Sommet s in m_sommets)
            {
                s.determineAdjacents(m_sommets);
            }
        }

        // Retourne l'objet GridSudoku contenant la grille Sudoku à valider du graphe
        public GridSudoku getGrid()
        {
            return m_grid;
        }

        public void displayGrid()
        {
            Console.WriteLine("----------------------------------");
            int i = 0;
            foreach (Sommet s in m_sommets)
            {
                if (i % 3 == 0)
                    Console.Write("| ");
                Console.Write("{0,2:#0} ", s.getCouleur());
                i++;
                if (i % 9 == 0)
                    Console.WriteLine("|");
                if (i % 27 == 0)
                    Console.WriteLine("----------------------------------");
            }
            Console.WriteLine();
        }

        public void algoNaifOptimise()
        {
            // Si la première case contient déjà une couleur,
            // on lance l'algorithme sur cette couleur
            if (m_sommets.First().getCouleur() != 0)
                attribuerCouleurGraphe(0, 0, m_sommets.First().getCouleur());
            else
            {
                // On lance l'algorithme en testant l'ensemble des couleurs de 1 à 9
                // sur la première case
                for (int colour=1; colour<=9; colour++)
                {
                    if (attribuerCouleurGraphe(0, 0, colour))
                    {
                        break;
                    }
                }
            }
            // On a trouvé la solution. On met à jour la grille
            for (int i=0; i<m_sommets.Count; i++)
                m_grid.Cellules[(int)(i/9)][i%9] = m_sommets.ElementAt(i).getCouleur();
        }

        // Cette fonction renvoie :
        //      - true si le graphe a pu être entièrement complété
        //      - false sinon
        bool attribuerCouleurGraphe(int nbOk, int nbCouleurs, int couleur)
        {
            // On parcourt la liste des sommets
            Sommet s = m_sommets.ElementAt(nbOk);

            int couleurBackup = 0;
            // On vérifie si la case a déjà une couleur issue de la grille initiale
            // Si oui, on sauvegarde sa couleur
            if (s.getCouleur() != 0)
                couleurBackup = s.getCouleur();
            else
            {
                // On teste la couleur passée en paramètre auprès des adjacents
                if (s.testCouleur(couleur))
                {
                    // La couleur n'est pas déjà utilisée par un adjacent
                    // On sauvegarde l'absence de couleur
                    couleurBackup = 0;
                    // Puis on affecte la couleur à la case
                    s.setCouleur(couleur);
                }
            }
            // On vérifie si la case a désormais une couleur
            if (s.getCouleur() != 0)
            {
                // On met à jour le nombre de couleurs
                nbCouleurs++;
                nbOk++;
                // Si nbOk == m_ordre, on a rempli la grille Sudoko
                if (nbOk == m_ordre)
                    return true;
                // Si le nombre de couleurs est à 9, on a une ligne complète
                // On réinitialise le nombre de couleurs à 0
                nbCouleurs = nbCouleurs % 9;

                // Si la case suivante contient déjà une couleur,
                // on lance l'algorithme sur cette couleur
                if (m_sommets.ElementAt(nbOk).getCouleur() != 0)
                {
                    if (attribuerCouleurGraphe(nbOk, nbCouleurs, m_sommets.ElementAt(nbOk).getCouleur()))
                        return true;
                }
                else
                {
                    // On lance l'algorithme en testant l'ensemble des couleurs de 1 à 9
                    // sur la case suivante
                    for (int colour=1; colour<=9; colour++)
                    {
                        if (attribuerCouleurGraphe(nbOk, nbCouleurs, colour))
                            return true;
                    }
                }

                // Si on arrive ici, c'est que la résolution a échoué
                // On restaure la couleur initiale de la case (ou l'absence de couleur)
                s.setCouleur(couleurBackup);
            }
            return (nbOk == m_ordre);
        }
    }
}