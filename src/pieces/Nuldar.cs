using NinoChess.Moves;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NinoChess.Pieces;

class Nuldar(FullBoardState boardState) : Piece(boardState)
{
    public override RegistryID ID => PieceID.Nuldar;
    public override int MaxMoveRange => 3;

    private static List<Position> SwappablePositions => [
            new(1, 0), new(2, 0), new(3, 0), new(2, 1), new(2, -1)
        ];

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.S || relativePos == Position.NW || relativePos == Position.NE)
        {
            yield return new MoveBlockable(Board, new(Position, p));
        }

        if (relativePos == Position.N)
        {
            yield return new FirstMoveBlockable(Board, new(Position, p));
        }

        if (SwappablePositions.Any(pos => pos == relativePos with { X = Math.Abs(relativePos.X) }))
        {

            yield return new AlternateSwapUnblockable(Board, new(Position, p), (p-Position).ReflectAcross(Orientation * Position.N) + Position);
        }
    }
}