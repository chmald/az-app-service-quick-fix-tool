# Azure App Services Quick Fix Tool #

This tool is for the purpose of troubleshooting your Azure App Service Web App.
The tool will do the following in this order:

1. Stop your site
2. Start your site
3. Scale up your App Service Plan to S3 tier
4. Scale down your App Service Plan back to it's original tier

## Running this tool ##

In the `appsettings.json` file set the following:

1. Set the `RESOURCE_GROUP` and `APP_SERVICE` with the names of your App Service and the Resource Group the App Service is in.
2. Set the `AZURE_AUTH_LOCATION` with the full path for your auth file. [How to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md)

```
git clone https://github.com/chmald/az-app-service-quick-fix-tool.git

cd az-app-service-quick-fix-tool

dotnet build

bin\Debug\netcoreapp3.1\AzAppServiceQuickFix.exe
```