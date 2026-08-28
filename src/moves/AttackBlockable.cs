namespace NinoChess.Moves;

record AttackBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.AttackBlockable;
    public override bool CanTargetEmpty => false;
    public override bool CanTargetEnemy => true;
}