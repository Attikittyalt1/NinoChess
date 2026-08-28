using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinoChess;

public class BoardState(IBoard board)
{
    public bool ContainsPosition(Position p) => board.ContainsPosition(p);
    public bool HasPieceAt(Position p) => board.HasPieceAt(p);
    public Piece GetPieceAt(Position p) => board.GetPieceAt(p);
    public Piece? TryGetPieceAt(Position p) => board.HasPieceAt(p) ? board.GetPieceAt(p) : null;

    private List<EventArgs> _recentBoardUpdates = [];
    private List<EventArgs> _newBoardUpdates = [];

    public EventHandler? OnDestroy;
    public class PieceDestructionInfo(Piece piece) : EventArgs
    {
        public Piece Piece => piece;
    }
    public void DestroyPieceAt(Position p, PieceDestructionInfo info)
    {
        board.RemovePieceAt(p);

        _newBoardUpdates.Add(info);

        OnDestroy?.Invoke(this, info);
        info.Piece.OnDestroy(info);
    }

    public EventHandler? OnCreate;
    public class PieceCreationInfo(Piece piece) : EventArgs
    {
        public Piece Piece => piece;
    }
    public void CreatePieceAt(Position p, PieceCreationInfo info)
    {
        board.AddPieceAt(p, info.Piece);

        _newBoardUpdates.Add(info);

        OnCreate?.Invoke(this, info);
        info.Piece.OnCreate(info);
    }

    public EventHandler? OnSwap;
    public class PieceSwapInfo(Position p1, Position p2) : EventArgs
    {
        public Position P1 => p1;
        public Position P2 => p2;
    }
    public void SwapPieceLocations(Position p1, Position p2, PieceSwapInfo info)
    {
        TryGetPieceAt(p1)?.Position = p2;
        TryGetPieceAt(p2)?.Position = p1;
        board.SwapPiecesAt(p1, p2);

        _newBoardUpdates.Add(info);

        OnSwap?.Invoke(this, info);
        TryGetPieceAt(p1)?.OnSwap(info);
        TryGetPieceAt(p2)?.OnSwap(info);
    }

    public ReadOnlyCollection<EventArgs> RecentBoardUpdates => _recentBoardUpdates.AsReadOnly();

    public void ExecuteMove(Position p1, Position p2)
    {
        board.GetPieceAt(p1).GetBestValidMoveAt(p2).Execute();

        _recentBoardUpdates = _newBoardUpdates;
        _newBoardUpdates = [];
    }

    public bool IsValidMove(MoveInfo info)
    {
        return board.GetPieceAt(info.Origin).HasValidMoveAt(info.Target);
    }

    public IEnumerable<Move> GetValidMovesFrom(Position origin)
    {
        var piece = board.GetPieceAt(origin);

        return Position.Range(origin - Position.Unit * piece.MaxMoveRange, Position.Unit * (1 + 2 * piece.MaxMoveRange)).Where(pos => board.ContainsPosition(pos) && piece.HasValidMoveAt(pos)).Select(piece.GetBestValidMoveAt);
    }

    public IEnumerable<Move> GetValidMovesTo(Position target)
    {
        throw new NotImplementedException();
    }

    public bool HasPiecesBetween(Position p1, Position p2)
    {
        var diff = p2 - p1;
        var GCD = (int)Euclid.GreatestCommonDivisor(diff.X, diff.Y);

        var step = diff / GCD;
        var current = p1;

        for (int i = 1; i < GCD; i++)
        {
            current += step;
            if (board.ContainsPosition(current) && HasSolid(current))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnemy(Position target, Position reference)
    {
        var team1 = board.GetPieceAt(reference).Allegience;
        var team2 = board.GetPieceAt(target).Allegience;

        return team1 != Allegience.None && team2 != Allegience.None && team1 != team2;
    }
    public bool IsAlly(Position target, Position reference)
    {
        var team1 = board.GetPieceAt(reference).Allegience;
        var team2 = board.GetPieceAt(target).Allegience;

        return team1 != Allegience.None && team1 == team2;
    }
    public bool IsNeutral(Position target, Position reference)
    {
        var team1 = board.GetPieceAt(reference).Allegience;
        var team2 = board.GetPieceAt(target).Allegience;

        return team1 == Allegience.None || team2 == Allegience.None;
    }
    public bool IsSolid(Position p) => board.GetPieceAt(p).IsSolid;

    public bool IsEmpty(Position target) => !board.HasPieceAt(target);
    public bool HasEnemy(Position target, Position reference) => board.HasPieceAt(target) && IsEnemy(target, reference);
    public bool HasAlly(Position target, Position reference) => board.HasPieceAt(target) && IsAlly(target, reference);
    public bool HasNeutral(Position target, Position reference) => board.HasPieceAt(target) && IsNeutral(target, reference);
    public bool HasSolid(Position p) => board.HasPieceAt(p) && IsSolid(p);
}