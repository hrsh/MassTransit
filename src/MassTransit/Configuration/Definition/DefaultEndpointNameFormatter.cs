namespace MassTransit.Definition
{
    using System;
    using System.Text;
    using Courier;
    using Metadata;
    using Saga;
    using Util;


    /// <summary>
    /// The default endpoint name formatter, which simply trims the words Consumer, Activity, and Saga
    /// from the type name. If you need something more readable, consider the <see cref="SnakeCaseEndpointNameFormatter" />
    /// or the <see cref="KebabCaseEndpointNameFormatter" />.
    /// </summary>
    public class DefaultEndpointNameFormatter :
        IEndpointNameFormatter
    {
        readonly bool _includeNamespace;

        protected DefaultEndpointNameFormatter()
        {
            _includeNamespace = false;
        }

        public DefaultEndpointNameFormatter(bool includeNamespace = false)
        {
            _includeNamespace = includeNamespace;
        }

        public static IEndpointNameFormatter Instance { get; } = new DefaultEndpointNameFormatter();

        public string TemporaryEndpoint(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                tag = "endpoint";

            var host = HostMetadataCache.Host;

            var sb = new StringBuilder(host.MachineName.Length + host.ProcessName.Length + tag.Length + 35);

            foreach (var c in host.MachineName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            sb.Append('_');
            foreach (var c in host.ProcessName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            sb.Append('_');
            sb.Append(tag);
            sb.Append('_');
            sb.Append(NewId.Next().ToString(FormatUtil.Formatter));

            return sb.ToString();
        }

        public string Consumer<T>()
            where T : class, IConsumer
        {
            return GetConsumerName<T>();
        }

        public string Message<T>()
            where T : class
        {
            return GetMessageName(typeof(T));
        }

        public string Saga<T>()
            where T : class, ISaga
        {
            return GetSagaName<T>();
        }

        public string ExecuteActivity<T, TArguments>()
            where T : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            var activityName = GetActivityName<T>();

            return $"{activityName}_execute";
        }

        public string CompensateActivity<T, TLog>()
            where T : class, ICompensateActivity<TLog>
            where TLog : class
        {
            var activityName = GetActivityName<T>();

            return $"{activityName}_compensate";
        }

        public virtual string SanitizeName(string name)
        {
            return name;
        }

        string GetConsumerName<T>()
        {
            if (typeof(T).IsGenericType)
                return SanitizeName(typeof(T).GetGenericArguments()[0].Name);

            const string consumer = "Consumer";

            var consumerName = _includeNamespace
                ? TypeMetadataCache<T>.ShortName.Replace(".", "_").Replace("+", "_")
                : typeof(T).Name;

            if (consumerName.EndsWith(consumer, StringComparison.InvariantCultureIgnoreCase))
                consumerName = consumerName.Substring(0, consumerName.Length - consumer.Length);

            return SanitizeName(consumerName);
        }

        string GetMessageName(Type type)
        {
            if (type.IsGenericType)
                return SanitizeName(type.GetGenericArguments()[0].Name);

            var messageName = type.Name;

            return SanitizeName(messageName);
        }

        string GetSagaName<T>()
        {
            const string saga = "Saga";

            var sagaName = _includeNamespace
                ? TypeMetadataCache<T>.ShortName.Replace(".", "_").Replace("+", "_")
                : typeof(T).Name;

            if (sagaName.EndsWith(saga, StringComparison.InvariantCultureIgnoreCase))
                sagaName = sagaName.Substring(0, sagaName.Length - saga.Length);

            return SanitizeName(sagaName);
        }

        string GetActivityName<T>()
        {
            const string activity = "Activity";

            var activityName = _includeNamespace
                ? TypeMetadataCache<T>.ShortName.Replace(".", "_").Replace("+", "_")
                : typeof(T).Name;

            if (activityName.EndsWith(activity, StringComparison.InvariantCultureIgnoreCase))
                activityName = activityName.Substring(0, activityName.Length - activity.Length);

            return SanitizeName(activityName);
        }
    }
}
