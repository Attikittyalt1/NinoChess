namespace NinoChess.Moves;

record AttackBlockable(BoardState Board, Position Origin, Position Target) : TypicalMove(Board, Origin, Target)
{
    public override RegistryID ID => MoveID.AttackBlockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}