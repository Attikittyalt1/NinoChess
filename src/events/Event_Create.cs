using System;

namespace NinoChess.Events;

public class Event_Create : BoardStateEventArgs
{
    public required Piece Piece { get; init; }
}