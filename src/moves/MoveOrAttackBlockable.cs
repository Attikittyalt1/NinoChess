namespace NinoChess.Moves;

record MoveOrAttackBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.MoveOrAttackBlockable;
    public override bool CanTargetEnemy => true;
}