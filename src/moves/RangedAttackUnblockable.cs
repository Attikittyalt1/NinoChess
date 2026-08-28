namespace NinoChess.Moves;

record RangedAttackBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.RangedAttackUnblockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
    public override bool MovePieces => false;
}