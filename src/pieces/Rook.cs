using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Rook(FullBoardState boardState) : Piece(boardState)
{
    public override RegistryID ID => PieceID.Rook;
    public override int MaxMoveRange => Range;
    public static int Range => 8;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.N, 1, Range, true, true)
            )
        {
            yield return new MoveOrAttackBlockable(BoardState, new(Position, p));
        }
    }
}