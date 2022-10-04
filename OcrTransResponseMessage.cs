using Newtonsoft.Json;

namespace OcrTrans.Models;

public class ResponseMessage
{
    public string Status{get;set;}
    public string OriginalUrl{get;set;}
    public string TranslatedUrl{get;set;}
    public string ErrorMessage{get;set;}

    public ResponseMessage(string status, string originalUrl, string translatedUrl)
    {
        Status = status;
        OriginalUrl = originalUrl;
        TranslatedUrl = translatedUrl;
    }
}