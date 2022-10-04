
using Newtonsoft.Json;
using System.Text;
namespace OcrTrans;


public class TranslationResult
{
    public DetectedLanguage? DetectedLanguage { get; set; }
    public TextResult? SourceText { get; set; }
    public Translation[]? Translations { get; set; }
}

public class DetectedLanguage
{
    public string? Language { get; set; }
    public float Score { get; set; }
}

public class TextResult
{
    public string? Text { get; set; }
    public string? Script { get; set; }
}

public class Translation
{
    public string? Text { get; set; }
    public TextResult? Transliteration { get; set; }
    public string? To { get; set; }
    public Alignment? Alignment { get; set; }
    public SentenceLength? SentLen { get; set; }
}

    public class Alignment
    {
        public string? Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[]? SrcSentLen { get; set; }
        public int[]? TransSentLen { get; set; }
    }

public class Translator
{
    private string _endpoint = string.Empty;
    private string _subscriptionKey = string.Empty;

    private string _region = "eastasia";
    private readonly string _route = "translate?api-version=3.0&to=zh-Hans";

    public Translator(string endpoint, string key, string region)
    {
        if(!string.IsNullOrWhiteSpace(endpoint))
        {
            _endpoint = endpoint;
        }
        if(!string.IsNullOrWhiteSpace(key))
        {
            _subscriptionKey = key;
        }
        if(!string.IsNullOrWhiteSpace(region))
        {
            _region = region;
        }
    }

    public async Task<TranslationResult[]?> Translate(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
            return null;

        object[] body = new object[] { new { Text = text } };
        var requestBody = JsonConvert.SerializeObject(body);

        using(var client = new HttpClient())
        using(var request = new HttpRequestMessage())
        {
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_endpoint + _route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
            TranslationResult[]? deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
            return deserializedOutput;
        }
    }
}