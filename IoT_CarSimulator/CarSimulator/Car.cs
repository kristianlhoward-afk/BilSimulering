using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Net.Http.Json;  // innehåller PostAsJsonAsync, som gör om objektet till JSON och skickar det i ett steg.
using System.Text.Json.Serialization;

namespace CarSimulator
{
    internal class Car   // håller tillståndet i sina fält, uppdaterar det och paketerar värdena i TelemetryData (Bilens motorstyrenhet ECU) 
    {
        // These constants can be adjusted to change the simulation's behavior.
        private const double MaxFuelConsumptionPerSecond = 0.005; // liters per second
        private const int IdleRpm = 800;
        private const int MaxRpm = 6000;
        private const int MinSpeed = 0;
        private const int MaxSpeed = 120; // km/h
        private const int UpdateIntervalSeconds = 15;
        private const int TripDurationMinutes = 10;

        // Instance variables to track the car's state
        private double _currentSpeed = 0;
        private double _currentRpm = IdleRpm;
        private double _currentFuelLevel = 100.0; // Starts at 100%
        private readonly Random _random = new Random();  //slumptalsgenerator. Så att simuleringen
                                                         //inte blir exakt likadan varje gång.
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey;         

        public Car(string apiKey)                
        {
            _apiKey = apiKey;
        }



        // Represents a single telemetry data point
        private class TelemetryData   //klass som håller ett mättillfälle.
        {                               //paketet som ska skickas till ThingSpeak var 15:e sekund
                                        //där du man se grafer över hastighet, varvtal, bränsle och temperatur
                                        //i syfte att kunna övervaka i realtid, upptäcka fel tidigt,
                                        //se trender över tid och fatta beslut


            [JsonPropertyName("api_key")]
            public required string ApiKey { get; init; }


            [JsonPropertyName("created_at")]
            public required DateTime CreatedAt { get; init; }


            [JsonPropertyName("field1")]
            public double Speed { get; init; }


            [JsonPropertyName("field2")]
            public double RPM { get; init; }


            [JsonPropertyName("field3")]
            public double FuelLevel { get; init; }


            [JsonPropertyName("field4")]
            public double EngineTemp { get; init; }

        }

        // Main simulation loop
        public async void StartSimulation() 
        {
            Console.WriteLine("Starting car telemetry simulation...");
            var startTime = DateTime.Now;

            while (true)
            {
                var elapsedTime = DateTime.Now - startTime;
                if (elapsedTime.TotalMinutes >= TripDurationMinutes)  //om 10 minuter har gått (eller det värdet som satts på
                                                                      //"private const int TripDurationMinutes"). I så fall avslutas loopen
                {
                    Console.WriteLine("Simulation finished. Trip duration reached.");
                    break;
                }

                SimulateTrip(); //Anropas för att uppdatera bilens tillstånd 

                var data = new TelemetryData  //skapar ett nytt TelemetryData-objekt och
                                              //kopierar de aktuella värdena från fälten in i det
                {

                    ApiKey = _apiKey,
                    CreatedAt = DateTime.UtcNow, // UTC - standard för tidsstämplar i system som tar emot data från flera källor
                    Speed = Math.Round(_currentSpeed, 2),  //Math.Round(värde, 2) - 2 avrundar till två decimaler:
                    RPM = Math.Round(_currentRpm, 2),
                    FuelLevel = Math.Round(_currentFuelLevel, 2),
                    EngineTemp = Math.Round(GenerateEngineTemperature(), 2)
                };

                Console.WriteLine($"Time: {elapsedTime.TotalSeconds:F0}s | Speed: {_currentSpeed:F0} km/h | RPM: {_currentRpm:F0} | Fuel: {_currentFuelLevel:F1}%");

                _httpClient.PostAsJsonAsync("https://api.thingspeak.com/update.json", data).Wait(); //_httpClient sköter HTTP-kommunikation,
                                                                                                    //PostAsJsonAsync(...) gör om data till JSON och skickar
                                                                                                    //med POST, data – TelemetryData-objekt med nyckel, tid och
                                                                                                    //de fyra värdena.
                                                                                                    // wait -  väntar tills anropet är klart innan koden går vidare
                                                                                                    // till Thread.Sleep

                Thread.Sleep(TimeSpan.FromSeconds(UpdateIntervalSeconds));
            }
        }

        // Simulates the car's movement and state changes
        private void SimulateTrip() //Första varvet utgår den från startvärdena (0 km/h, 800 rpm, 100 %),
                                    //nästa varv från värdena det förra varvet lämnade efter sig.
        {
            // Simple state machine for the trip: Idle -> Accelerate -> Cruise -> Decelerate -> Idle
            string tripPhase;
            if (_currentSpeed < 30) //under 30 acceleration 
            {
                tripPhase = "Accelerating";
            }
            else if (_currentSpeed < 90)  //mellan 30 och 90 åk - ungefär jämn hastighet
            {
                tripPhase = "Cruising";
            }
            else
            {
                tripPhase = "Decelerating";     //över 90 inbromsning
            }

            // Adjust speed and RPM based on the current phase
            switch (tripPhase)  //väljer vad som ska göras utifrån fasen
                                //switch (tripPhase)jämför strängen med varje case

            {
                case "Accelerating":
                    _currentSpeed += _random.Next(2, 5); // Increase speed
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
                case "Cruising":
                    _currentSpeed += _random.NextDouble() * 2 - 1; // Slight random fluctuation
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
                case "Decelerating":
                    _currentSpeed -= _random.Next(2, 5); // Decrease speed
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
            }

            // Clamp speed to a realistic range
            //håller värdena inom gränser
            _currentSpeed = Math.Max(MinSpeed, Math.Min(MaxSpeed, _currentSpeed)); //Math.Min(MaxSpeed) tar det minsta av de två
                                                                                   //Math.Min(MaxSpeed) tar det största, så allt under 0 blir 0


            _currentRpm = Math.Max(IdleRpm, _currentRpm);//håller värdena inom gränser - har
                                                         //ingen effekt i praktiken, eftersom CalculateRpm
                                                         //aldrig returnerar mindre än 800,

            // Calculate fuel consumption based on speed and RPM
            double consumptionRate = (_currentSpeed / MaxSpeed) * (_currentRpm / MaxRpm) * MaxFuelConsumptionPerSecond;
                                                        //Hastighet och varvtal görs om till andelar mellan 0 och 1,
                                                            //multipliceras och skalas med maxförbrukningen
            _currentFuelLevel -= consumptionRate * UpdateIntervalSeconds;
            _currentFuelLevel = Math.Max(0, _currentFuelLevel);  //ser till att tanken inte går under noll
        }

        // A simple function to calculate RPM based on speed for a more realistic feel
        
        private double CalculateRpm(double speed)  //låtsassensor
        {
            if (speed < 1)   //Under 1 km/h returneras tomgångsvarvet (IdleRpm)
            {
                return IdleRpm;
            }
            // A simple linear relationship for simulation purposes
            return IdleRpm + (speed / MaxSpeed) * (MaxRpm - IdleRpm);
        }

        // Generates a realistic engine temperature based on speed
        private double GenerateEngineTemperature() //låtsassensor
        {
            // Engine runs hotter at higher RPM/speed
            double baseTemp = 85;  //Bastemperatur 85 grader
            double speedFactor = _currentSpeed / MaxSpeed;
            double temp = baseTemp + (speedFactor * _random.Next(0, 15));//Lägsta möjliga: 85 + 0 = 85 grader
                                                                        //Högsta möjliga: 85 + (1 × 14) = 99 grader
            return Math.Max(80, Math.Min(110, temp));  //Math.Min(a, b) returnerar det minsta av två tal.
                                                       //Math.Max(a, b) returnerar det största av två tal
                                                       // om någon skulle få för sig att ändra ovanstående till 
                                                       //double temp = baseTemp + (speedFactor * _random.Next(0, 40))
                                                       // så skyddar koden så attdet alltid hamnar mellan 80 och 110
        }

    }
}          //Det finns inga aktuatorer. Men om programmet läste temperaturen från ThingSpeak och skickade “starta fläkten”
           //vid över 95 °C, skulle det vara en aktuator-styrning.
           //Finns ingen egen gateway. En HttpClient skickar direkt till ThingSpeak, och i praktiken
           //fungerar en internetuppkoppling och router som vägen ut.

            //Sammanhang: Sensor mäter → enheten samlar in och paketerar → gatewayen förmedlar → molnet tar emot
            //och lagrar (telemetri) → analys och beslut → eventuellt en signal tillbaka till en aktuator.
