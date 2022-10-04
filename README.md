# Image translation with .NET Core 5.0 and Azure Translation Service

## Requirements

### Install .NET Core 5.0

Install .NET Core SDK from https://dotnet.microsoft.com/en-us/download

### Install packages

> dotnet add package Newtonsoft.Json

> dotnet add package SixLabors.Fonts --version 1.0.0-beta18

> dotnet add package SixLabors.ImageSharp --version 2.1.3

> dotnet add package SixLabors.ImageSharp.Drawing --version 1.0.0-beta15

### Create Azure resources

Create a Azure Storage Account:
https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal 

Create two containers for input and output.

Create a computer vision resource for OCR:https://portal.azure.com/#create/Microsoft.CognitiveServicesComputerVision 

Create a text translator resource: https://portal.azure.com/#create/Microsoft.CognitiveServicesTextTranslation


## Testing

Run this project and navigate to https://localhost:7150/swagger


