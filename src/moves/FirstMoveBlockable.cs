namespace NinoChess.Moves;

record FirstMoveBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.FirstMoveBlockable;
    public override bool DisableAfterFirstMove => true;
}