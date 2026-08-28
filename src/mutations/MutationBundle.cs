using System.Collections.Generic;
using System.Linq;

namespace NinoChess.Mutations;

public class MutationBundle(FullBoardState currentBoardState, object? sender, IEnumerable<IBoardStateMutation> mutations) : BoardStateEvent(currentBoardState, sender)
{
    public override void Execute()
    {
        foreach (var mutation in mutations)
        {
            mutation.Execute();
        }
    }

    public override IBoardStateMutation GetInverse() => new MutationBundle(currentBoardState, sender, Enumerable.Reverse(mutations.Select(mutation => mutation.GetInverse())));
}