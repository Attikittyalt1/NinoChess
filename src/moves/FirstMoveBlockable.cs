namespace NinoChess.Moves;

class FirstMoveBlockable : TypicalMove
{
    public override RegistryID ID => MoveID.FirstMoveBlockable;
    public override bool DisableAfterFirstMove => true;
}