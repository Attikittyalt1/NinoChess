using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Bishop : Piece
{
    public override RegistryID ID => PieceID.Bishop;
    public override int MaxMoveRange => Range;
    public static int Range => 8;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.NE, 1, Range, true, true)
            )
        {
            yield return new MoveOrAttackBlockable
            {
                BoardState = BoardState,
                MoveInfo = new(Position, p)
            };
        }
    }
}