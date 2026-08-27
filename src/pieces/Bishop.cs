using NinoChess.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NinoChess.Pieces;

class Bishop(BoardState Board, Position Position, Transformation Orientation, Allegience Allegience) : Piece(Board, Position, Orientation, Allegience)
{
    public override RegistryID ID => PieceID.Bishop;
    public override int MaxMoveRange => Range;
    public static int Range => 8;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (
            relativePos.IsInDirection(Position.NE, 1, Range, true, true)
            )
        {
            yield return new MoveOrAttackBlockable(Board, Position, p);
        }
    }
}