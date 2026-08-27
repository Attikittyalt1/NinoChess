using NinoChess.Moves;
using System;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Scholar : Piece
{
    public enum Mode
    {
        Agile = 0,
        Aggressive = 1
    }

    public Scholar(BoardState board, Position position, Transformation orientation, Allegience allegience) : base(board, position, orientation, allegience)
    {
        OnSwap += (o, e) =>
        {
            var info = (BoardState.PieceSwapInfo)e;

            if (board.TryGetPieceAt(info.P1) is Scholar && board.TryGetPieceAt(info.P2) is Scholar)
            {
                CurrentMode = CurrentMode switch
                {
                    Mode.Agile => Mode.Aggressive,
                    Mode.Aggressive => Mode.Agile,
                    var mode => mode
                };
            }
        };
    }

    public override RegistryID ID => PieceID.Scholar;
    public override int MaxMoveRange => Range;
    public static int Range => 3;

    public Mode CurrentMode = Mode.Agile;
    public override int CurrentTokenIndex => (int)CurrentMode;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = RelativePositionOf(p);

        if (
            relativePos.IsInDirection(Position.N, 1, 1, true, true)
            )
        {
            yield return new MoveOrSwapBlockable(Board, Position, p);
        }

        if (
            relativePos.IsInDirection(Position.NE, 2, 2, true, true)
            )
        {
            if (CurrentMode == Mode.Agile)
            {
                yield return new MoveUnblockable(Board, Position, p);
            }

            if (CurrentMode == Mode.Aggressive)
            {
                yield return new AttackUnblockable(Board, Position, p);
            }
        }
    }
}