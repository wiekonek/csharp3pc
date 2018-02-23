using Akka.Actor;
using System.Collections.Immutable;

namespace csharp3pc.Data
{
  public class CoordinatorData : IData
  {
    public readonly ImmutableList<IActorRef> Cohorts;
    public readonly int AgreeReplies;
    public readonly int AckReplies;

    public CoordinatorData(ImmutableList<IActorRef> cohorts = null, int agreeReplies = 0, int ackReplies = 0)
    {
      Cohorts = cohorts ?? ImmutableList<IActorRef>.Empty;
      AgreeReplies = agreeReplies;
      AckReplies = ackReplies;
    }

    public CoordinatorData Copy(ImmutableList<IActorRef> cohorts = null, int? agreeReplies = null, int? ackReplies = null)
    {
      return new CoordinatorData(
        cohorts ?? Cohorts,
        agreeReplies ?? AgreeReplies, 
        ackReplies ?? AckReplies);
    }
  }
}
