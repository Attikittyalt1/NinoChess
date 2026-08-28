using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NinoChess;

public class Grid(Position Dimensions) : IBoard
{
    public readonly Position Dimensions = Dimensions;

    private readonly Piece?[,] _pieces = new Piece?[Dimensions.X, Dimensions.Y];

    public Grid(int Dimensions) : this(Position.Unit * Dimensions)
    {

    }

    public int Size => _pieces.Length;
    public IEnumerable<Position> GetValidPositions() => Position.Range(Position.Zero, Dimensions);

    public bool ContainsPosition(Position p) => p.IsBetween(Position.Zero, Dimensions, true, false);
    public bool HasPieceAt(Position p) => _pieces[p.X, p.Y] is not null;
    public Piece GetPieceAt(Position p) => _pieces[p.X, p.Y] ?? throw new NullReferenceException();

    public void RemovePieceAt(Position p)
    {
        Guard.IsNotNull(_pieces[p.X, p.Y]);

        _pieces[p.X, p.Y] = null;
    }
    public void AddPieceAt(Position p, Piece piece)
    {
        Guard.IsNull(_pieces[p.X, p.Y]);

        _pieces[p.X, p.Y] = piece;
    }
    public void SwapPiecesAt(Position p1, Position p2)
    {
        (_pieces[p2.X, p2.Y], _pieces[p1.X, p1.Y]) = (_pieces[p1.X, p1.Y], _pieces[p2.X, p2.Y]);
    }

    public bool IsPromotableTerritoryFor(Position p, Allegience team)
    {
        if (team == Allegience.White)
        {
            return p.Y == Dimensions.Y - 1;
        }

        if (team == Allegience.Black)
        {
            return p.Y == 0;
        }

        return false;
    }

    public void Print()
    {
        Debug.WriteLine("Grid Size: {0}", Dimensions);

        foreach (var piece in _pieces)
        {
            if (piece is not null)
            {
                Debug.WriteLine("Piece at {0} is of type {1}", piece.Position, piece.GetType());
            }
        }
    }
}