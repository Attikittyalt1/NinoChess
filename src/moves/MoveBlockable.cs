namespace NinoChess.Moves;

record MoveBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveBlockable;
}