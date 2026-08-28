namespace NinoChess;

public abstract class Move : ICanMove, IHasMoveID
{
    public required BoardStateData BoardState { get; init; }
    public required MoveInfo MoveInfo { get; init; }

    public abstract void Execute(MutationService mutationService, EventService eventService);
    public abstract bool IsValid();
    public abstract RegistryID ID { get; }
}

public readonly record struct MoveInfo(Position Origin, Position Target);

public interface ICanMove
{
    public void Execute(MutationService mutationService, EventService eventService);
    public bool IsValid();
}

public interface IHasMoveID
{
    public RegistryID ID { get; }
}