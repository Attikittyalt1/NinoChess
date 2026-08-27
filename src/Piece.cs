using System;
using System.Collections.Generic;
using System.Linq;

namespace NinoChess;

abstract class Piece(BoardState board, Position position, Transformation orientation, Allegience allegience) : IHasMoves, IHasPieceID
{
    public abstract RegistryID ID { get; }
    public BoardState Board { get => board; set => board = value; }
    public Position Position 
    { 
        get => position; 
        set 
        { 
            position = value;
            HasMoved = true;
        } 
    }
    public Transformation Orientation { get => orientation; set => orientation = value; }
    public Allegience Allegience { get => allegience; set => allegience = value; }
    public bool HasMoved { get; private set; } = false;

    public virtual bool IsSolid => true;
    public abstract int MaxMoveRange { get; }
    public virtual int CurrentTokenIndex => -1;

    public abstract IEnumerable<Move> GetMovesAt(Position p);

    public bool HasValidMoveAt(Position p) => GetMovesAt(p).Any(move => move.IsValid());

    public Move GetBestValidMoveAt(Position p) => GetMovesAt(p).SkipWhile(move => !move.IsValid()).First();

    public Position RelativePositionOf(Position p) => orientation * (p - position);

    public EventHandler? OnCreate;

    public EventHandler? OnDestroy;

    public EventHandler? OnSwap;
}

interface IHasMoves
{
    public bool HasValidMoveAt(Position p);
    public Move GetBestValidMoveAt(Position p);
}

interface IHasPieceID
{
    public RegistryID ID { get; }
}