using System;

namespace NinoChess.Events;

public class Event_Destroy : BoardStateEventArgs
{
    public required Position Position { get; init; }
}