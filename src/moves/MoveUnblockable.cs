namespace NinoChess.Moves;

record MoveUnblockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveUnblockable;
    public override bool IsBlockable => false;
}