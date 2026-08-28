using MathNet.Numerics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace NinoChess;

public readonly record struct Position(int X, int Y)
{
    public static Position Zero => new(0, 0);
    public static Position Unit => new(1, 1);
    public static Position MaxValue => new(int.MaxValue, int.MaxValue);
    public static Position MinValue => new(int.MinValue, int.MinValue);

    public static Position N => new(0, 1);
    public static Position NE => new(1, 1);
    public static Position E => new(1, 0);
    public static Position SE => new(1, -1);
    public static Position S => new(0, -1);
    public static Position SW => new(-1, -1);
    public static Position W => new(-1, 0);
    public static Position NW => new(-1, 1);

    public static List<Position> Directions => [N, NE, E, SE, S, SW, W, NW];
    public static List<Position> OrthogonalDirections => [N, E, S, W];
    public static List<Position> DiagonalDirections => [NE, SE, SW, NW];

    public static implicit operator Vector2(Position p) => new(p.X, p.Y);
    public static explicit operator Position(Vector2 p) => new((int)p.X, (int)p.Y);
    public static implicit operator Point(Position p) => new(p.X, p.Y);
    public static implicit operator Position(Point p) => new(p.X, p.Y);
    public static Position operator +(Position p) => p;
    public static Position operator -(Position p) => new(-p.X, -p.Y);
    public static Position operator +(Position p1, Position p2) => new(p1.X + p2.X, p1.Y + p2.Y);
    public static Position operator -(Position p1, Position p2) => new(p1.X - p2.X, p1.Y - p2.Y);
    public static Position operator *(Position p, int c) => new(p.X * c, p.Y * c);
    public static Position operator *(int c, Position p) => p * c;
    public static Position operator /(Position p, int c) => new(p.X / c, p.Y / c);
    public static Position operator %(Position p, int c) => new(p.X % c, p.Y % c);
    public static Position operator *(Transformation t, Position p) => new(p.X * t.A + p.Y * t.B, p.X * t.C + p.Y * t.D);
    public static Transformation operator /(Position p1, Position p2)
    {
        var p = p1.RawDivision(p2) / p2.MagnitudeSquared; // note that this division is lossy

        return new(p.X, -p.Y, p.Y, p.X);
    }
    public int MagnitudeSquared => X * X + Y * Y;

    public bool IsBetween(Position p1, Position p2, bool inclusiveLower = true, bool inclusiveUpper = true) => X.IsBetween(p1.X, p2.X, inclusiveLower, inclusiveUpper) && Y.IsBetween(p1.Y, p2.Y, inclusiveLower, inclusiveUpper);

    public Position Clamp(Position p1, Position p2) => new(X.Clamp(p1.X, p2.X), Y.Clamp(p1.Y, p2.Y));

    public Position RawDivision(Position p) => new(X * p.X + Y * p.Y, X * p.Y - Y * p.X);
    public bool IsInDirection(Position direction, int min, int max, bool mirror = false, bool includePerpendicular = false)
    {
        if (this == Zero) return false;

        var division = RawDivision(direction);
        var mag = direction.MagnitudeSquared;

        return 
            division.Y == 0 && (mirror ? Math.Abs(division.X) : division.X).IsBetween(min * mag, max * mag) || 
            (includePerpendicular && division.X == 0 && Math.Abs(division.Y).IsBetween(min * mag, max * mag));
    }

    public bool IsInDirection(Position direction, bool mirror = false, bool includePerpendicular = false)
    {
        if (this == Zero || direction == Zero) return this == direction;

        var division = RawDivision(direction);

        return division.Y == 0 && (mirror || division.X > 0) || (includePerpendicular && division.X == 0);
    }

    public Position ProjectOnto(Position direction) => direction * Dot(this, direction) / direction.MagnitudeSquared;
    public Position ReflectAcross(Position direction) => 2 * ProjectOnto(direction) - this;

    public static IEnumerable<Position> Range(Position start, Position size)
    {
        if (size.X < 0 || size.Y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        if (size.X > 0 && size.Y > 0)
        {
            for (int y = start.Y; y < start.Y + size.Y; y++)
            {
                for (int x = start.X; x < start.X + size.X; x++)
                {
                    yield return new(x, y);
                }
            }
        }
    }

    public static Position MultiplyComponentWise(Position p1, Position p2) => new(p1.X * p2.X, p1.Y * p2.Y);
    public static int Dot(Position p1, Position p2) => p1.X * p2.X + p1.Y * p2.Y;
    public static bool SatisfiesBetween(Position p1, Position p2, Predicate<Position> predicate)
    {
        var diff = p2 - p1;
        var GCD = (int)Euclid.GreatestCommonDivisor(diff.X, diff.Y);

        var step = diff / GCD;
        var current = p1;

        for (int i = 1; i < GCD; i++)
        {
            current += step;
            if (predicate(current))
            {
                return true;
            }
        }

        return false;
    }


    public override string ToString()
    {
        return String.Format("({0},{1})", X, Y);
    }
}

public readonly record struct Transformation(int A, int B, int C, int D)
{
    public static Transformation Zero => new(0, 0, 0, 0);
    public static Transformation Identity => new(1, 0, 0, 1);
    public static Transformation Flip => new(-1, 0, 0, -1);
    public static Transformation Clockwise => new(0, 1, -1, 0);
    public static Transformation CounterClockwise => new(0, -1, 1, 0);

    public static Transformation operator +(Transformation t) => t;
    public static Transformation operator -(Transformation t) => new(-t.A, -t.B, -t.C, -t.D);
    public static Transformation operator +(Transformation t1, Transformation t2) => new(t1.A + t2.A, t1.B + t2.B, t1.C + t2.C, t1.D + t2.D);
    public static Transformation operator -(Transformation t1, Transformation t2) => new(t1.A - t2.A, t1.B - t2.B, t1.C - t2.C, t1.D - t2.D);
    public static Transformation operator *(Transformation t, int c) => new(t.A * c, t.B * c, t.C * c, t.D * c);
    public static Transformation operator *(int c, Transformation t) => t * c;
    public static Transformation operator /(Transformation t, int c) => new(t.A / c, t.B / c, t.C / c, t.D / c);
    public static Transformation operator %(Transformation t, int c) => new(t.A % c, t.B % c, t.C % c, t.D % c);
    public static Transformation operator *(Transformation t1, Transformation t2) => new(
        t1.A * t2.A + t1.B * t2.C, 
        t1.A * t2.B + t1.B * t2.D,
        t1.C * t2.A + t1.D * t2.C,
        t1.C * t2.B + t1.D * t2.D
        );

    public Transformation Transpose => new(A, C, B, D);
    public Transformation Minor => new(D, C, B, A);
    public Transformation Cofactor => new(D, -C, -B, A);
    public Transformation Adjoint => new(D, -B, -C, A);
    public Transformation Inverse => Adjoint / Determinent; // note that this division is lossy
    public int Determinent => A * D - B * C;
}