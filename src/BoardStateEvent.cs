using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public abstract class BoardStateEvent(FullBoardState currentBoardState) : EventArgs, IBoardStateMutation
{
    public void InvokeOnto(object? handler)
    {
        handler?.GetType().GetMethod("Invoke").Invoke(this, [this, GetEventArgs()]);
    }

    public abstract void Execute();

    public EventArgs GetEventArgs() => this;

    public abstract IBoardStateMutation GetInverse();
}

public interface IBoardStateMutation
{
    public void Execute();

    public EventArgs GetEventArgs();

    public IBoardStateMutation GetInverse();
}
