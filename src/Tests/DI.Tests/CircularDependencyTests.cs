// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using MicrosoftCopy.DependencyInjection.Tests.Fakes;
using RonSijm.Syringe;
using Xunit;

namespace MicrosoftCopy.DependencyInjection.Tests;

public class CircularDependencyTests
{
    [Fact]
    public void SelfCircularDependency()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency";

        var serviceProvider = new ServiceCollection()
            .AddTransient<SelfCircularDependency>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<SelfCircularDependency>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void SelfCircularDependencyInEnumerable()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency'." +
                              Environment.NewLine +
                              "System.Collections.Generic.IEnumerable<MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency> -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependency";

        var serviceProvider = new ServiceCollection()
            .AddTransient<SelfCircularDependency>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IEnumerable<SelfCircularDependency>>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void SelfCircularDependencyGenericDirect()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string>'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string> -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string>";

        var serviceProvider = new ServiceCollection()
            .AddTransient<SelfCircularDependencyGeneric<string>>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<string>>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void SelfCircularDependencyGenericIndirect()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string>'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<int> -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string> -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyGeneric<string>";

        var serviceProvider = new ServiceCollection()
            .AddTransient<SelfCircularDependencyGeneric<int>>()
            .AddTransient<SelfCircularDependencyGeneric<string>>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<int>>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NoCircularDependencyGeneric()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(new SelfCircularDependencyGeneric<string>())
            .AddTransient<SelfCircularDependencyGeneric<int>>()
            .BuildServiceProvider();

        // This will not throw because we are creating an instance of the first time
        // using the parameterless constructor which has no circular dependency
        var resolvedService = serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<int>>();
        Assert.NotNull(resolvedService);
    }

    [Fact]
    public void SelfCircularDependencyWithInterface()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.ISelfCircularDependencyWithInterface'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyWithInterface -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.ISelfCircularDependencyWithInterface" +
                              "(MicrosoftCopy.DependencyInjection.Tests.Fakes.SelfCircularDependencyWithInterface) -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.ISelfCircularDependencyWithInterface";

        var serviceProvider = new ServiceCollection()
            .AddTransient<ISelfCircularDependencyWithInterface, SelfCircularDependencyWithInterface>()
            .AddTransient<SelfCircularDependencyWithInterface>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<SelfCircularDependencyWithInterface>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void DirectCircularDependency()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyB -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA";

        var serviceProvider = new ServiceCollection()
            .AddSingleton<DirectCircularDependencyA>()
            .AddSingleton<DirectCircularDependencyB>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<DirectCircularDependencyA>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void IndirectCircularDependency()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.IndirectCircularDependencyA'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.IndirectCircularDependencyA -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.IndirectCircularDependencyB -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.IndirectCircularDependencyC -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.IndirectCircularDependencyA";

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IndirectCircularDependencyA>()
            .AddTransient<IndirectCircularDependencyB>()
            .AddTransient<IndirectCircularDependencyC>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IndirectCircularDependencyA>());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NoCircularDependencySameTypeMultipleTimes()
    {
        var serviceProvider = new ServiceCollection()
            .AddTransient<NoCircularDependencySameTypeMultipleTimesA>()
            .AddTransient<NoCircularDependencySameTypeMultipleTimesB>()
            .AddTransient<NoCircularDependencySameTypeMultipleTimesC>()
            .BuildServiceProvider();

        var resolvedService = serviceProvider.GetRequiredService<NoCircularDependencySameTypeMultipleTimesA>();
        Assert.NotNull(resolvedService);
    }

    [Fact]
    public void DependencyOnCircularDependency()
    {
        var expectedMessage = "A circular dependency was detected for the service of type " +
                              "'MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA'." +
                              Environment.NewLine +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DependencyOnCircularDependency -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyB -> " +
                              "MicrosoftCopy.DependencyInjection.Tests.Fakes.DirectCircularDependencyA";

        var serviceProvider = new ServiceCollection()
            .AddTransient<DependencyOnCircularDependency>()
            .AddTransient<DirectCircularDependencyA>()
            .AddTransient<DirectCircularDependencyB>()
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<DependencyOnCircularDependency>());

        Assert.Equal(expectedMessage, exception.Message);
    }
}