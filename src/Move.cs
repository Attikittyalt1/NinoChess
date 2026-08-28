namespace NinoChess;

public abstract record Move(BoardState Board, MoveInfo MoveInfo) : ICanMove, IHasMoveID
{
    public abstract void Execute();
    public abstract bool IsValid();
    public abstract RegistryID ID { get; }
}

public readonly record struct MoveInfo(Position Origin, Position Target);

public interface ICanMove
{
    public void Execute();
    public bool IsValid();
}

public interface IHasMoveID
{
    public RegistryID ID { get; }
}