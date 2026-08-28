namespace NinoChess.Moves;

class RangedAttackBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.RangedAttackUnblockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
    public override bool MovePieces => false;
}