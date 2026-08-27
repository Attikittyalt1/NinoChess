namespace NinoChess.Moves;

record RangedAttackBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.RangedAttackUnblockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
    public override bool MovePieces => false;
}