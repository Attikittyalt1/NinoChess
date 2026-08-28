namespace NinoChess.Moves;

record MoveOrAttackUnblockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrAttackUnblockable;
    public override bool CanTargetEnemy => true;
    public override bool IsBlockable => false;
}