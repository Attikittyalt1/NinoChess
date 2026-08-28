using NinoChess.Moves;
using NinoChess.Mutations;
using System;
using System.Collections.Generic;
namespace NinoChess.Pieces;

// this piece was made by request of a friend. i do not personally mean anything by it
class Arab(FullBoardState boardState) : Piece(boardState)
{
    public override RegistryID ID => PieceID.Arab;
    public override int MaxMoveRange => 4;
    public int ExplosionRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.SW || relativePos == Position.SE)
        {
            yield return new MoveUnblockable(BoardState, new(Position, p));
        }

        if (relativePos == Position.N + Position.W * 4 || relativePos == Position.N + Position.E * 4)
        {
            yield return new MoveUnblockable(BoardState, new(Position, p));
        }
    }

    public override void OnSwap(Mutation_Swap eventInfo)
    {
        base.OnSwap(eventInfo);

        if (eventInfo.Sender is Arab && CanPromote())
        {
            foreach (var pos in Position.Range(Position + Position.Unit * -ExplosionRange, Position.Unit * (1 + ExplosionRange * 2)))
            {
                if (pos != Position && BoardState.Data.Board.ContainsPosition(pos) && BoardState.Data.Board.HasPieceAt(pos))
                {
                    BoardState.MutationHandler.Execute(new Mutation_Destroy(BoardState, this, pos));
                };
            }

            BoardState.MutationHandler.Execute(new Mutation_Destroy(BoardState, this, Position));
        }
    }
}