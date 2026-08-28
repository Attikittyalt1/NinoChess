using System;

namespace NinoChess.Events;

public class Event_Swap : BoardStateEventArgs
{
    public required (Position, Position) Positions { get; init; }
}