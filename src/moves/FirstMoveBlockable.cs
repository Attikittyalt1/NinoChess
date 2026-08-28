namespace NinoChess.Moves;

record FirstMoveBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.FirstMoveBlockable;
    public override bool DisableAfterFirstMove => true;
}