using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class BoardStateMutationHandler(BoardStateData boardState)
{
    private List<IBoardStateMutation> _previousBoardMutations = [];
    private List<IBoardStateMutation> _boardMutations = [];

    public ReadOnlyCollection<IBoardStateMutation> RecentBoardMutations => _previousBoardMutations.AsReadOnly();
    public EventService MutationEvents = new();


    public void Execute<T>(T mutation)
        where T : EventArgs, IBoardStateMutation
    {
        mutation.Execute();

        _boardMutations.Add(mutation);

        MutationEvents.Get<T>()?.Invoke(this, (T) mutation.GetEventArgs());
    }

    public void Execute(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute();

        _previousBoardMutations = _boardMutations;
        _boardMutations = [];
    }
}