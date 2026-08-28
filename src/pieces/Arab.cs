using NinoChess.Events;
using NinoChess.Moves;
using NinoChess.Mutations;
using System;
using System.Collections.Generic;
namespace NinoChess.Pieces;

// this piece was made by request of a friend. i do not personally mean anything by it
class Arab : Piece
{
    public override RegistryID ID => PieceID.Arab;
    public override int MaxMoveRange => 4;
    public int ExplosionRange => 2;

    public override IEnumerable<Move> GetMovesAt(Position p)
    {
        var relativePos = ToRelativePosition(p);

        if (relativePos == Position.SW || relativePos == Position.SE)
        {
            yield return new MoveUnblockable 
            { 
                BoardState = BoardState, 
                MoveInfo = new(Position, p)
            };
        }

        if (relativePos == Position.N + Position.W * 4 || relativePos == Position.N + Position.E * 4)
        {
            yield return new MoveUnblockable
            {
                BoardState = BoardState,
                MoveInfo = new(Position, p)
            };
        }
    }

    public override void OnSwap(object? sender, Event_Swap eventInfo)
    {
        base.OnSwap(sender, eventInfo);

        if (sender is Arab && CanPromote())
        {
            foreach (var pos in Position.Range(Position + Position.Unit * -ExplosionRange, Position.Unit * (1 + ExplosionRange * 2)))
            {
                if (pos != Position && BoardState.Board.ContainsPosition(pos) && BoardState.Board.HasPieceAt(pos))
                {
                    eventInfo.MutationService.Execute(new Mutation_Destroy 
                    { 
                        Board = BoardState.Board, 
                        Position = pos
                    });
                };
            }

            eventInfo.MutationService.Execute(new Mutation_Destroy 
            { 
                Board = BoardState.Board, 
                Position = Position
            });
        }
    }
}