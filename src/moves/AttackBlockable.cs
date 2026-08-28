namespace NinoChess.Moves;

class AttackBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.AttackBlockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}