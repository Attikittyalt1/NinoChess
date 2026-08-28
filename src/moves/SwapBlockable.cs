namespace NinoChess.Moves;

class SwapBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.SwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
    public override bool CanTargetEmpty => false;
}