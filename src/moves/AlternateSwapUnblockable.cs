namespace NinoChess.Moves;

record AlternateSwapUnblockable(BoardState Board, MoveInfo MoveInfo, Position AlternateOrigin) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.AlternateSwapUnblockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool CanTargetEmpty => false;
    public override bool DestroyTarget => false;
    public override bool IsBlockable => false;
    public override Position EffectiveOrigin =>  AlternateOrigin;
}