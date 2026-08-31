using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class TurnManager(BoardStateData boardState, MutationService mutationService, EventService eventService)
{
    public int Turn => boardState.Turn;
    public int PlayerCount 
    { 
        get => boardState.PlayerCount; 
        set => boardState.PlayerCount = value;
    }

    public int CurrentPlayer => boardState.CurrentPlayer;

    public void Do(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute(mutationService, eventService);

        mutationService.Finish();

        boardState.Turn++;
    }

    public void Undo()
    {
        mutationService.Undo();

        boardState.Turn--;
    }

    public bool CanUndo() => mutationService.CanUndo();

    public void Redo()
    {
        mutationService.Redo();

        boardState.Turn++;
    }

    public bool CanRedo() => mutationService.CanRedo();

    public bool IsValid(MoveInfo info) => 
          boardState.Board.TryGetPieceAt(info.Origin, out var piece)
        && piece.Allegience == boardState.CurrentAllegience
        && boardState.IsValidMove(info);

    public IEnumerable<Move> GetValidMovesFrom(Position origin) => boardState.GetValidMovesFrom(origin);

    public IEnumerable<Move> GetValidMovesTo(Position target) => boardState.GetValidMovesTo(target);
}