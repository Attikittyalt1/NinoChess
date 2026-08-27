using System;
using System.Collections.Generic;

namespace NinoChess;

public readonly record struct RegistryID(Enum Value) : IEquatable<RegistryID>, IComparable<RegistryID>
{
    public Type EnumType => Value.GetType();
    public bool IsNone => Value == EnumType.DefaultValue;

    public static implicit operator RegistryID(Enum value) => new(value);
    public static implicit operator Enum(RegistryID id) => id.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(RegistryID other) => 
        EnumType == other.EnumType ? 
        Value.CompareTo(other.Value) : 
        EnumType.FullName?.CompareTo(other.EnumType.FullName) ?? -1;

    public override string ToString() => Value.ToString();
}