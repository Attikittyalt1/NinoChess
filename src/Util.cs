using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;

namespace NinoChess;

public static class Util
{
    extension<T>(T value) where T : IComparable<T>
    {
        public bool IsBetween(T min, T max, bool inclusiveLower = true, bool inclusiveUpper = true)
        {
            if (min.CompareTo(max) > 0) throw new ArgumentException("Min must be less than max");

            return 
                (inclusiveLower ? value.CompareTo(min) >= 0 : value.CompareTo(min) > 0) &&
                (inclusiveUpper ? value.CompareTo(max) <= 0 : value.CompareTo(max) < 0);
        }

        public T Clamp(T min, T max)
        {
            if (min.CompareTo(max) > 0) throw new ArgumentException("Min must be less than max");

            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;

            return value;
        }
    }

    extension<T>(T[,] array)
    {
        public IEnumerable<T> ToEnumerable()
        {
            foreach (var item in array)
                yield return item;
        }
    }

    private readonly static Dictionary<Type, object?> _defaultValuesCache = [];

    extension(Type type)
    {
        public object? DefaultValue
        {
            get
            {
                if (_defaultValuesCache.TryGetValue(type, out var value))
                {
                    return value;
                }

                value = Activator.CreateInstance(type);
                _defaultValuesCache.Add(type, value);
                return value;
            }
        }
    }

    extension(ServiceContainer container)
    {
        public void AddService<T>(object serviceInstance) => container.AddService(typeof(T), serviceInstance);
        public void AddService<T>(object serviceInstance, bool promote) => container.AddService(typeof(T), serviceInstance, promote);
        public void AddService<T>(ServiceCreatorCallback callback) => container.AddService(typeof(T), callback);
        public void AddService<T>(ServiceCreatorCallback callback, bool promote) => container.AddService(typeof(T), callback, promote);

        public void RemoveService<T>() => container.RemoveService(typeof(T));
        public void RemoveService<T>(bool promote) => container.RemoveService(typeof(T), promote);

        public T? GetService<T>() => (T?)container.GetService(typeof(T));
        public T GetService<T>(object defaultServiceInstance) => (T)container.GetService(typeof(T), defaultServiceInstance);
        public T GetService<T>(object defaultServiceInstance, bool promote) => (T)container.GetService(typeof(T), defaultServiceInstance, promote);


        public object GetService(Type serviceType, object defaultServiceInstance)
        {
            var service = container.GetService(serviceType);

            if (service == null)
            {
                container.AddService(serviceType, defaultServiceInstance);
                return defaultServiceInstance;
            }

            return service;
        }

        public object GetService(Type serviceType, object defaultServiceInstance, bool promote)
        {
            var service = container.GetService(serviceType);

            if (service == null)
            {
                container.AddService(serviceType, defaultServiceInstance, promote);
                return defaultServiceInstance;
            }

            return service;
        }
    }

    extension(Vector2 v)
    {
        public bool IsBetween(Vector2 p1, Vector2 p2, bool inclusiveLower = true, bool inclusiveUpper = true) => v.X.IsBetween(p1.X, p2.X, inclusiveLower, inclusiveUpper) && v.Y.IsBetween(p1.Y, p2.Y, inclusiveLower, inclusiveUpper);
        public Vector2 Clamp(Vector2 p1, Vector2 p2) => new(v.X.Clamp(p1.X, p2.X), v.Y.Clamp(p1.Y, p2.Y));
    }

    public static Vector2 MultiplyComponentWise(Vector2 v1, Vector2 v2) => new(v1.X * v2.X, v1.Y * v2.Y);
    public static Vector2 DivideComponentWise(Vector2 v1, Vector2 v2) => new(v1.X / v2.X, v1.Y / v2.Y);
    public static Vector2 ModulusComponentWise(Vector2 v1, Vector2 v2) => new(v1.X % v2.X, v1.Y % v2.Y);

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}