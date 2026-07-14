using System;
using System.Collections.Generic;

namespace GlitchCompiler.Core
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();
        public void Subscribe<T>(Action<T> handler) { handlers.TryGetValue(typeof(T), out var current); handlers[typeof(T)] = Delegate.Combine(current, handler); }
        public void Unsubscribe<T>(Action<T> handler) { if (handlers.TryGetValue(typeof(T), out var current)) handlers[typeof(T)] = Delegate.Remove(current, handler); }
        public void Publish<T>(T message) { if (handlers.TryGetValue(typeof(T), out var current)) ((Action<T>)current)?.Invoke(message); }
    }
}
