namespace NinoChess.Moves;

class MoveOrAttackUnblockable : TypicalMove
{
    public override RegistryID ID => MoveID.MoveOrAttackUnblockable;
    public override bool CanTargetEnemy => true;
    public override bool IsBlockable => false;
}