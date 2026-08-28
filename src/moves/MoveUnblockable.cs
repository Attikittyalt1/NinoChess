namespace NinoChess.Moves;

class MoveUnblockable : TypicalMove
{
    public override RegistryID ID => MoveID.MoveUnblockable;
    public override bool IsBlockable => false;
}