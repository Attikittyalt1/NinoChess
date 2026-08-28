using System;

namespace NinoChess.Mutations;

public class Mutation_SetCustomData(FullBoardState currentBoardState, Position target, object? data) : BoardStateEvent(currentBoardState)
{
    public override void Execute()
    {
        currentBoardState.Data.Board.GetPieceAt(target)._customData = data;
    }


    public override IBoardStateMutation GetInverse() => new Mutation_SetCustomData(currentBoardState, target, ((ICloneable?)currentBoardState.Data.Board.GetPieceAt(target)._customData)?.Clone());
}