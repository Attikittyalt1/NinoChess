using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class BoardStateEventArgs : EventArgs
{
    public required MutationService MutationService { get; init; }
}