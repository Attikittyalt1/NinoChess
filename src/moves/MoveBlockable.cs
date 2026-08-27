namespace NinoChess.Moves;

record MoveBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.MoveBlockable;
}