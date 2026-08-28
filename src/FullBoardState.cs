using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class FullBoardState
{
    public FullBoardState(IBoard board)
    {
        Data = new(board);
        MutationHandler = new(Data);
    }

    public BoardStateData Data { get; }
    public BoardStateMutationHandler MutationHandler { get; }
}