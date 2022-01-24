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
                Reseau reseau = new Reseau(grid);
                Console.WriteLine("Affichage de la grille a completer");
                reseau.displayGrid();

                // Coloration algorithme naif et affichage
                int nbFreq;
                reseau.reinitGrid();
                reseau.attribuerNaif(out nbFreq);
                Console.WriteLine("Algo naif : " + nbFreq + " couleurs utilisees");
//                reseau.display();
                reseau.displayGrid();
//                reseau.updateGrid(ref grid);

                // Coloration algorithme naif3 et affichage
                int nbFreq3;
                reseau.reinitGrid();
                reseau.displayGrid();
                Console.WriteLine("Algo naif 3");
                //reseau.attribuerNaif3(out nbFreq3);
                //Console.WriteLine("Algo naif 3 : " + nbFreq3 + " couleurs utilisees");
//                reseau.display();
                reseau.displayGrid();
//                reseau.updateGrid(ref grid);

            }
            catch (Exception e)
            {
                Console.Write("Attention ", e);
            }
            return grid;
        }
    }


    // La classe Sommet représente un sommet du graphe
    // Les adjacences sont déduites des positions ligne,colonne des Sommets
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
                    if ( s.m_ligne == m_ligne || s.m_colonne == m_colonne || s.m_id == m_id)
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
                if ( s.m_couleur == couleur )
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


    // La classe Reseau représente un graphe dans son ensemble
    public class Reseau
    {
        /// Le réseau est constitué d'une collection de sommets
        List<Sommet> m_sommets;
        GridSudoku m_grid;

        public void updateGrid(ref GridSudoku grid)
        {
            foreach (Sommet s in m_sommets)
                grid.Cellules[s.getLigne()][s.getColonne()] = s.getCouleur();
        }

        /// La construction du réseau se fait à partir d'une grille Sudoku
        public Reseau(GridSudoku grid)
        {
            m_grid = grid;
            reinitGrid();
        }

        public void reinitGrid()
        {
            m_sommets = new List<Sommet>();
            int bloc = 0;
            for (int ligne=0; ligne<9; ligne++)
                for (int colonne=0; colonne<9; colonne++)
                    {
                        if (ligne < 3)
                            bloc = (int)(colonne/3);
                        else if (ligne < 6)
                            bloc = 3 + (int)(colonne/3);
                        else if (ligne < 9)
                            bloc = 6 + (int)(colonne/3);
                        m_sommets.Add(new Sommet(bloc, ligne, colonne, m_grid.Cellules[ligne][colonne]));
                    }
            foreach (Sommet s in m_sommets)
            {
                s.determineAdjacents(m_sommets);
            }
        }

        // Retourne l'ordre du graphe (ordre = nombre de sommets)
        public int getOrdre()
        {
            return m_sommets.Count;
        }

        // Méthode d'affichage des objets de type Reseau
        public void display()
        {
            Console.WriteLine("Reseau d'ordre " + getOrdre() + " :");
            foreach (Sommet s in m_sommets)
                s.display();
            Console.WriteLine();
        }

        public void displayGrid()
        {
            Console.WriteLine("----------------------------------");
            int i=0;
            foreach (Sommet s in m_sommets)
            {
                if (i%3 == 0)
                    Console.Write("| ");
                Console.Write("{0,2:#0} ", s.getCouleur());
                i++;
                if (i%9 == 0)
                    Console.WriteLine("|");
                if (i%27 == 0)
                    Console.WriteLine("----------------------------------");
            }
            Console.WriteLine();
        }

        public void attribuerNaif(out int nbcouleurs)
        {
            int couleur;
            nbcouleurs = 0;

            // On parcourt la liste des adjacents
            foreach (Sommet s in m_sommets)
            {
                couleur = 1;
                // "Coloration du graphe". On attribue à chaque sommet une couleur, selon les conditions.
                while (s.getCouleur() == 0)
                {
                    // S'il n'y a pas de conflits entre les sommets voisins, on peut affecter la couleur
                    if (s.testCouleur(couleur))
                        s.setCouleur(couleur);
                    else    // Sinon on augmente la couleur
                        ++couleur;
                }
                // On cherche le nombre de couleurs
                if (s.getCouleur() > nbcouleurs)
                    nbcouleurs = s.getCouleur();
            }
        }

        public void attribuerNaif3(out int nbcouleurs)
        {
            int couleur;
            int couleurDebut = 1;
            nbcouleurs = 0;
            int nbOk = 0;
            int iteration;

            // On parcourt la liste des adjacents
            while (nbOk < 81)
            {
                foreach (Sommet s in m_sommets)
                {
                    couleur = couleurDebut;
                    // "Coloration du graphe". On attribue à chaque sommet une couleur, selon les conditions.
                    iteration = 0;
                    do
                    {
                        if (s.getCouleur() != 0)
                        {
                            // La couleur présente dans la grille source
                            // On met à jour le nombre de couleurs
                            nbcouleurs++;
                            nbOk++;
                        }
                        else if (s.testCouleur(couleur))
                        {
                            // S'il n'y a pas de conflits entre les sommets voisins, on peut affecter la couleur
                            s.setCouleur(couleur);
                            // On met à jour le nombre de couleurs
                            nbcouleurs++;
                            nbOk++;
                        }
                        else    // Sinon on augmente la couleur
                            couleur = 1 + (couleur++)%9;
                        iteration++;
                    } while (s.getCouleur() == 0 && iteration < 9);
                    Console.WriteLine("nbOk = " + nbOk);
                    displayGrid();
                    if (nbOk >= 7)
                    {
                        Console.WriteLine("nbOk = " + nbOk);
                        displayGrid();
                        Console.WriteLine();
                    }
                    if (iteration == 9 && nbOk%9 == 0)
                        iteration = 0;
//                    else if (nbOk%9 != 0)
//                        break;
                }
                if (nbOk < 81)
                {
                    couleurDebut++;
                    if (couleurDebut > 9)
                        couleurDebut = 1;
                    reinitGrid();
                    nbcouleurs = 0;
                    nbOk = 0;
                }
            }    
        }
    }
}