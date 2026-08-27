using NinoChess.Moves;
using System;
using System.Collections.Generic;
namespace NinoChess.Pieces;

class Pawn(BoardState Board, Position Position, Transformation Orientation, Allegience Allegience) : Piece(Board, Position, Orientation, Allegience)
{
    public override RegistryID ID => PieceID.Pawn;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (relativePos == Position.N)
        {
            yield return new MoveBlockable(Board, Position, p);
        }

        if (relativePos == Position.N * 2)
        {
            yield return new FirstMoveBlockable(Board, Position, p);
        }

        if (relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new AttackBlockable(Board, Position, p);
        }
    }
}