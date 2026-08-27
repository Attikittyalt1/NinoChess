using NinoChess.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NinoChess.Pieces;

class King(BoardState Board, Position Position, Transformation Orientation, Allegience Allegience) : Piece(Board, Position, Orientation, Allegience)
{
    public override RegistryID ID => PieceID.King;
    public override int MaxMoveRange => Range;
    public static int Range => 1;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (
            relativePos.IsInDirection(Position.N, 1, Range, true, true) ||
            relativePos.IsInDirection(Position.NE, 1, Range, true, true)
            )
        {
            yield return new MoveOrAttackBlockable(Board, Position, p);
        }
    }
}