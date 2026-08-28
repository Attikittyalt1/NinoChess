using System;

namespace NinoChess.Mutations;

public class Mutation_Create(FullBoardState currentBoardState, Piece piece) : BoardStateEvent(currentBoardState)
{
    public Piece Piece => piece;

    public override void Execute()
    {
        currentBoardState.Data.Board.AddPieceAt(piece.Position, piece);
        piece.OnCreate(this);
    }

    public override IBoardStateMutation GetInverse() => new Mutation_Destroy(currentBoardState, piece.Position);
}