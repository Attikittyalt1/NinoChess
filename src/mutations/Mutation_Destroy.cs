using System;

namespace NinoChess.Mutations;

public class Mutation_Destroy : IBoardStateMutation
{
    public required IBoard Board { get; init; }
    public required Position Position { get; init; }

    public void Execute()
    {
        Board.RemovePieceAt(Position);
    }

    public IBoardStateMutation GetInverse() => new Mutation_Create 
    { 
        Board = Board, 
        Piece = Board.GetPieceAt(Position) 
    };
}