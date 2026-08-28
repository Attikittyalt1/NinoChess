namespace NinoChess.Moves;

class MoveOrAttackBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.MoveOrAttackBlockable;
    public override bool CanTargetEnemy => true;
}