using NinoChess.Events;
using NinoChess.Mutations;
using System.Numerics;
using System.Reflection;

namespace NinoChess.Moves;

abstract class TypicalMove : Move
{
    public virtual bool CanTargetEnemy => false;
    public virtual bool CanTargetAlly => false;
    public virtual bool CanTargetNeutral => false;
    public virtual bool CanTargetEmpty => true;
    public virtual bool CanTargetEnPassent => true;
    public virtual bool IsBlockable => true;
    public virtual bool MovePieces => true;
    public virtual bool DestroyTarget => true;
    public virtual bool DisableAfterFirstMove => false;
    public virtual Position EffectiveOrigin => MoveInfo.Origin;
    public virtual bool EffectiveOriginCanTargetEnemy => true;
    public virtual bool EffectiveOriginCanTargetAlly => true;
    public virtual bool EffectiveOriginCanTargetNeutral => true;
    public virtual bool EffectiveOriginCanTargetEmpty => true;

    public override void Execute(MutationService mutationService, EventService eventService)
    {
        var sender = BoardState.Board.GetPieceAt(MoveInfo.Origin);

        if (DestroyTarget && BoardState.Board.TryGetPieceAt(MoveInfo.Target, out var piece))
        {
            var args = new Event_Destroy
            {
                MutationService = mutationService,
                Position = MoveInfo.Target
            };

            piece.OnDestroy(sender, args);

            mutationService.Execute(new Mutation_Destroy 
            { 
                Board = BoardState.Board, 
                Position = args.Position
            });

            eventService.Get<Event_Destroy>()?.Invoke(sender, args);
        }

        if (MovePieces)
        {
            var args = new Event_Swap
            {
                MutationService = mutationService,
                Positions = (EffectiveOrigin, MoveInfo.Target)
            };

            mutationService.Execute(new Mutation_Swap
            {
                Board = BoardState.Board,
                Positions = args.Positions
            });

            if (BoardState.Board.TryGetPieceAt(args.Positions.Item1, out var piece1))
            {
                piece1.OnSwap(sender, args);
            }

            if (BoardState.Board.TryGetPieceAt(args.Positions.Item2, out var piece2))
            {
                piece2.OnSwap(sender, args);
            }

            eventService.Get<Event_Swap>()?.Invoke(sender, args);
        }
    }

    public override bool IsValid() => (
           (CanTargetEmpty && BoardState.IsEmpty(MoveInfo.Target))
        || (CanTargetEnemy && BoardState.HasEnemy(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetAlly && BoardState.HasAlly(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetNeutral && BoardState.HasNeutral(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetEnPassent && BoardState.HasNeutral(MoveInfo.Target, MoveInfo.Origin)))
        && BoardState.Board.ContainsPosition(EffectiveOrigin)
        && (EffectiveOrigin == MoveInfo.Origin
        || (EffectiveOriginCanTargetEmpty && BoardState.IsEmpty(EffectiveOrigin))
        || (EffectiveOriginCanTargetEnemy && BoardState.HasEnemy(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetAlly && BoardState.HasAlly(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetNeutral && BoardState.HasNeutral(EffectiveOrigin, MoveInfo.Origin)))
        && !(DisableAfterFirstMove && BoardState.Board.GetPieceAt(MoveInfo.Origin).HasMoved)
        && !(IsBlockable && BoardState.HasPiecesBetween(EffectiveOrigin, MoveInfo.Target));
}