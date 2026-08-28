using NinoChess.Moves;
using System;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Scholar(BoardState board, Position position, Transformation orientation, Allegience allegience) : Piece(board, position, orientation, allegience)
{
    public enum Mode
    {
        Agile = 0,
        Aggressive = 1
    }

    public override void OnSwap(BoardState.PieceSwapInfo info)
    {
        if (Board.HasPieceAt(info.P1) && Board.HasPieceAt(info.P2))
        {
            CurrentMode = CurrentMode switch
            {
                Mode.Agile => Mode.Aggressive,
                Mode.Aggressive => Mode.Agile,
                var mode => mode
            };
        }
    }

    public override RegistryID ID => PieceID.Scholar;
    public override int MaxMoveRange => Range;
    public static int Range => 3;

    public Mode CurrentMode = Mode.Agile;
    public override int CurrentTokenIndex => (int)CurrentMode;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.N, 1, 1, true, true)
            )
        {
            yield return new MoveOrSwapBlockable(Board, new(Position, p));
        }

        if (
            relativePos.IsInDirection(Position.N, 3, 3, true, true) ||
            relativePos.IsInDirection(Position.NE, 2, 2, true, true)
            )
        {
            if (CurrentMode == Mode.Agile)
            {
                yield return new MoveUnblockable(Board, new(Position, p));
            }

            if (CurrentMode == Mode.Aggressive)
            {
                yield return new AttackUnblockable(Board, new(Position, p));
            }
        }
    }
}