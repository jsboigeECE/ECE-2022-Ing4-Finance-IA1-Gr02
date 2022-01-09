# ECE-2022-Ing4-Finance-IA1-Gr02
Bienvenue sur le dépôt du TP Sudoku.

Chaque groupe est invité à créer un Fork de ce dépôt principal muni d'un compte sur Github, travailler sur ce fork au sein du groupe, par le biais de validations, de push sur le server, et de pulls/tirages sur les machines locales des utilisateurs du groupe habilités sur le fork. Une fois le travail effectué et remonté sur le fork, une pull-request sera créée depuis le fork vers le dépôt principal pour fusion et évaluation.

Le fichier de solution "Sudoku.sln" constitue l'environnement de base du travail et s'ouvre dans Visual Studio Community (attention à bien ouvrir la solution et ne pas rester en "vue de dossier").
En l'état, la solution contient:
- Un répertoire Puzzles contenant 3 fichiers de Sudokus de difficultés différentes
- Un projet Sudoku.Shared: il consitue la librairie de base de l'application et fournit la définition de la classe représentant un Sudoku (GridSudoku) et l'interface à implémenter par les solvers de sudoku (ISolverSudoku).
- Un projet Sudoku.Z3Solvers qui fournit plusieurs exemples de solvers utilisant la librairie z3 grâce au package Nuget correspondant, et qui illustre également l'utilisation de Python depuis le langage c# via  Python.Net grâce aux packages Nuget correspondants.
- Un projet Sudoku.Benchmark de Console permettant de tester les solvers de Sudoku de façon individuels ou dans le cadre d'un Benchmark comparatif. Ce projet référence les projets de solvers, et c'est celui que vous devez lancer pour tester votre code.

Les groupes de travail sont invités à àjouter à la solution leur propre projet de librairie sur le modèle du projet z3 en exemple.