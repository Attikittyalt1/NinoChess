using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace NinoChess;

class BoardState(IBoard board)
{
    public bool ContainsPosition(Position p) => board.ContainsPosition(p);
    public bool HasPieceAt(Position p) => board.HasPieceAt(p);
    public Piece GetPieceAt(Position p) => board.GetPieceAt(p);

    public record BoardStateUpdateInfo;

    private List<BoardStateUpdateInfo> _recentBoardUpdates = [];
    private List<BoardStateUpdateInfo> _newBoardUpdates = [];
    public record PieceDestructionInfo(Piece Piece) : BoardStateUpdateInfo;
    public void DestroyPieceAt(Position p, PieceDestructionInfo info)
    {
        board.RemovePieceAt(p);

        _newBoardUpdates.Add(info);
    }

    public record PieceCreationInfo(Piece Piece) : BoardStateUpdateInfo;
    public void CreatePieceAt(Position p, PieceCreationInfo info)
    {
        board.AddPieceAt(p, info.Piece);

        _newBoardUpdates.Add(info);
    }

    public record PieceSwapInfo(Position p1, Position p2) : BoardStateUpdateInfo;
    public void SwapPieceLocations(Position p1, Position p2, PieceSwapInfo info)
    {
        if (board.HasPieceAt(p1)) board.GetPieceAt(p1).Position = p2;
        if (board.HasPieceAt(p2)) board.GetPieceAt(p2).Position = p1;
        board.SwapPiecesAt(p1, p2);

        _newBoardUpdates.Add(info);
    }

    public ReadOnlyCollection<BoardStateUpdateInfo> RecentBoardUpdates => _recentBoardUpdates.AsReadOnly();

    public void ExecuteMove(Position p1, Position p2)
    {
        board.GetPieceAt(p1).GetBestValidMoveAt(p2).Execute();

        _recentBoardUpdates = _newBoardUpdates;
        _newBoardUpdates = [];
    }

    public bool IsValidMove(Position p1, Position p2)
    {
        return board.GetPieceAt(p1).HasValidMoveAt(p2);
    }

    public IEnumerable<Move> GetValidMovesFrom(Position p)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Move> GetValidMovesTo(Position p)
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

        return team1 != Allegience.Neutral && team2 != Allegience.Neutral && team1 != team2;
    }
    public bool IsAlly(Position target, Position reference)
    {
        var team1 = board.GetPieceAt(reference).Allegience;
        var team2 = board.GetPieceAt(target).Allegience;

        return team1 != Allegience.Neutral && team1 == team2;
    }
    public bool IsNeutral(Position target, Position reference)
    {
        var team1 = board.GetPieceAt(reference).Allegience;
        var team2 = board.GetPieceAt(target).Allegience;

        return team1 == Allegience.Neutral || team2 == Allegience.Neutral;
    }
    public bool IsSolid(Position p) => board.GetPieceAt(p).IsSolid;

    public bool IsEmpty(Position target) => !board.HasPieceAt(target);
    public bool HasEnemy(Position target, Position reference) => board.HasPieceAt(target) && IsEnemy(target, reference);
    public bool HasAlly(Position target, Position reference) => board.HasPieceAt(target) && IsAlly(target, reference);
    public bool HasNeutral(Position target, Position reference) => board.HasPieceAt(target) && IsNeutral(target, reference);
    public bool HasSolid(Position p) => board.HasPieceAt(p) && IsSolid(p);
}