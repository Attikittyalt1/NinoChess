namespace NinoChess.Moves;

record RangedAttackBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.RangedAttackUnblockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
    public override bool MovePieces => false;
}