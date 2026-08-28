using NinoChess.Events;
using NinoChess.Mutations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NinoChess;

public abstract class Piece : IHasMoves, IHasPieceID
{
    public abstract RegistryID ID { get; }
    public required BoardStateData BoardState { get; init; }
    public required EventService EventService { get; init; }

    public required Position Position { get; set; }
    public required Transformation Orientation { get; set; }
    public required Allegience Allegience { get; set; }

    public bool HasMoved { get; set; } = false;

    public virtual bool IsSolid => true;
    public abstract int MaxMoveRange { get; }
    public virtual int CurrentTokenIndex => -1;

    public object? _customData;

    public abstract IEnumerable<Move> GetMovesAt(Position p);

    public bool HasValidMoveAt(Position p) => GetMovesAt(p).Any(move => move.IsValid());

    public Move GetBestValidMoveAt(Position p) => GetMovesAt(p).SkipWhile(move => !move.IsValid()).First();

    public Move? TryGetBestValidMoveAt(Position p) => GetMovesAt(p).SkipWhile(move => !move.IsValid()).FirstOrDefault(defaultValue: null);

    public Position ToRelativePosition(Position p) => Orientation * (p - Position);

    public Position ToAbsolutePosition(Position p) => Orientation.Inverse * p + Position;
    public bool CanPromote() => BoardState.Board.IsPromotableTerritoryFor(Position, Allegience);

    public virtual void OnCreate(object? sender, Event_Create eventInfo) { }
    public virtual void OnDestroy(object? sender, Event_Destroy eventInfo) { }
    public virtual void OnSwap(object? sender, Event_Swap eventInfo) { }
}

public abstract class Piece<TData> : Piece
    where TData : ICloneable
{
    public TData? CustomData => (TData?) _customData;

    public override void OnCreate(object? sender, Event_Create eventInfo)
    {
        base.OnCreate(sender, eventInfo);
        _customData = GetDefaultData();
    }

    public virtual TData? GetDefaultData() => default;
}

public interface IHasMoves
{
    public bool HasValidMoveAt(Position p);
    public Move GetBestValidMoveAt(Position p);
}

public interface IHasPieceID
{
    public RegistryID ID { get; }
}