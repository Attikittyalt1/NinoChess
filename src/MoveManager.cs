using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class MoveManager(BoardStateData boardState, MutationService mutationService, EventService eventService)
{

    public void Do(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute(mutationService, eventService);

        mutationService.Finish();
    }

    public void Undo() => mutationService.Undo();

    public void Redo() => mutationService.Redo();

    public bool IsValidMove(MoveInfo info) => boardState.IsValidMove(info);

    public IEnumerable<Move> GetValidMovesFrom(Position origin) => boardState.GetValidMovesFrom(origin);

    public IEnumerable<Move> GetValidMovesTo(Position target) => boardState.GetValidMovesTo(target);

}