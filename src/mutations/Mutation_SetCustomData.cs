using System;

namespace NinoChess.Mutations;

public class Mutation_SetCustomData(FullBoardState currentBoardState, object? sender, Position target, object? data) : BoardStateEvent(currentBoardState, sender)
{
    public override void Execute()
    {
        currentBoardState.Data.Board.GetPieceAt(target)._customData = data;
    }


    public override IBoardStateMutation GetInverse() => new Mutation_SetCustomData(currentBoardState, sender, target, ((ICloneable?)currentBoardState.Data.Board.GetPieceAt(target)._customData)?.Clone());
}