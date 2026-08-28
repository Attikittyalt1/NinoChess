using System;

namespace NinoChess.Mutations;

public class Mutation_Destroy(FullBoardState currentBoardState, Position target) : BoardStateEvent(currentBoardState)
{
    public Position Target => target;

    public override void Execute()
    {
        currentBoardState.Data.Board.RemovePieceAt(target);
    }

    public override IBoardStateMutation GetInverse() => new Mutation_Create(currentBoardState, currentBoardState.Data.Board.GetPieceAt(target));
}