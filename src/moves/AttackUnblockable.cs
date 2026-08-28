namespace NinoChess.Moves;

class AttackUnblockable : TypicalMove
{
    public override RegistryID ID => MoveID.AttackUnblockable;
    public override bool IsBlockable => false;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}