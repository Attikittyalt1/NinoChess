using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Knight(BoardState board, Position position, Transformation orientation, Allegience allegience) : Piece(board, position, orientation, allegience)
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
            yield return new MoveOrAttackUnblockable(Board, new(Position, p));
        }
    }
}