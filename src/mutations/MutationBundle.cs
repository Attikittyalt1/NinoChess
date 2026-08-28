using System.Collections.Generic;
using System.Linq;

namespace NinoChess.Mutations;

public class MutationBundle : IBoardStateMutation
{
    public required IEnumerable<IBoardStateMutation> Mutations { get; init; }

    public void Execute()
    {
        foreach (var mutation in Mutations)
        {
            mutation.Execute();
        }
    }

    public IBoardStateMutation GetInverse() => new MutationBundle 
    { 
        Mutations = Enumerable.Reverse(Mutations.Select(mutation => mutation.GetInverse())) 
    };
}