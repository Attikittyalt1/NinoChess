namespace NinoChess.Moves;

record AttackUnblockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.AttackUnblockable;
    public override bool IsBlockable => false;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}