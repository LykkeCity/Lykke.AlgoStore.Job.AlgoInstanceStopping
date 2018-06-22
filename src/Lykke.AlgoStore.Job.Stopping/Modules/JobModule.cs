using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Settings;
using Lykke.AlgoStore.Job.Stopping.Settings.JobSettings;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.Security.InstanceAuth;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using System;

namespace Lykke.AlgoStore.Job.Stopping.Modules
{
    public class JobModule : Module
    {
        private readonly AppSettings _settings;
        private readonly IReloadingManager<AlgoStoreStoppingSettings> _settingsManager;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public JobModule(AppSettings settings, IReloadingManager<AlgoStoreStoppingSettings> settingsManager, ILog log)
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

            _services.AddInstanceAuthentication(_settings.AlgoStoreStoppingJob.StoppingServiceCache);

            RegisterExternalServices(builder);
            RegisterRepositories(builder);
            builder.Populate(_services);
        }

        private void RegisterExternalServices(ContainerBuilder builder)
        {
            builder.RegisterType<KubernetesApiClient>()
                   .As<IKubernetesApiClient>()
                   .WithParameter("baseUri", new Uri(_settings.AlgoStoreStoppingJob.Kubernetes.Url))
                   .WithParameter("credentials", new TokenCredentials(_settings.AlgoStoreStoppingJob.Kubernetes.BasicAuthenticationValue))
                   .WithParameter("certificateHash", _settings.AlgoStoreStoppingJob.Kubernetes.CertificateHash)
                   .SingleInstance();
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            var reloadingDbManager = _settingsManager.ConnectionString(x => x.Db.DataStorageConnectionString);

            builder.RegisterInstance(AzureTableStorage<AlgoClientInstanceEntity>.Create(reloadingDbManager, AlgoClientInstanceRepository.TableName, _log));
            builder.RegisterType<AlgoClientInstanceRepository>().As<IAlgoClientInstanceRepository>();
        }
    }
}
