
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using OcrTrans.Models;

namespace OcrTrans.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageTranslateController : ControllerBase
{
    private readonly ILogger<ImageTranslateController> _logger;

    public ImageTranslateController(ILogger<ImageTranslateController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetStatus")]
    public string Get()
    {
        var config = Startup.Configuration;
        if(null == config)
        return string.Empty;

        StringBuilder sb = new StringBuilder();
        sb.Append("Computer Vision: ");
        sb.AppendLine(config["Computer_Vision_Endpoint"]);
        sb.Append("Translator Endpoint: ");
        sb.AppendLine(config["Translator_Endpoint"]);
        return sb.ToString();
    }

    [HttpPost(Name = "Translate image from URL.")]
    public async Task<string> Post([FromHeader]string originalUrl)
    {
        var config = Startup.Configuration;
        if(null == config)
        return string.Empty;


        ResponseMessage msg = new ResponseMessage("Succeeded", originalUrl, string.Empty);
        if(string.IsNullOrWhiteSpace(originalUrl))
        {
            msg.Status = "Failed";
            msg.ErrorMessage = "Image url not specified.";
            return JsonConvert.SerializeObject(msg);
        }
        OcrService ocr = new OcrService(config["Computer_Vision_Endpoint"], config["Computer_Vision_Key"]);
        IList<Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.ReadResult>? ocrResultList = null;
        try
        {
            ocr.Authenticate();
            ocrResultList = await ocr.ReadFileUrl(originalUrl);
        }
        catch(Exception ex)
        {
            msg.Status = "Failed";
            msg.ErrorMessage = "Reading image failed.";
            return JsonConvert.SerializeObject(msg);
        }

        var trans = new Translator(config["Translator_EndPoint"], config["Translator_Key"], config["Translator_Region"]);
        foreach(var ocrResult in ocrResultList)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var line in ocrResult.Lines)
            {
                sb.AppendLine(line.Text);
            }
            var tranResult = await trans.Translate(sb.ToString());
            if (0 != tranResult.Length)
            {
                ImageService imgsrv = new ImageService(config["Storage_Connection_String"]);
                imgsrv.LoadImageFromUrl(originalUrl);
                imgsrv.DrawText(ocrResult, tranResult[0]);
                var translatedUrl = await imgsrv.SaveToStorage("output");
                msg.TranslatedUrl = translatedUrl;
                return JsonConvert.SerializeObject(msg);
            }

        }
        msg.Status = "Failed";
        msg.ErrorMessage = "There is no text need to be translated.";
        return JsonConvert.SerializeObject(msg);
    }
}