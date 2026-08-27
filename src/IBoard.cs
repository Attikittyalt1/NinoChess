using System.Collections.Generic;

namespace NinoChess;

interface IBoard
{
    public int Size { get; }
    public IEnumerable<Position> GetValidPositions();

    public bool ContainsPosition(Position p);
    public bool HasPieceAt(Position p);
    public Piece GetPieceAt(Position p);

    public void RemovePieceAt(Position p);
    public void AddPieceAt(Position p, Piece piece);
    public void SwapPiecesAt(Position p1, Position p2);
}