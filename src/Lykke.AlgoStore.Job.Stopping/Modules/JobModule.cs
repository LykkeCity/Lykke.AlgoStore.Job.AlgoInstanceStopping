using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.AlgoStore.Job.Stopping.Settings.JobSettings;
using Lykke.SettingsReader;
using Microsoft.Rest;
using System;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using AzureStorage.Tables;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;

namespace Lykke.AlgoStore.Job.Stopping.Modules
{
    public class JobModule : Module
    {
        private readonly AlgoStoreStoppingSettings _settings;
        private readonly IReloadingManager<AlgoStoreStoppingSettings> _settingsManager;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public JobModule(AlgoStoreStoppingSettings settings, IReloadingManager<AlgoStoreStoppingSettings> settingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _settingsManager = settingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                   .As<ILog>()
                   .SingleInstance();



            RegisterExternalServices(builder);
            RegisterRepositories(builder);
            builder.Populate(_services);
        }

        private void RegisterExternalServices(ContainerBuilder builder)
        {
            builder.RegisterType<KubernetesApiClient>()
                   .As<IKubernetesApiClient>()
                   .WithParameter("baseUri", new Uri(_settings.Kubernetes.Url))
                   .WithParameter("credentials", new TokenCredentials(_settings.Kubernetes.BasicAuthenticationValue))
                   .WithParameter("certificateHash", _settings.Kubernetes.CertificateHash)
                   .SingleInstance();

        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            var reloadingDbManager = _settingsManager.ConnectionString(x => x.Db.DataStorageConnectionString);

            builder.RegisterInstance(AzureTableStorage<UserLogEntity>.Create(reloadingDbManager, UserLogRepository.TableName, _log));

            builder.RegisterType<UserLogRepository>().As<IUserLogRepository>();
        }
    }
}
