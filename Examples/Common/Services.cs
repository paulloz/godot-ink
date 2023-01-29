using GodotInk.Examples.TeenyTiny; // Boo, dirty.
using System;
using System.Collections.Generic;

namespace GodotInk.Examples;

public static class Locator
{
    private static readonly Dictionary<Type, IService> services = new();

    public static T Fetch<T>()
    where T : IService
    {
        services[typeof(T)] ??= BuildDefaultService<T>();
        return (T)services[typeof(T)];
    }

    public static void Provide<T>(T service)
    where T : IService
    {
        services[typeof(T)] = service;
    }

    public static void Unprovide<T>(T service)
    where T : IService
    {
        if (services.TryGetValue(typeof(T), out IService? knownService) && knownService == (IService)service)
            services[typeof(T)] = BuildDefaultService<T>();
    }

    private static IService BuildDefaultService<T>()
    where T : IService
    {
        if (typeof(T) == typeof(IDialogueService))
            return new EmptyDialogueService();

        throw new Exception($"Unknown service: {typeof(T)}");
    }
}

public interface IService
{
}
