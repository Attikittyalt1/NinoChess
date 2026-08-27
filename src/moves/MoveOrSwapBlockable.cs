namespace NinoChess.Moves;

record MoveOrSwapBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.MoveOrSwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
}