using NinoChess.Moves;
using System;
using System.Collections.Generic;
namespace NinoChess.Pieces;

class Dannel(BoardState Board, Position Position, Transformation Orientation, Allegience Allegience) : Piece(Board, Position, Orientation, Allegience)
{
    public override RegistryID ID => PieceID.Dannel;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (relativePos == Position.S)
        {
            yield return new MoveOrAttackBlockable(Board, Position, p);
        }

        if (relativePos == Position.N)
        {
            yield return new AttackBlockable(Board, Position, p);
        }

        if (relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new SwapBlockable(Board, Position, p);
        }

        if (relativePos == Position.N + Position.NW || relativePos == Position.N + Position.NE)
        {
            yield return new AttackBlockable(Board, Position, p);
        }
    }
}