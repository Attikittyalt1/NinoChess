namespace NinoChess.Moves;

record FirstMoveBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.FirstMoveBlockable;
    public override bool DisableAfterFirstMove => true;
}