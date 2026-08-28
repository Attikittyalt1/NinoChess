using NinoChess.Moves;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Dannel(BoardState board, Position position, Transformation orientation, Allegience allegience) : Piece(board, position, orientation, allegience)
{
    public override RegistryID ID => PieceID.Dannel;
    public override int MaxMoveRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.S)
        {
            yield return new MoveOrAttackBlockable(Board, new(Position, p));
        }

        if (relativePos == Position.N || relativePos == Position.N + Position.NW || relativePos == Position.N + Position.NE)
        {
            yield return new AttackBlockable(Board, new(Position, p));
        }

        if (relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new SwapBlockable(Board, new(Position, p));
        }
    }
}