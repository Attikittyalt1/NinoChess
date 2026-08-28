using MathNet.Numerics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.Design;

namespace NinoChess;

public class BoardStateData(IBoard board)
{
    public IBoard Board => board;

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

    public bool HasPiecesBetween(Position p1, Position p2) => Position.SatisfiesBetween(p1, p2, pos => board.ContainsPosition(pos) && HasSolid(pos));

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