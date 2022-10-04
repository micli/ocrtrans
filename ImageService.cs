using SixLabors;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System.Net;

namespace OcrTrans;

public class ImageService
{
    private Image? _image = null;
    private string _connectionString = string.Empty;
    private readonly string _tempFolder = "ImageCache";
    private string _tempName = string.Empty;
    private string _localFilename = string.Empty;

    private Color _coverColor = new Color(new Rgb24(128, 128, 128));
    private static FontFamily _family;
    static ImageService()
    {
        FontCollection collection = new FontCollection();
        _family = collection.Add("../OcrTrans/msyhbd.ttf");
    }
    public ImageService(string storageConnectionString)
    {
        if(!string.IsNullOrWhiteSpace(storageConnectionString))
        {
            _connectionString = storageConnectionString;
        }
        
        if(!System.IO.Directory.Exists("ImageCache"))
        {
            System.IO.Directory.CreateDirectory("ImageCache");
        }
        _tempName = System.Guid.NewGuid().ToString("N");
        _localFilename = System.IO.Path.Combine(_tempFolder, _tempName);
    }

    public void LoadImageFromUrl(string url)
    {
        if(string.IsNullOrWhiteSpace(url))
            return;

        _localFilename = _localFilename + System.IO.Path.GetExtension(url);
        if (System.IO.File.Exists(_localFilename))
            System.IO.File.Delete(_localFilename);
        try
        {
            WebClient client = new WebClient();
            client.DownloadFile(new Uri(url), _localFilename);
            client.Dispose();
        }
        catch(IOException ex)
        {
            // Do nothing.
        }
        if(File.Exists(_localFilename))
        {
            _image = Image<Rgba32>.Load(_localFilename);
        }
    }

    public async Task<string> SaveToStorage(string containerName)
    {
        BlobServiceClient serviceClient = new BlobServiceClient(_connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        containerClient.CreateIfNotExists();
        containerClient.SetAccessPolicy(PublicAccessType.Blob);
        if(File.Exists(_localFilename))
        {
            FileStream localFile = new FileStream(_localFilename, FileMode.Open);
            await containerClient.UploadBlobAsync(System.IO.Path.GetFileName(_localFilename), localFile);
            localFile.Close();
            var blobUrl = Path.Combine(containerClient.Uri.ToString(), Path.GetFileName(_localFilename));
            if(File.Exists(_localFilename))
                File.Delete(_localFilename);
            return blobUrl;
        }
        return string.Empty;
    }

    public void Close()
    {
        if(_image != null)
        {
            _image.Dispose();
            _image = null;
        }
    }
    
    public void DrawText(ReadResult ocrResults, TranslationResult translationResult)
    {
        if(null == _image)
            return;

        if(null == translationResult.Translations || null == ocrResults)
            return;
        if(null == translationResult.Translations[0].Text)
            return;
        var translatedLines = translationResult.Translations[0].Text.Split("\n");
        if(null == translatedLines)
            return;
        Image cover = new Image<Rgb24>(_image.Width, _image.Height);
        cover.Mutate( c => {
            c.Fill(_coverColor);
        });
        _image.Mutate( x =>
        {
            x.DrawImage(cover, 0.7f);
            cover.Dispose();
            cover = null;
            int i = 0;
            foreach(var line in ocrResults.Lines)
            {
                if(i <= translatedLines.Length)
                {
                    var font = GetFont((float)line.BoundingBox[3] - (float)line.BoundingBox[1]);
                    x.DrawText(translatedLines[i], font, Color.WhiteSmoke, 
                        new PointF((float)line.BoundingBox[0], (float)line.BoundingBox[1]));
                    
                    i += 1;
                }
                else
                {
                    break;
                }
            }
        });
        
        _image.Save(_localFilename);
    }
    private Font GetFont(float boxHeight)
    {
        int fontSize = 14; // default
        if(boxHeight >= 20.0f)
        {
            fontSize = 36;
        }
        else if(boxHeight < 20.0f && boxHeight >= 16.0f)
        {
            fontSize = 30;
        }
        else if(boxHeight < 16.0f && boxHeight >= 12.0f)
        {
            fontSize = 18;
        }
        else
        {
            fontSize = 14;
        }
        // FontCollection collection = new FontCollection();
        // FontFamily family = collection.Add("../imagesharptest/msyhbd.ttf");
        Font font = _family.CreateFont(fontSize, FontStyle.Bold);
        return font;
    }
}