namespace NinoChess.Moves;

record MoveOrAttackUnblockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.MoveOrAttackUnblockable;
    public override bool CanTargetEnemy => true;
    public override bool IsBlockable => false;
}