using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public interface IBoardStateMutation
{
    public void Execute(BoardStateData data);

    public EventArgs GetEventArgs();

    public IBoardStateMutation GetInverse();
}