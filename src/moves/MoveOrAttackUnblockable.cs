namespace NinoChess.Moves;

record MoveOrAttackUnblockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrAttackUnblockable;
    public override bool CanTargetEnemy => true;
    public override bool IsBlockable => false;
}