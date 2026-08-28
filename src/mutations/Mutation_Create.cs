using System;

namespace NinoChess.Mutations;

public class Mutation_Create(FullBoardState currentBoardState, object? sender, Piece piece) : BoardStateEvent(currentBoardState, sender)
{
    public Piece Piece => piece;

    public override void Execute()
    {
        currentBoardState.Data.Board.AddPieceAt(piece.Position, piece);
        piece.OnCreate(this);
    }

    public override IBoardStateMutation GetInverse() => new Mutation_Destroy(currentBoardState, sender, piece.Position);
}