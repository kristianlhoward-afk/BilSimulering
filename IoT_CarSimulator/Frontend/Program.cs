using Frontend.DTO;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
IConfigurationRoot _config = new ConfigurationBuilder()
 .AddUserSecrets<Program>()
 .Build();
string? apiKey = _config["apiKey"];
string? channelID = _config["channelID"];
if (string.IsNullOrWhiteSpace(apiKey)
 || string.IsNullOrWhiteSpace(channelID))
{
    throw new Exception("Läsnyckel eller kanal-ID saknas.");
}




HttpClient apiClient = new()
{
    BaseAddress = new Uri("https://api.thingspeak.com")
};
string queryAddress = $"/channels/{channelID}/feeds.json"
 + $"?api_key={Uri.EscapeDataString(apiKey)}&results=100";
Root? result = await apiClient.GetFromJsonAsync<Root>(queryAddress);


if (result?.Feeds == null || result.Feeds.Count == 0)
{
    Console.WriteLine("Det finns inga mätningar att visa.");
}
else
{
    Console.WriteLine($"Kanal: {result.Channel?.Name}");
    foreach (ReadCarDTO car in result.Feeds)
    {
        Console.WriteLine($"Post: {car.EntryId}");
        Console.WriteLine($"Tid: {car.CreatedAt}");
        Console.WriteLine($"Hastighet: {car.Speed} km/h");
        Console.WriteLine($"Varvtal: {car.RPM} RPM");
        Console.WriteLine($"Bränslenivå: {car.FuelLevel}");
        Console.WriteLine($"Motortemperatur: {car.EngineTemp}");
        Console.WriteLine();
    }
}
Console.ReadLine();




