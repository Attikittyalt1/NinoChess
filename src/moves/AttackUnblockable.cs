namespace NinoChess.Moves;

record AttackUnblockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.AttackUnblockable;
    public override bool IsBlockable => false;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}