using NinoChess.Moves;
using NinoChess.Mutations;
using System;
using System.Collections.Generic;

namespace NinoChess.Pieces;

class Scholar(FullBoardState boardState) : Piece<Scholar.ScholarData>(boardState)
{
    public sealed class ScholarData : ICloneable
    {
        public Mode CurrentMode { get; set; }
        public object Clone() => new ScholarData() { CurrentMode = CurrentMode };
    }
    public enum Mode
    {
        Agile = 0,
        Aggressive = 1
    }

    public override RegistryID ID => PieceID.Scholar;
    public override int MaxMoveRange => Range;
    public static int Range => 3;

    public override ScholarData GetDefaultData() => new() { CurrentMode = Mode.Agile };
    public override int CurrentTokenIndex => (int)CustomData.CurrentMode;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (
            relativePos.IsInDirection(Position.N, 1, 1, true, true)
            )
        {
            yield return new MoveOrSwapBlockable(BoardState, new(Position, p));
        }

        if (
            relativePos.IsInDirection(Position.N, 3, 3, true, true) ||
            relativePos.IsInDirection(Position.NE, 2, 2, true, true)
            )
        {
            if (CustomData.CurrentMode == Mode.Agile)
            {
                yield return new MoveUnblockable(BoardState, new(Position, p));
            }

            if (CustomData.CurrentMode == Mode.Aggressive)
            {
                yield return new AttackUnblockable(BoardState, new(Position, p));
            }
        }
    }

    public override void OnSwap(Mutation_Swap eventInfo)
    {
        base.OnSwap(eventInfo);

        if (BoardState.Data.Board.HasPieceAt(eventInfo.Pos.Item1) && BoardState.Data.Board.HasPieceAt(eventInfo.Pos.Item2))
        {
            var newMode = CustomData.CurrentMode switch
            {
                Mode.Agile => Mode.Aggressive,
                Mode.Aggressive => Mode.Agile,
                var mode => mode
            };

            BoardState.MutationHandler.Execute(new Mutation_SetCustomData
            (
                currentBoardState: boardState,
                sender: this,
                target: Position,
                data: new ScholarData() { CurrentMode = newMode}
            ), false);
        }
    }
}