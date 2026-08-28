namespace NinoChess.Moves;

class MoveOrSwapBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.MoveOrSwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
}