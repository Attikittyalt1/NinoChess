namespace NinoChess.Moves;

record MoveOrAttackBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrAttackBlockable;
    public override bool CanTargetEnemy => true;
}