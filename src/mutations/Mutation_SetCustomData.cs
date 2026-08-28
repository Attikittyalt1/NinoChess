using System;

namespace NinoChess.Mutations;

public class Mutation_SetCustomData : IBoardStateMutation
{
    public required IBoard Board { get; init; }
    public required Position Position { get; init; }
    public required object? CustomData { get; init; }

    public void Execute()
    {
        Board.GetPieceAt(Position)._customData = CustomData;
    }

    public IBoardStateMutation GetInverse() => new Mutation_SetCustomData 
    { 
        Board = Board, 
        Position = Position, 
        CustomData = ((ICloneable?)Board.GetPieceAt(Position)._customData)?.Clone() 
    };
}