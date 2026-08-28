namespace NinoChess.Moves;

record AttackBlockable(BoardState Board, MoveInfo MoveInfo) : TypicalMove(Board, MoveInfo)
{
    public override RegistryID ID => MoveID.AttackBlockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}