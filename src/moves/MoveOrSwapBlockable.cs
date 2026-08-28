namespace NinoChess.Moves;

record MoveOrSwapBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrSwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
}