using Frontend;
using Microsoft.Extensions.Configuration;




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


Menu menu = new Menu(apiKey, channelID);
await menu.Run();








