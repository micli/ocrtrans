using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace OcrTrans
{
    class OcrService
    {
        private ComputerVisionClient? _client = null;
        // Add your Computer Vision subscription key and endpoint
        private string _subscriptionKey = "PASTE_YOUR_COMPUTER_VISION_SUBSCRIPTION_KEY_HERE";
        private string _endpoint = "PASTE_YOUR_COMPUTER_VISION_ENDPOINT_HERE";

        public OcrService(string endpoint, string key)
        {
            if(!string.IsNullOrWhiteSpace(endpoint))
            {
                _endpoint = endpoint;
            }
            if(!string.IsNullOrWhiteSpace(key))
            {
                _subscriptionKey = key;
            }
        }

        public void Authenticate()
        {
            _client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(_subscriptionKey)){ Endpoint = _endpoint };
        }

        public async Task<IList<ReadResult>?> ReadFileUrl(string urlFile)
        {
            if(string.IsNullOrWhiteSpace(urlFile))
            {
                return null;
            }
            // Read text from URL
            var textHeaders = await _client.ReadAsync(urlFile);
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            // Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;

            do
            {
                results = await _client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            // Display the found text.
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            return textUrlFileResults;
        }

    }
}