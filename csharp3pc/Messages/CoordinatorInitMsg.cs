using Akka.Actor;
using System.Collections.Immutable;

namespace csharp3pc.Messages
{
  public class CoordinatorInitMsg : IMessage
  {
    public CoordinatorInitMsg(ImmutableList<IActorRef> immutableCohortsList)
    {
      Cohorts = immutableCohortsList;
    }

    public readonly ImmutableList<IActorRef> Cohorts;
  }
}
