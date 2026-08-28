using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class King(BoardState board, Position position, Transformation orientation, Allegience allegience) : Piece(board, position, orientation, allegience)
{
    public override RegistryID ID => PieceID.King;
    public override int MaxMoveRange => Range;
    public static int Range => 1;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.N, 1, Range, true, true) ||
            relativePos.IsInDirection(Position.NE, 1, Range, true, true)
            )
        {
            yield return new MoveOrAttackBlockable(Board, new(Position, p));
        }
    }
}