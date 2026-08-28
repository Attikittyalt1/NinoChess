namespace NinoChess.Moves;

record MoveOrAttackBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrAttackBlockable;
    public override bool CanTargetEnemy => true;
}