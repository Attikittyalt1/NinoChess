namespace NinoChess.Moves;

record MoveUnblockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.MoveUnblockable;
    public override bool IsBlockable => false;
}