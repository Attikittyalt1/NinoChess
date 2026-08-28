using System;

namespace NinoChess.Mutations;

public class Mutation_Destroy(FullBoardState currentBoardState, object? sender, Position target) : BoardStateEvent(currentBoardState, sender)
{
    public Position Target => target;

    public override void Execute()
    {
        currentBoardState.Data.Board.GetPieceAt(target).OnDestroy(this);
        currentBoardState.Data.Board.RemovePieceAt(target);
    }

    public override IBoardStateMutation GetInverse() => new Mutation_Create(currentBoardState, sender, currentBoardState.Data.Board.GetPieceAt(target));
}