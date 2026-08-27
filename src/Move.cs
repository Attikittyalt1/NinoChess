namespace NinoChess;


abstract record Move(BoardState Board, Position Origin, Position Target) : ICanMove, IHasMoveID
{
    public abstract void Execute();
    public abstract bool IsValid();
    public abstract RegistryID ID { get; }
}


interface ICanMove
{
    public void Execute();
    public bool IsValid();
}

interface IHasMoveID
{
    public RegistryID ID { get; }
}