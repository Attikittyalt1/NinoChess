using NinoChess.Moves;
using System.Collections.Generic;
namespace NinoChess.Pieces;

class Pawn(FullBoardState boardState) : Piece(boardState)
{
    public override RegistryID ID => PieceID.Pawn;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.N)
        {
            yield return new MoveBlockable(BoardState, new(Position, p));
        }

        if (relativePos == Position.N * 2)
        {
            yield return new FirstMoveBlockable(BoardState, new(Position, p));
        }

        if (relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new AttackBlockable(BoardState, new(Position, p));
        }
    }
}