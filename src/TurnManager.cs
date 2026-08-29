using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class TurnManager(BoardStateData boardState, MutationService mutationService, EventService eventService)
{

    public int PlayerCount { get; init; } = 2;

    public int Turn { get; private set; } = 0;

    public Allegience CurrentAllegience => (Allegience) (Turn % PlayerCount + 1);

    public void Do(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute(mutationService, eventService);

        mutationService.Finish();

        Turn++;
    }

    public void Undo()
    {
        mutationService.Undo();

        Turn--;
    }


    public void Redo()
    {
        mutationService.Redo();

        Turn++;
    }

    public bool IsValid(MoveInfo info) => 
          boardState.Board.TryGetPieceAt(info.Origin, out var piece)
        && piece.Allegience == CurrentAllegience
        && boardState.IsValidMove(info);

    public IEnumerable<Move> GetValidMovesFrom(Position origin) => boardState.GetValidMovesFrom(origin);

    public IEnumerable<Move> GetValidMovesTo(Position target) => boardState.GetValidMovesTo(target);

}