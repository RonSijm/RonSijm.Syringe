using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicrosoftCopy.DependencyInjection.Tests.Shared;

public static class ReflectionHelper
{
    public static object? InvokeGetImplementationType(this ServiceDescriptor serviceDescriptorInstance)
    {
        if (serviceDescriptorInstance == null)
            throw new ArgumentNullException(nameof(serviceDescriptorInstance));

        var type = serviceDescriptorInstance.GetType();
        var methodInfo = type.GetMethod("GetImplementationType", BindingFlags.NonPublic | BindingFlags.Instance);

        if (methodInfo == null)
            throw new InvalidOperationException("Method 'GetImplementationType' not found.");

        return methodInfo.Invoke(serviceDescriptorInstance, null);
    }
}