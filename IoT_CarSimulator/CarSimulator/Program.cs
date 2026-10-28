using Microsoft.Extensions.Configuration;

namespace CarSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                 .AddUserSecrets<Program>()
                    .Build();

            string? apiKey = config["apiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Du har inte lagt in din API-nyckel.");
            }


            Car car = new Car(apiKey); //skapar Car-objektet och skickar med nyckeln till konstruktorn
            car.StartSimulation();
            
            Console.WriteLine("Hello, World!");
        }
    }
}
