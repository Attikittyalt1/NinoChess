using NinoChess.Moves;
using System.Collections.Generic;
namespace NinoChess.Pieces;

class Pawn(BoardState board, Position position, Transformation orientation, Allegience allegience) : Piece(board, position, orientation, allegience)
{
    public override RegistryID ID => PieceID.Pawn;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.N)
        {
            yield return new MoveBlockable(Board, new(Position, p));
        }

        if (relativePos == Position.N * 2)
        {
            yield return new FirstMoveBlockable(Board, new(Position, p));
        }

        if (relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new AttackBlockable(Board, new(Position, p));
        }
    }
}