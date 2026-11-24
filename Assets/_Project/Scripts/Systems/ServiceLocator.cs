using System;
using System.Collections.Generic;

namespace StaticLeakLab.Systems
{
    /// <summary>
    /// HAZARD #6 — Static service locator / dictionary keyed by type.
    ///
    /// Register() refuses duplicates, which is a sane rule inside one Play session.
    /// With Domain Reload DISABLED, the services registered in session 1 are still
    /// there when session 2 starts, so the bootstrap's Register() call throws:
    ///   InvalidOperationException: Service GameBootstrap is already registered.
    /// and the game fails to start. Resolve() on the other hand happily returns the
    /// destroyed service from the previous session.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_Services = new Dictionary<Type, object>();

        public static int Count => s_Services.Count;

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (s_Services.ContainsKey(type))
                throw new InvalidOperationException(
                    $"Service {type.Name} is already registered. " +
                    "(Did a previous Play session leave it behind?)");

            s_Services.Add(type, service);
        }

        public static T Resolve<T>() where T : class =>
            s_Services.TryGetValue(typeof(T), out var service) ? (T)service : null;

        public static bool IsRegistered<T>() => s_Services.ContainsKey(typeof(T));
    }
}
