namespace NinoChess.Moves;

abstract record TypicalMove(BoardState Board, MoveInfo MoveInfo) : Move(Board, MoveInfo)
{
    public virtual bool CanTargetEnemy => false;
    public virtual bool CanTargetAlly => false;
    public virtual bool CanTargetNeutral => false;
    public virtual bool CanTargetEmpty => true;
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
        if (DestroyTarget && Board.HasPieceAt(MoveInfo.Target))
        {
            Board.DestroyPieceAt(MoveInfo.Target, new(Board.GetPieceAt(MoveInfo.Target)));
        }

        if (MovePieces)
        {
            Board.SwapPieceLocations(EffectiveOrigin, MoveInfo.Target, new(MoveInfo.Origin, MoveInfo.Target));
        }
    }

    public override bool IsValid() => (
           (CanTargetEmpty && Board.IsEmpty(MoveInfo.Target))
        || (CanTargetEnemy && Board.HasEnemy(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetAlly && Board.HasAlly(MoveInfo.Target, MoveInfo.Origin))
        || (CanTargetNeutral && Board.HasNeutral(MoveInfo.Target, MoveInfo.Origin)))
        && Board.ContainsPosition(EffectiveOrigin)
        && (EffectiveOrigin == MoveInfo.Origin
        || (EffectiveOriginCanTargetEmpty && Board.IsEmpty(EffectiveOrigin))
        || (EffectiveOriginCanTargetEnemy && Board.HasEnemy(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetAlly && Board.HasAlly(EffectiveOrigin, MoveInfo.Origin))
        || (EffectiveOriginCanTargetNeutral && Board.HasNeutral(EffectiveOrigin, MoveInfo.Origin)))
        && !(DisableAfterFirstMove && Board.GetPieceAt(MoveInfo.Origin).HasMoved)
        && !(IsBlockable && Board.HasPiecesBetween(EffectiveOrigin, MoveInfo.Target));
}