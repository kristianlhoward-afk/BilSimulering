using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontend
{
    internal class Menu
    {

        public void ShowMenu()
        {
            Console.WriteLine("Välkommen till Bilsimulatorns Analysmeny!\n");
            Console.WriteLine("******* Välj ett av följande alternativ *******\n");
            Console.WriteLine("1. Period 24h");
            Console.WriteLine("2. Senaste 100 datapunkterna");
            Console.WriteLine("0. Avsluta");
        }



        public void Run()
        {
            bool keepRunning = true;


            //Skapar CheckLibrary-objektet en gång här (innan loopen)
            var library = new CheckLibrary();

            //Skapar ett nytt objekt av typen AddBooks 
            var addBooks = new AddBooks(library.Books); //Injekterar" listan med böcker från CheckLibrary in i AddBooks så
                                                        //att AddBooks kan lägga till nya böcker).
                                                        //skapar objekt för att fil-skriva/läsa
            var fileHandler = new BookFileHandler();
            var searchBooks = new SearchBooks(library.Books);




            while (keepRunning)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("1. Period 24h");






                        Console.Write("ID: ");
                        string id = Console.ReadLine();

                        Console.Write("Titel: ");
                        string titel = Console.ReadLine();

                        Console.Write("Författare: ");
                        string author = Console.ReadLine();







                        Console.Write("Antal kopior: ");
                        int copies = int.Parse(Console.ReadLine());


                        // Lägg till boken i listan
                        addBooks.AddBook(id, titel, author, copies);

                        // Spara listan till fil 
                        fileHandler.SaveBooks(library.Books);

                        Console.WriteLine("Boken har lagts till och sparats!");
                        Console.WriteLine("\n\nTryck på valfri knapp för att återvända till huvudmeny...");

                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case "2":
                       
                        Console.WriteLine("\n2. Senaste 100 datapunkterna");







                        Console.Write("\n\nAnge ID på boken du vill ta bort: ");
                        string idToRemove = Console.ReadLine();

                        BookRemover remover = new BookRemover(library.Books);

                        bool success = remover.RemoveBook(idToRemove);

                        if (success)
                        {
                            Console.WriteLine("Boken med ID: " + idToRemove + " har tagits bort nu från systemet.");

                            fileHandler.SaveBooks(library.Books);
                        }
                        else
                        {
                            Console.WriteLine("Boken hittades inte");
                        }
                        Console.ReadKey();


                        break;

                 

                  
                       
                          

                    case "0":
                        Console.WriteLine("Avslutas!");
                        keepRunning = false;
                        break;

                    default:
                        Console.WriteLine("Fel inmatning, Skriv en siffra mellan 0-2");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}






