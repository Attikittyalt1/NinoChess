using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NinoChess;

public class EventService
{
    private readonly Dictionary<Type, EventHandler> _events = [];

    public EventHandler? Get<T>() where T : EventArgs => Get(typeof(T));

    public EventHandler? Get(Type t) => _events.GetValueOrDefault(t);

    public void Add<T>(EventHandler eventHandler) where T : EventArgs => Add(typeof(T), eventHandler);

    public void Add(Type t, EventHandler eventHandler)
    {
        _events[t] = (EventHandler)Delegate.Combine(Get(t), eventHandler);
    }

    public void Remove<T>(EventHandler eventHandler) where T : EventArgs => Remove(typeof(T), eventHandler);

    public void Remove(Type t, EventHandler eventHandler)
    {
        var value = (EventHandler?)Delegate.Remove(Get(t), eventHandler);

        if (value is not null)
        {
            _events[t] = value;
        }
        else
        {
            _events.Remove(t);
        }
    }
}