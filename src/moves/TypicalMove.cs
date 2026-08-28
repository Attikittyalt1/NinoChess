using NinoChess.Mutations;
using System.Numerics;
using System.Reflection;

namespace NinoChess.Moves;

abstract record TypicalMove(FullBoardState BoardState, MoveInfo MoveInfo) : Move(BoardState, MoveInfo)
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

    public override void Execute()
    {
        var sender = BoardState.Data.Board.GetPieceAt(MoveInfo.Origin);

        if (DestroyTarget && BoardState.Data.Board.HasPieceAt(MoveInfo.Target))
        {
            Board.MutationHandler.Execute(new Mutation_Destroy
            (
                currentBoardState: BoardState,
                sender: sender,
                target: MoveInfo.Target
            ));
        }

        if (MovePieces)
        {
            Board.MutationHandler.Execute(new Mutation_Swap
            (
                currentBoardState: BoardState,
                sender: sender,
                pos: (EffectiveOrigin, MoveInfo.Target)
            ));
        }
    }

    public override bool IsValid() => (
           (CanTargetEmpty && BoardState.Data.IsEmpty(MoveInfo.Target))
        || (CanTargetEnemy && BoardState.Data.HasEnemy(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetAlly && BoardState.Data.HasAlly(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetNeutral && BoardState.Data.HasNeutral(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetEnPassent && BoardState.Data.HasNeutral(MoveInfo.Target, MoveInfo.Origin)))
        && BoardState.Data.Board.ContainsPosition(EffectiveOrigin)
        && (EffectiveOrigin == MoveInfo.Origin
        || (EffectiveOriginCanTargetEmpty && BoardState.Data.IsEmpty(EffectiveOrigin))
        || (EffectiveOriginCanTargetEnemy && BoardState.Data.HasEnemy(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetAlly && BoardState.Data.HasAlly(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetNeutral && BoardState.Data.HasNeutral(EffectiveOrigin, MoveInfo.Origin)))
        && !(DisableAfterFirstMove && BoardState.Data.Board.GetPieceAt(MoveInfo.Origin).HasMoved)
        && !(IsBlockable && BoardState.Data.HasPiecesBetween(EffectiveOrigin, MoveInfo.Target));
}