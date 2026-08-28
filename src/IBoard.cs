using System.Collections.Generic;

namespace NinoChess;

public interface IBoard
{
    public int Size { get; }
    public IEnumerable<Position> GetValidPositions();

    public bool ContainsPosition(Position p);
    public bool HasPieceAt(Position p);
    public Piece GetPieceAt(Position p);

    public void RemovePieceAt(Position p);
    public void AddPieceAt(Position p, Piece piece);
    public void SwapPiecesAt(Position p1, Position p2);

    public bool IsPromotableTerritoryFor(Position p, Allegience team);
}