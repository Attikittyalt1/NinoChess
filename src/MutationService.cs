using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class MutationService()
{
    private List<IBoardStateMutation> _inverseBoardMutations = [];
    private readonly Stack<List<IBoardStateMutation>> _undoStack = [];
    private readonly Stack<List<IBoardStateMutation>> _redoStack = [];

    public void Execute<T>(T mutation, bool addToUndoStack = true)
        where T : IBoardStateMutation
    {
        if (addToUndoStack)
        {
            _inverseBoardMutations.Add(mutation.GetInverse());
        }

        mutation.Execute();
    }

    public void Finish()
    {
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
            Execute(mutation);
        }

        _redoStack.Push(_inverseBoardMutations);
        _inverseBoardMutations = [];
    }

    public bool CanUndo() => _undoStack.Count > 0;

    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        foreach (var mutation in Enumerable.Reverse(_redoStack.Pop()))
        {
            Execute(mutation);
        }

        _undoStack.Push(_inverseBoardMutations);
        _inverseBoardMutations = [];
    }

    public bool CanRedo() => _redoStack.Count > 0;
}