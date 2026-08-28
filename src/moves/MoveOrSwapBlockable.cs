namespace NinoChess.Moves;

record MoveOrSwapBlockable(FullBoardState BoardState, MoveInfo MoveInfo) : TypicalMove(BoardState, MoveInfo)
{
    public override RegistryID ID => MoveID.MoveOrSwapBlockable;
    public override bool CanTargetEnemy => true;
    public override bool CanTargetAlly => true;
    public override bool DestroyTarget => false;
}