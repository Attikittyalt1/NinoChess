namespace NinoChess.Moves;

record AttackUnblockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.AttackUnblockable;
    public override bool IsBlockable => false;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}