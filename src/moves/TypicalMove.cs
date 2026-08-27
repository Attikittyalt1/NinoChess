namespace NinoChess.Moves;

abstract record TypicalMove(BoardState Board, Position Origin, Position Target) : Move(Board, Origin, Target)
{
    public virtual bool CanTargetEnemy => false;
    public virtual bool CanTargetAlly => false;
    public virtual bool CanTargetNeutral => false;
    public virtual bool CanTargetEmpty => true;
    public virtual bool IsBlockable => true;
    public virtual bool MovePieces => true;
    public virtual bool DestroyTarget => true;
    public virtual bool DisableAfterFirstMove => false;
    public virtual Position EffectiveOrigin => Origin;
    public virtual bool EffectiveOriginCanTargetEnemy => true;
    public virtual bool EffectiveOriginCanTargetAlly => true;
    public virtual bool EffectiveOriginCanTargetNeutral => true;
    public virtual bool EffectiveOriginCanTargetEmpty => true;

    public override void Execute()
    {
        if (DestroyTarget && Board.HasPieceAt(Target))
        {
            Board.DestroyPieceAt(Target, new(Board.GetPieceAt(Target)));
        }

        if (MovePieces)
        {
            Board.SwapPieceLocations(EffectiveOrigin, Target, new(Origin, Target));
        }
    }

    public override bool IsValid() => (
           (CanTargetEmpty && Board.IsEmpty(Target))
        || (CanTargetEnemy && Board.HasEnemy(Target, Origin))
        || (CanTargetAlly && Board.HasAlly(Target, Origin))
        || (CanTargetNeutral && Board.HasNeutral(Target, Origin)))
        && Board.ContainsPosition(EffectiveOrigin)
        && (EffectiveOrigin == Origin
        || (EffectiveOriginCanTargetEmpty && Board.IsEmpty(EffectiveOrigin))
        || (EffectiveOriginCanTargetEnemy && Board.HasEnemy(EffectiveOrigin, Origin))
        || (EffectiveOriginCanTargetAlly && Board.HasAlly(EffectiveOrigin, Origin))
        || (EffectiveOriginCanTargetNeutral && Board.HasNeutral(EffectiveOrigin, Origin)))
        && !(DisableAfterFirstMove && Board.GetPieceAt(Origin).HasMoved)
        && !(IsBlockable && Board.HasPiecesBetween(EffectiveOrigin, Target));
}