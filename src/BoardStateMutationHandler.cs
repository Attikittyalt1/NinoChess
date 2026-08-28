using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class BoardStateMutationHandler(BoardStateData boardState)
{
    private List<IBoardStateMutation> _inverseBoardMutations = [];
    private readonly Stack<List<IBoardStateMutation>> _undoStack = [];
    private readonly Stack<List<IBoardStateMutation>> _redoStack = [];

    public EventService MutationEvents = new();


    public void Execute<T>(T mutation, bool addToUndoStack = true)
        where T : EventArgs, IBoardStateMutation
    {
        if (addToUndoStack)
        {
            _inverseBoardMutations.Add(mutation.GetInverse());
        }

        mutation.Execute();

        MutationEvents.Get<T>()?.Invoke(this, (T) mutation.GetEventArgs());
    }

    public void Execute(BoardStateEvent mutation, bool addToUndoStack = true)
    {
        if (addToUndoStack)
        {
            _inverseBoardMutations.Add(mutation.GetInverse());
        }

        mutation.Execute();

        // not ideal. change this
        mutation.InvokeOnto(MutationEvents.Get(mutation.GetType()));
    }

    public void Do(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute();

        _undoStack.Push(_inverseBoardMutations);
        _inverseBoardMutations = [];

        _redoStack.Clear();
    }

    public void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        foreach (var mutation in Enumerable.Reverse(_undoStack.Pop()))
        {
            Execute((BoardStateEvent)mutation);
        }

        _redoStack.Push(_inverseBoardMutations);
        _inverseBoardMutations = [];
    }
    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        foreach (var mutation in Enumerable.Reverse(_redoStack.Pop()))
        {
            Execute((BoardStateEvent)mutation);
        }

        _undoStack.Push(_inverseBoardMutations);
        _inverseBoardMutations = [];
    }
}