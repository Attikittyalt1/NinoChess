using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Knight(FullBoardState boardState) : Piece(boardState)
{
    public override RegistryID ID => PieceID.Knight;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.N + Position.NE, 1, 1, true, true) ||
            relativePos.IsInDirection(Position.N + Position.NW, 1, 1, true, true)
            )
        {
            yield return new MoveOrAttackUnblockable(BoardState, new(Position, p));
        }
    }
}