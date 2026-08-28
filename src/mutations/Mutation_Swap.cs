using System;

namespace NinoChess.Mutations;

public class Mutation_Swap : IBoardStateMutation
{
    public required IBoard Board { get; init; }
    public required (Position, Position) Positions { get; init; }
    private (bool, bool) NewHasMoved { get; set; } = (true, true);

    public void Execute()
    {
        if (Board.TryGetPieceAt(Positions.Item1, out var piece1)) {
            piece1.Position = Positions.Item2;
            piece1.HasMoved = NewHasMoved.Item1;
        }

        if (Board.TryGetPieceAt(Positions.Item2, out var piece2))
        {
            piece2.Position = Positions.Item1;
            piece2.HasMoved = NewHasMoved.Item2;
        }

        Board.SwapPiecesAt(Positions.Item1, Positions.Item2);
    }


    public IBoardStateMutation GetInverse() => new Mutation_Swap 
    { 
        Board = Board,
        Positions = Positions,
        NewHasMoved = (
            Board.TryGetPieceAt(Positions.Item1, out var piece1) && piece1.HasMoved,
            Board.TryGetPieceAt(Positions.Item2, out var piece2) && piece2.HasMoved
        )
    };
}