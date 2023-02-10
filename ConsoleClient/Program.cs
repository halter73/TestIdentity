// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

var handler = new HttpClientHandler
{
    UseCookies = true,
};
var client = new HttpClient(handler);

var loginResponse = await client.PostAsJsonAsync("https://localhost:7148/login", new
{
    Email = args[0],
    Password = args[1],
});
loginResponse.EnsureSuccessStatusCode();

Console.WriteLine(await client.GetStringAsync("https://localhost:7148/weatherforecast"));
