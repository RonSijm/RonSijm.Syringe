// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe.ServiceLookup;

namespace RonSijm.Syringe;

[EventSource(Name = "Microsoft-Extensions-DependencyInjection")]
internal sealed class DependencyInjectionEventSource : EventSource
{
    public static readonly DependencyInjectionEventSource Log = new();

    public static class Keywords
    {
        public const EventKeywords ServiceProviderInitialized = (EventKeywords)0x1;
    }

    // Event source doesn't support large payloads so we chunk large payloads like formatted call site tree and descriptors
    private const int MaxChunkSize = 10 * 1024;

    private readonly List<WeakReference<MicrosoftServiceProvider>> _providers = new();

    private DependencyInjectionEventSource() : base(EventSourceSettings.EtwSelfDescribingEventFormat)
    {
    }

    // NOTE
    // - The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They
    //   enable creating 'activities'.
    //   For more information, take a look at the following blog post:
    //   https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/
    // - A stop event's event id must be next one after its start event.
    // - Avoid renaming methods or parameters marked with EventAttribute. EventSource uses these to form the event object.

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
        Justification = "Parameters to this method are primitive and are trimmer safe.")]
    [Event(1, Level = EventLevel.Verbose)]
    private void CallSiteBuilt(string serviceType, string callSite, int chunkIndex, int chunkCount, int serviceProviderHashCode)
    {
        WriteEvent(1, serviceType, callSite, chunkIndex, chunkCount, serviceProviderHashCode);
    }

    [Event(2, Level = EventLevel.Verbose)]
    public void ServiceResolved(string serviceType, int serviceProviderHashCode)
    {
        WriteEvent(2, serviceType, serviceProviderHashCode);
    }

    [Event(3, Level = EventLevel.Verbose)]
    public void ExpressionTreeGenerated(string serviceType, int nodeCount, int serviceProviderHashCode)
    {
        WriteEvent(3, serviceType, nodeCount, serviceProviderHashCode);
    }

    [Event(4, Level = EventLevel.Verbose)]
    public void DynamicMethodBuilt(string serviceType, int methodSize, int serviceProviderHashCode)
    {
        WriteEvent(4, serviceType, methodSize, serviceProviderHashCode);
    }

    [Event(5, Level = EventLevel.Verbose)]
    public void ScopeDisposed(int serviceProviderHashCode, int scopedServicesResolved, int disposableServices)
    {
        WriteEvent(5, serviceProviderHashCode, scopedServicesResolved, disposableServices);
    }

    [Event(6, Level = EventLevel.Error)]
    public void ServiceRealizationFailed(string exceptionMessage, int serviceProviderHashCode)
    {
        WriteEvent(6, exceptionMessage, serviceProviderHashCode);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
        Justification = "Parameters to this method are primitive and are trimmer safe.")]
    [Event(7, Level = EventLevel.Informational, Keywords = Keywords.ServiceProviderInitialized)]
    private void ServiceProviderBuilt(int serviceProviderHashCode, int singletonServices, int scopedServices, int transientServices, int closedGenericsServices, int openGenericsServices)
    {
        WriteEvent(7, serviceProviderHashCode, singletonServices, scopedServices, transientServices, closedGenericsServices, openGenericsServices);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
        Justification = "Parameters to this method are primitive and are trimmer safe.")]
    [Event(8, Level = EventLevel.Informational, Keywords = Keywords.ServiceProviderInitialized)]
    private void ServiceProviderDescriptors(int serviceProviderHashCode, string descriptors, int chunkIndex, int chunkCount)
    {
        WriteEvent(8, serviceProviderHashCode, descriptors, chunkIndex, chunkCount);
    }

    [NonEvent]
    public void ServiceResolved(MicrosoftServiceProvider provider, Type serviceType)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.All))
        {
            ServiceResolved(serviceType.ToString(), provider.GetHashCode());
        }
    }

    [NonEvent]
    public void CallSiteBuilt(MicrosoftServiceProvider provider, Type serviceType, ServiceCallSite callSite)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.All))
        {
            var format = CallSiteJsonFormatter.Instance.Format(callSite);
            var chunkCount = format.Length / MaxChunkSize + (format.Length % MaxChunkSize > 0 ? 1 : 0);

            var providerHashCode = provider.GetHashCode();
            for (var i = 0; i < chunkCount; i++)
            {
                CallSiteBuilt(
                    serviceType.ToString(),
                    format.Substring(i * MaxChunkSize, Math.Min(MaxChunkSize, format.Length - i * MaxChunkSize)), i, chunkCount,
                    providerHashCode);
            }
        }
    }

    [NonEvent]
    public void DynamicMethodBuilt(MicrosoftServiceProvider provider, Type serviceType, int methodSize)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.All))
        {
            DynamicMethodBuilt(serviceType.ToString(), methodSize, provider.GetHashCode());
        }
    }

    [NonEvent]
    public void ServiceRealizationFailed(Exception exception, int serviceProviderHashCode)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            ServiceRealizationFailed(exception.ToString(), serviceProviderHashCode);
        }
    }

    [NonEvent]
    public void ServiceProviderBuilt(MicrosoftServiceProvider provider)
    {
        lock (_providers)
        {
            _providers.Add(new WeakReference<MicrosoftServiceProvider>(provider));
        }

        WriteServiceProviderBuilt(provider);
    }

    [NonEvent]
    public void ServiceProviderDisposed(MicrosoftServiceProvider provider)
    {
        lock (_providers)
        {
            for (var i = _providers.Count - 1; i >= 0; i--)
            {
                // remove the provider, along with any stale references
                var reference = _providers[i];
                if (!reference.TryGetTarget(out var target) || target == provider)
                {
                    _providers.RemoveAt(i);
                }
            }
        }
    }

    [NonEvent]
    private void WriteServiceProviderBuilt(MicrosoftServiceProvider provider)
    {
        if (IsEnabled(EventLevel.Informational, Keywords.ServiceProviderInitialized))
        {
            var singletonServices = 0;
            var scopedServices = 0;
            var transientServices = 0;
            var closedGenericsServices = 0;
            var openGenericsServices = 0;

            var descriptorBuilder = new StringBuilder("{ \"descriptors\":[ ");
            var firstDescriptor = true;
            foreach (var descriptor in provider.CallSiteFactory.Descriptors)
            {
                if (firstDescriptor)
                {
                    firstDescriptor = false;
                }
                else
                {
                    descriptorBuilder.Append(", ");
                }

                AppendServiceDescriptor(descriptorBuilder, descriptor);

                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        singletonServices++;
                        break;
                    case ServiceLifetime.Scoped:
                        scopedServices++;
                        break;
                    case ServiceLifetime.Transient:
                        transientServices++;
                        break;
                }

                if (descriptor.ServiceType.IsGenericType)
                {
                    if (descriptor.ServiceType.IsConstructedGenericType)
                    {
                        closedGenericsServices++;
                    }
                    else
                    {
                        openGenericsServices++;
                    }
                }
            }
            descriptorBuilder.Append(" ] }");

            var providerHashCode = provider.GetHashCode();
            ServiceProviderBuilt(providerHashCode, singletonServices, scopedServices, transientServices, closedGenericsServices, openGenericsServices);

            var descriptorString = descriptorBuilder.ToString();
            var chunkCount = descriptorString.Length / MaxChunkSize + (descriptorString.Length % MaxChunkSize > 0 ? 1 : 0);

            for (var i = 0; i < chunkCount; i++)
            {
                ServiceProviderDescriptors(
                    providerHashCode,
                    descriptorString.Substring(i * MaxChunkSize, Math.Min(MaxChunkSize, descriptorString.Length - i * MaxChunkSize)), i, chunkCount);
            }
        }
    }

    [NonEvent]
    private static void AppendServiceDescriptor(StringBuilder builder, ServiceDescriptor descriptor)
    {
        builder.Append("{ \"serviceType\": \"");
        builder.Append(descriptor.ServiceType);
        builder.Append("\", \"lifetime\": \"");
        builder.Append(descriptor.Lifetime);
        builder.Append("\", ");

        if (descriptor.HasImplementationType())
        {
            builder.Append("\"implementationType\": \"");
            builder.Append(descriptor.GetImplementationType());
        }
        else if (!descriptor.IsKeyedService && descriptor.ImplementationFactory != null)
        {
            builder.Append("\"implementationFactory\": \"");
            builder.Append(descriptor.ImplementationFactory.Method);
        }
        else if (descriptor.IsKeyedService && descriptor.KeyedImplementationFactory != null)
        {
            builder.Append("\"implementationFactory\": \"");
            builder.Append(descriptor.KeyedImplementationFactory.Method);
        }
        else if (descriptor.HasImplementationInstance())
        {
            var instance = descriptor.GetImplementationInstance();
            Debug.Assert(instance != null, "descriptor.ImplementationInstance != null");
            builder.Append("\"implementationInstance\": \"");
            builder.Append(instance.GetType());
            builder.Append(" (instance)");
        }
        else
        {
            builder.Append("\"unknown\": \"");
        }

        builder.Append("\" }");
    }

    protected override void OnEventCommand(EventCommandEventArgs command)
    {
        if (command.Command == EventCommand.Enable)
        {
            // When this EventSource becomes enabled, write out the existing ServiceProvider information
            // because building the ServiceProvider happens early in the process. This way a listener
            // can get this information, even if they attach while the process is running.

            lock (_providers)
            {
                foreach (var reference in _providers)
                {
                    if (reference.TryGetTarget(out var provider))
                    {
                        WriteServiceProviderBuilt(provider);
                    }
                }
            }
        }
    }
}