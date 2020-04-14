using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.AppService.Fluent;

namespace AzAppServiceQuickFix
{
    class Program
    {
        static void Main(string[] args)
        {
            if(!File.Exists(Directory.GetParent(AppContext.BaseDirectory).FullName + "\\appsettings.json"))
            {
                Log("Please ensure you have appsettings.json in your application directory.");
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json");

            IConfiguration config = builder.Build();

            var authFile = config["AZURE_AUTH_LOCATION"];
            var resourceGroup = config["RESOURCE_GROUP"];
            var appService = config["APP_SERVICE"];

            if(new List<string> { authFile, resourceGroup, appService }.Any(i => String.IsNullOrEmpty(i)))
            {
                Log("Please provide AZURE_AUTH_LOCATION, RESOURCE_GROUP, and APP_SERVICE inside your appsettings.json file.");
                return;
            } else
            {
                try
                {
                    AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromFile(authFile);
                    var azure = Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithDefaultSubscription();

                    RunQuickFix(azure, resourceGroup, appService);

                } catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
        }

        public static void RunQuickFix(IAzure azure, string rg, string app)
        {
            IWebApp app1 = azure.WebApps
                .GetByResourceGroup(rg, app);

            IAppServicePlan plan = azure.AppServices.AppServicePlans.GetById(app1.AppServicePlanId);

            WriteSection("APP SERVICE INFO");
            Log("Web App: " + app1.Name);
            Log("App Service Plan: " + plan.Name);
            Log("App Service Plan Tier: " + plan.PricingTier.ToString());

            StopStart(app1);
            ScaleUpDown(plan);

            WriteSection("QUICK FIX COMPLETED! CHECK SITE\n"+
                "https://" + app1.Name + ".azurewebsites.net/");
        }

        public static void ScaleUpDown(IAppServicePlan appServicePlan)
        {
            WriteSection("Applying a possible scale up/down fix");
            PricingTier _default = appServicePlan.PricingTier;
            Log("Scaling Up App Service Plan");
            appServicePlan.Update().WithPricingTier(PricingTier.StandardS3).Apply();
            LetsWaitALittle();
            Log("Scaling Down App Service Plan");
            appServicePlan.Update().WithPricingTier(_default).Apply();
            LetsWaitALittle();
        }

        public static void StopStart(IWebApp app)
        {
            WriteSection("Applying a possible stop/start fix");
            Log("Stopping Web App");
            app.Stop();
            LetsWaitALittle();
            Log("Starting Web App");
            app.Start();
            LetsWaitALittle();
        }

        public static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void WriteSection(string msg)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(msg);
            Console.WriteLine("========================================");
        }

        public static void LetsWaitALittle()
        {
            Task.Delay(5000).Wait();
        }
    }
}
