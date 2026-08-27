using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Knight(BoardState Board, Position Position, Transformation Orientation, Allegience Allegience) : Piece(Board, Position, Orientation, Allegience)
{
    public override RegistryID ID => PieceID.Knight;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (
            relativePos.IsInDirection(Position.N + Position.NE, 1, 1, true, true) ||
            relativePos.IsInDirection(Position.N + Position.NW, 1, 1, true, true)
            )
        {
            yield return new MoveOrAttackBlockable(Board, Position, p);
        }
    }
}