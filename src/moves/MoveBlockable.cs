namespace NinoChess.Moves;

record MoveBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveBlockable;
}