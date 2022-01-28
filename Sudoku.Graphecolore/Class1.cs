using System;
using Sudoku;
using Sudoku.Shared;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Sudoku.Graphecolore
{
    public class graphcoloringsolver1 : ISolverSudoku
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
                // Mise à jour de la grille résultat
                graphe.updateGrid();
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


    // La classe Sommet représente un sommet du graphe
    // Les cases adjacences sont déduites des positions ligne,colonne des Sommets
    public class Sommet
    {
        // Voisinage : liste d'adjacence
        List<Sommet> m_adjacents;

        // Données spécifiques du sommet
        int m_id;        // Indice de la case dans la grille (de 0 à 80)
        int m_ligne, m_colonne; // Position dans la grille
        int m_couleur; // couleur affectée (ou 0 si pas encore de couleur)

        // Constructeur
        public Sommet(int indice, int ligne, int colonne, int couleur)
        {
            m_id = indice;
            m_ligne = ligne;
            m_colonne = colonne;
            m_couleur = couleur;
            m_adjacents = new List<Sommet>();
        }

        // Retourne la liste des adjacents
        public List<Sommet> getAdjacents()
        {
            return m_adjacents;
        }

        // Méthode de détermination des sommets adjacents
        public void determineAdjacents(List<Sommet> sommets)
        {
            m_adjacents = new List<Sommet>();
            foreach (Sommet s in sommets)
            {
                if (s != this)
                {
                    if (s.m_ligne == m_ligne || s.m_colonne == m_colonne || s.m_id == m_id)
                        m_adjacents.Add(s);
                }
            }
        }

        // Retourne la ligne dans la grille actuellement affectée au sommet
        public int getLigne()
        {
            return m_ligne;
        }

        // Retourne la colonne dans la grille actuellement affectée au sommet
        public int getColonne()
        {
            return m_ligne;
        }

        // Retourne la couleur (numéro de couleur) actuellement affecté au sommet
        // Par convention la valeur 0 indique "pas encore de couleur affectée"
        public int getCouleur()
        {
            return m_couleur;
        }

        // Affecte une couleur au sommet
        public void setCouleur(int couleur)
        {
            m_couleur = couleur;
        }

        // Teste l'affectation d'une couleur
        // Retourne vrai si la couleur n'est pas en conflit avec un sommet adjacent
        // faux sinon
        public bool testCouleur(int couleur)
        {
            foreach (Sommet s in m_adjacents)
                if (s.m_couleur == couleur)
                    return false;
            return true;
        }

        // Méthode d'affichage des objets de type Sommet
        public void display()
        {
            Console.Write("id " + m_id + "  ligne=" + m_ligne + "  colonne=" + m_colonne + "  couleur=" + m_couleur + "  Adjacents=");
            foreach (Sommet s in m_adjacents)
                Console.Write(s.m_id + " ");
            Console.WriteLine("");
        }
    }


    // La classe Graphe représente un graphe dans son ensemble
    public class Graphe
    {
        /// Le réseau est constitué d'une collection de sommets
        List<Sommet> m_sommets;
        GridSudoku m_grid;

        /// La construction du réseau se fait à partir d'une grille Sudoku
        public Graphe(GridSudoku grid)
        {
            m_grid = grid.CloneSudoku();
            reinitGrid();
        }

        // Retourne l'ordre du graphe (ordre = nombre de sommets)
        public int getOrdre()
        {
            return m_sommets.Count;
        }

        // Méthode d'affichage des objets de type Graphe
        public void display()
        {
            Console.WriteLine("Graphe d'ordre " + getOrdre() + " :");
            foreach (Sommet s in m_sommets)
                s.display();
            Console.WriteLine();
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

        public void reinitGrid()
        {
            m_sommets = new List<Sommet>();
            int bloc = 0;
            for (int ligne = 0; ligne < 9; ligne++)
                for (int colonne = 0; colonne < 9; colonne++)
                {
                    if (ligne < 3)
                        bloc = (int)(colonne / 3);
                    else if (ligne < 6)
                        bloc = 3 + (int)(colonne / 3);
                    else if (ligne < 9)
                        bloc = 6 + (int)(colonne / 3);
                    m_sommets.Add(new Sommet(bloc, ligne, colonne, m_grid.Cellules[ligne][colonne]));
                }
            foreach (Sommet s in m_sommets)
            {
                s.determineAdjacents(m_sommets);
            }
        }

        public void updateGrid()
        {
            for (int i=0; i<m_sommets.Count; i++)
                m_grid.Cellules[(int)(i/9)][i%9] = m_sommets.ElementAt(i).getCouleur();
        }

        public GridSudoku getGrid()
        {
            return m_grid;
        }

        public void display_m_grid()
        {
            Console.WriteLine("----------------------------------");
            int i = 0;
            for (int ligne=0; ligne<9; ligne++)
            {
                for (int colonne=0; colonne<9; colonne++)
                {
                    i = (ligne*9) + colonne; 
                    if (i % 3 == 0)
                        Console.Write("| ");
                    Console.Write("{0,2:#0} ", m_grid.Cellules[ligne][colonne]);
                    i++;
                    if (i % 9 == 0)
                        Console.WriteLine("|");
                    if (i % 27 == 0)
                        Console.WriteLine("----------------------------------");
                }
            }
            Console.WriteLine();
        }

        public int algoNaifOptimise()
        {
            // Si la première case contient déjà une couleur,
            // on lance l'algorithme sur cette couleur
            if (m_sommets.First().getCouleur() != 0)
                return attribuerCouleurGraphe(0, 0, m_sommets.First().getCouleur());
            else
            {
                // On lance l'algorithme en testant l'ensemble des couleurs de 1 à 9
                // sur la première case
                for (int colour=1; colour<=9; colour++)
                {
                    int nbOk = attribuerCouleurGraphe(0, 0, colour);
                    if (nbOk == 81)
                        return 81;
                }
            }
            return 0;
        }

        public int attribuerCouleurGraphe(int nbOk, int nbCouleurs, int couleur)
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
                // Console.WriteLine("nbOk = " + nbOk + " - nbCouleurs = " + nbCouleurs + " - couleur = " + s.getCouleur());
                // displayGrid();
                // Console.WriteLine();
                if (nbOk == 81)
                    return nbOk;
                // Si le nombre de couleurs est à 9, on a une ligne complète
                // On réinitialise le nombre de couleurs à 0
                nbCouleurs = nbCouleurs % 9;

                // Si la case suivante contient déjà une couleur,
                // on lance l'algorithme sur cette couleur
                if (m_sommets.ElementAt(nbOk).getCouleur() != 0)
                {
                    if (attribuerCouleurGraphe(nbOk, nbCouleurs, m_sommets.ElementAt(nbOk).getCouleur()) == 81)
                        return 81;
                }
                else
                {
                    // On lance l'algorithme en testant l'ensemble des couleurs de 1 à 9
                    // sur la case suivante
                    for (int colour=1; colour<=9; colour++)
                    {
                        if (attribuerCouleurGraphe(nbOk, nbCouleurs, colour) == 81)
                            return 81;
                    }
                }

                // Si on arrive ici, c'est que la résolution a échoué
                // On restaure la couleur initiale de la case (ou l'absence de couleur)
                s.setCouleur(couleurBackup);
            }
            return nbOk;
        }
    }
}