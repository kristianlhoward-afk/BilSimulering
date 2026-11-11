using Frontend.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Frontend
{
    internal class Menu
    {








        private readonly string _apiKey;
        private readonly string _channelID;

        public Menu(string apiKey, string channelID)
        {
            _apiKey = apiKey;
            _channelID = channelID;
        }





        public void ShowMenu()
        {
            Console.WriteLine("Välkommen till Bilsimulatorns Analysmeny!\n");
            Console.WriteLine("******* Välj ett av följande alternativ *******\n");
            Console.WriteLine("1. Period 24h");
            Console.WriteLine("2. Senaste 100 datapunkterna");
            Console.WriteLine("0. Avsluta");
        }






        /*

        //Skapar CheckLibrary-objektet en gång här (innan loopen)
        var library = new CheckLibrary();

        //Skapar ett nytt objekt av typen AddBooks 
        var addBooks = new AddBooks(library.Books); //Injekterar" listan med böcker från CheckLibrary in i AddBooks så
                                                    //att AddBooks kan lägga till nya böcker).
                                                    //skapar objekt för att fil-skriva/läsa
        var fileHandler = new BookFileHandler();
        var searchBooks = new SearchBooks(library.Books);

        */

        public async Task Run()

        {


            bool keepRunning = true;

            while (keepRunning)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":


                        Console.WriteLine("1. Period 24h – kommer snart");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    /*


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
                    */
                    case "2":

                        Console.WriteLine("2. Senaste 100 datapunkterna\n");
                        await FetchAndShow("results=100");
                        Console.WriteLine("\nTryck på valfri knapp för att återvända till huvudmenyn...");
                        Console.ReadKey();
                        Console.Clear();
                        break;















                    /*

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



              */



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



        private async Task FetchAndShow(string period)
        {
            HttpClient apiClient = new() { BaseAddress = new Uri("https://api.thingspeak.com") };
            string queryAddress = $"/channels/{_channelID}/feeds.json"
                + $"?api_key={Uri.EscapeDataString(_apiKey)}&{period}";
            Root? result = await apiClient.GetFromJsonAsync<Root>(queryAddress);

            if (result?.Feeds == null || result.Feeds.Count == 0)
            {
                Console.WriteLine("Det finns inga mätningar att visa.");
                return;
            }

            Console.WriteLine($"Kanal: {result.Channel?.Name}");
            foreach (ReadCarDTO car in result.Feeds)
            {
                Console.WriteLine($"Post: {car.EntryId}");
                Console.WriteLine($"Tid: {car.CreatedAt}");
                Console.WriteLine($"Hastighet: {car.Speed} km/h");
                Console.WriteLine($"Varvtal: {car.RPM} RPM");
                Console.WriteLine($"Bränslenivå: {car.FuelLevel}");
                Console.WriteLine($"Motortemperatur: {car.EngineTemp}");

                Console.WriteLine($"Medelvärde varvtal: {car.RPM}");

                Console.WriteLine();

            }




            //Uträkning medelhastiget RPM och hastighet




            //summor och räknare skapas med startvärdet 0
            double speedSum = 0;
            double rpmSum = 0;
            int speedCount = 0;
            int rpmCount = 0;

            foreach (ReadCarDTO car in result.Feeds) //går igenom varje hämtad post i result.Feeds.
            {
                if (double.TryParse(car.Speed, NumberStyles.Any, CultureInfo.InvariantCulture, out double speed)) //varje värde måste omvandlas från text till tal 
                                                //ThingSpeak skickar tal med punkt som decimaltecken ->
                           //TryParse läser talet enligt datorns språkinställningar (här svenska: punkt = punkt, ej decimaltecken) - >
                                                                 //CultureInfo.InvariantCulture gör att
                                                                 //punkt läses som decimaltecken
                {
                    speedSum += speed; // om ovandlingen lyckas läggs det till summan (+=)
                    speedCount++;      //...och ökar räknaren med 1 (++)...
                    //... om det misslyckas hoppas posten över
                }

                if (double.TryParse(car.RPM, NumberStyles.Any, CultureInfo.InvariantCulture, out double rpm))
                {
                    rpmSum += rpm;
                    rpmCount++;
                }
            }

            Console.WriteLine($"Antal poster: {result.Feeds.Count}");

            if (speedCount > 0) //förhindrar division med noll.
                Console.WriteLine($"Medelhastighet: {speedSum / speedCount:F1} km/h"); //summan delas med antalet. :F1 = en decimal visas

            if (rpmCount > 0)
                Console.WriteLine($"Medelvarvtal: {rpmSum / rpmCount:F0} RPM"); //:F0 = noll decimaler visas





        }
    }
}













