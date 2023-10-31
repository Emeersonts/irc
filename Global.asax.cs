using Elmah.Contrib.WebApi;
using IDP.Common.Globalization;
using IDP.Common.IoC;
using IDP.DBX;
using IDP.DBX.DatabaseScripts;
using IDP.Monitor.Logs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using BackOffice.Authorizer.Management.I18N;
using Consul;
using DbUp;
using BackOffice.Authorizer.Management.Persistence;
using BackOffice.Authorizer.Management.Scripts;

namespace BackOffice.Authorizer.Management.API
{
    public class WebApiApplication : HttpApplication
    {
        private static readonly string AzureSqlDbConnectionStringKey = "Azure.SqlConnectionString";
        private static readonly string LogixHomologKey = "Authorizer.Logix.Homolog.ADV7.SqlConnectionString";
        private static readonly string LogixProductionKey = "Authorizer.Logix.ADV7.SqlConnectionString";
        private static readonly string MessageBrokerUriKey = "MessageBrokerUri";
        private static readonly string ApplicationNameKey = "ApplicationName";
        private static readonly string LogExchangeKey = "Log.Exchange";
        private static readonly string LogQueueKey = "Log.Queue";
        private static readonly string CultureInfo = "Application.CultureInfo";

        protected void Application_Start()
        {
            var messageTranslator =
                new MessageTranslator(
                    new System.Globalization.CultureInfo(ConfigurationManager.AppSettings[CultureInfo]),
                    new ResourceManagerMessageProcessor(CustomResourceManagerUtils.GetResourceManager()));

            Application.SetMessageTranslator(messageTranslator);

            var metricService = MetricsConfig.ConfigureMetrics();
            var healthService = HealthConfig.ConfigureHealths();

            GlobalConfiguration.Configuration.Filters.Add(new ElmahHandleErrorApiAttribute());

            var contextRegistry = AutofacConfig.RegisterComponents(healthService, metricService);

            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            // Update database
            var logService = CreateLogService();
            SqlServerUpdate.Execute(CreateDataContext(), new DefaultSqlUpdaterLog(logService));
            DefaultSqlServerUpdate.Execute(CreateDataContextHomolog(), new DefaultSqlUpdaterLog(logService));
            DefaultSqlServerUpdate.Execute(CreateDataContextProd(), new DefaultSqlUpdaterLog(logService));
        }

        private static ILogService CreateLogService()
        {
            var configuration = new LogConfiguration(
                ConfigurationManager.AppSettings[MessageBrokerUriKey],
                ConfigurationManager.AppSettings[LogExchangeKey],
                ConfigurationManager.AppSettings[LogQueueKey],
                ConfigurationManager.AppSettings[ApplicationNameKey]);
            return new LogFactory().CreateLog(configuration);
        }

        private static IDataContext CreateDataContext()
        {
            return new DataContextFactory().CreateDataContext(
                ConfigurationManager.AppSettings[AzureSqlDbConnectionStringKey]);
        }

        private static IDataContext CreateDataContextHomolog()
        {
            return new DataContextFactory().CreateDataContext(
                ConfigurationManager.AppSettings[LogixHomologKey]);
        }

        private static IDataContext CreateDataContextProd()
        {
            return new DataContextFactory().CreateDataContext(
                ConfigurationManager.AppSettings[LogixProductionKey]);
        }

        private class DefaultSqlUpdaterLog : ISqlUpdaterLog
        {
            private readonly ILogService logService;

            public DefaultSqlUpdaterLog(ILogService logService)
            {
                this.logService = logService;
            }

            private void PublishLog(string message, string module)
            {
                logService.Log(message, module: module);
            }

            public void OnError(string format, object[] args)
            {
                PublishLog(args == null || args.Length == 0 ? format : string.Format(format, args), "error");
            }

            public void OnInformation(string format, object[] args)
            {
                PublishLog(args == null || args.Length == 0 ? format : string.Format(format, args), "info");
            }

            public void OnWarning(string format, object[] args)
            {
                PublishLog(args == null || args.Length == 0 ? format : string.Format(format, args), "warning");
            }
        }
    }
}
