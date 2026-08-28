using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NinoChess;

public class EventService
{
    private readonly Dictionary<Type, object> _eventHandlers = [];

    public EventHandler<T>? Get<T>() => (EventHandler<T>?) _eventHandlers.GetValueOrDefault(typeof(T));

    public object? Get(Type t) => _eventHandlers.GetValueOrDefault(t);

    public void Add<T>(EventHandler<T> eventHandler)
    {
        _eventHandlers[typeof(T)] = Delegate.Combine(Get<T>(), eventHandler);
    }

    public void Remove<T>(EventHandler<T> eventHandler)
    {
        var value = Delegate.Remove(Get<T>(), eventHandler);

        if (value is not null)
        {
            _eventHandlers[typeof(T)] = value;
        }
        else
        {
            _eventHandlers.Remove(typeof(T));
        }
    }
}