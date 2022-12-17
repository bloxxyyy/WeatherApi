namespace HttpRequestExample;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

public record Coord(
	[property: JsonProperty("lon")] double lon,
	[property: JsonProperty("lat")] double lat
);

public record Main(
	[property: JsonProperty("temp")] double temp,
	[property: JsonProperty("feels_like")] double feels_like,
	[property: JsonProperty("temp_min")] double temp_min,
	[property: JsonProperty("temp_max")] double temp_max,
	[property: JsonProperty("humidity")] int humidity
);

public record Root(
	[property: JsonProperty("coord")] Coord coord,
	[property: JsonProperty("weather")] IReadOnlyList<Weather> weather,
	[property: JsonProperty("base")] string @base,
	[property: JsonProperty("main")] Main main,
	[property: JsonProperty("wind")] Wind wind,
	[property: JsonProperty("timezone")] int timezone
);

public record Weather(
	[property: JsonProperty("main")] string main,
	[property: JsonProperty("description")] string description
);

public record Wind(
	[property: JsonProperty("speed")] double speed,
	[property: JsonProperty("deg")] int deg,
	[property: JsonProperty("gust")] double gust
);

public class RequestSender<T> {
	private readonly HttpClient _client;
	private readonly HttpRequestMessage _request;
	private readonly string _url;

	public RequestSender(string url) {
		_url = url;
		ServicePointManager.DefaultConnectionLimit = 50;

		_client = new HttpClient();

		_request = new HttpRequestMessage(HttpMethod.Get, _url);
		_request.Headers.CacheControl = new CacheControlHeaderValue() {
			MaxAge = TimeSpan.FromDays(30)
		};
	}

	public async Task<T> GetAsync() {
		HttpResponseMessage response = await _client.SendAsync(_request);
		string responseContent = await response.Content.ReadAsStringAsync();
		var responseData = JsonConvert.DeserializeObject<T>(responseContent);
		if (responseData == null) throw new ArgumentNullException();
		return responseData;
	}
}

class Program {
	static async Task Main(string[] args) {
		RequestSender<Root> weatherRequest = new("https://api.openweathermap.org/data/2.5/weather?lat=51.9851&lon=5.8987&units=metric&appid={yourApiKeyHere}");
		Root testResponseData = await weatherRequest.GetAsync();
		Console.WriteLine("temp: " + testResponseData.main.temp);
		Console.WriteLine("max temp: " + testResponseData.main.temp_max);
		Console.WriteLine("min temp: " + testResponseData.main.temp_min);
		Console.WriteLine("feels like: " + testResponseData.main.feels_like);
		Console.WriteLine("humidity: " + testResponseData.main.humidity);
	}
}