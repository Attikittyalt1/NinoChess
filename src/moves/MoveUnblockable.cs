namespace NinoChess.Moves;

record MoveUnblockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveUnblockable;
    public override bool IsBlockable => false;
}