using MathNet.Numerics;
using NinoChess.Events;
using NinoChess.Mutations;
using NinoChess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class TurnManager(BoardStateData boardState, MutationService mutationService, EventService eventService)
{
    public int Turn => boardState.Turn;
    public int PlayerCount 
    { 
        get => boardState.PlayerCount; 
        set => boardState.PlayerCount = value;
    }

    public int CurrentPlayer => boardState.CurrentPlayer;

    public void Do(MoveInfo moveInfo)
    {
        boardState.Board.GetPieceAt(moveInfo.Origin).GetBestValidMoveAt(moveInfo.Target).Execute(mutationService, eventService);

        mutationService.Finish();

        boardState.Turn++;
    }

    public void Undo()
    {
        mutationService.Undo();

        boardState.Turn--;
    }

    public bool CanUndo() => mutationService.CanUndo();

    public void Redo()
    {
        mutationService.Redo();

        boardState.Turn++;
    }

    public bool CanRedo() => mutationService.CanRedo();

    public bool IsValid(MoveInfo info) => 
          boardState.Board.TryGetPieceAt(info.Origin, out var piece)
        && piece.Allegience == boardState.CurrentAllegience
        && boardState.IsValidMove(info);

    public IEnumerable<Move> GetValidMovesFrom(Position origin) => boardState.GetValidMovesFrom(origin);

    public IEnumerable<Move> GetValidMovesTo(Position target) => boardState.GetValidMovesTo(target);

    public void SetupBoard()
    {
        Create(new Pawn { Position = new(0, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(1, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(2, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(3, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(4, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(5, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(6, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(7, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(0, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(1, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(2, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Moog { Position = new(3, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new King { Position = new(4, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(5, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(6, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(7, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });

        Create(new Pawn { Position = new(0, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(1, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(2, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(3, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(4, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(5, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(6, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(7, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(0, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(1, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(2, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Moog { Position = new(3, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new King { Position = new(4, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(5, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(6, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(7, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });

        void Create(Piece piece)
        {
            var sender = boardState;
            var args = new Event_Create
            {
                MutationService = mutationService,
                Piece = piece
            };

            piece.OnCreate(sender, args);

            new Mutation_Create { Board = boardState.Board, Piece = piece }.Execute();

            eventService.Get<Event_Create>()?.Invoke(sender, args);
        }
    }

}