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

    public void Execute<TEventArgs>(IBoardStateMutation mutation)
        where TEventArgs : EventArgs
    {
        mutation.Execute(boardState);
        _boardMutations.Add(mutation);

        boardState.MutationEvents.Get<TEventArgs>()?.Invoke(this, (TEventArgs) mutation.GetEventArgs());
    }

    public void Execute(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute();

        _previousBoardMutations = _boardMutations;
        _boardMutations = [];
    }
}