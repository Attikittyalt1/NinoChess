namespace NinoChess.Moves;

record SwapBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.SwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
    public override bool CanTargetEmpty => false;
}