using Autofac;
using System;

namespace Lykke.AlgoStore.Job.Stopping.Client
{
    public static class AutofacExtension
    {
        public static void RegisterAlgoInstanceStoppingClient(this ContainerBuilder builder, string serviceUrl)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterInstance(new AlgoInstanceStoppingClient(serviceUrl)).As<IAlgoInstanceStoppingClient>().SingleInstance();
        }
    }
}
