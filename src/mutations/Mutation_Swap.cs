using System;

namespace NinoChess.Mutations;

public class Mutation_Swap(FullBoardState currentBoardState, object? sender, (Position, Position) pos) : BoardStateEvent(currentBoardState, sender)
{
    public (Position, Position) Pos => pos;

    private (bool, bool) NewHasMoved { get; set; } = (true, true);

    public override void Execute()
    {
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item1)?.Position = pos.Item2;
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item2)?.Position = pos.Item1;

        currentBoardState.Data.Board.SwapPiecesAt(pos.Item1, pos.Item2);

        currentBoardState.Data.Board.TryGetPieceAt(pos.Item1)?.HasMoved = NewHasMoved.Item1;
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item2)?.HasMoved = NewHasMoved.Item2;

        currentBoardState.Data.Board.TryGetPieceAt(pos.Item1)?.OnSwap(this);
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item2)?.OnSwap(this);
    }


    public override IBoardStateMutation GetInverse() => new Mutation_Swap(currentBoardState, sender, pos) { NewHasMoved = (
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item1)?.HasMoved ?? false,
        currentBoardState.Data.Board.TryGetPieceAt(pos.Item2)?.HasMoved ?? false
        )};
}