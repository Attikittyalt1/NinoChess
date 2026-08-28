using System;

namespace NinoChess.Mutations;

public class Mutation_Create : IBoardStateMutation
{
    public required IBoard Board { get; init; }
    public required Piece Piece { get; init; }

    public void Execute()
    {
        Board.AddPieceAt(Piece.Position, Piece);
    }

    public IBoardStateMutation GetInverse() => new Mutation_Destroy 
    { 
        Board = Board, 
        Position = Piece.Position
    };
}