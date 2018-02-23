using Akka.Configuration;
using Akka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using csharp3pc.Actor;
using System.Collections.Immutable;
using csharp3pc.Messages;

namespace csharp3pc
{
  class Program
  {
    static void Main(string[] args)
    {
      var system = ActorSystem.Create("3pc");
      var coordinator = system.ActorOf<CoordinatorActor>("Coordinator");
      var cohorts = Enumerable
        .Range(0, 3)
        .Select(id => system.ActorOf<CohortActor>($"Coohort{id}"))
        .ToImmutableList();

      coordinator.Tell(new CoordinatorInitMsg(cohorts));
      coordinator.Tell(new StartTransactionMsg());

      Console.ReadKey();
    }
  }
}
