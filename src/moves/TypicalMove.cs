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

    public override void Execute()
    {
        if (DestroyTarget && Board.HasPieceAt(Target))
        {
            Board.DestroyPieceAt(Target, new());
        }

        if (MovePieces)
        {
            Board.SwapPieceLocations(Origin, Target, new());
        }
    }

    public override bool IsValid() => (
           (CanTargetEmpty && Board.IsEmpty(Target))
        || (CanTargetEnemy && Board.HasEnemy(Target, Origin))
        || (CanTargetAlly && Board.HasAlly(Target, Origin))
        || (CanTargetNeutral && Board.HasNeutral(Target, Origin)))
        && !(DisableAfterFirstMove && Board.GetPieceAt(Origin).HasMoved)
        && !(IsBlockable && Board.HasPiecesBetween(Origin, Target));
}