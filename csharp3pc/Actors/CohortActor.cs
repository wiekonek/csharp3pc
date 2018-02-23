using Akka.Actor;
using csharp3pc.Data;
using csharp3pc.Messages;
using System;
using System.Threading.Tasks;

namespace csharp3pc.Actor
{
  public enum CohortState
  {
    Init,
    Waiting,
    Prepared,
    Commited,
    Aborted
  }

  public class CohortActor : FSM<CohortState, IData>
  {
    public CohortActor()
    {
      StartWith(CohortState.Init, Empty.Obj);

      When(CohortState.Init, state =>
      {
        if (state.FsmEvent is CanCommitMsg)
        {
          Log("received CanCommitMsg, sending YesMsg");
          Sender.Tell(new YesMsg());
          return GoTo(CohortState.Waiting);
        }
        return null;
      });

      When(CohortState.Waiting, state =>
      {
        
        if (state.FsmEvent is PreCommitMsg)
        {
          //if (Self.Path.Name == "Coohort0")
          //{
          //  Task.Delay(TimeSpan.FromSeconds(15)).Wait();
          //}
          Log("received PreCommitMsg");
          Sender.Tell(new AckMsg());
          return GoTo(CohortState.Prepared);
        }

        if (state.FsmEvent is AbortMsg || state.FsmEvent is StateTimeout)
        {
          Log("received AbortMsg || StateTimeout");
          return GoTo(CohortState.Aborted);
        }

        return null;
      }, TimeSpan.FromSeconds(10));

      When(CohortState.Prepared, state =>
      {
        if (state.FsmEvent is DoCommitMsg || state.FsmEvent is StateTimeout)
        {
          Log("received DoCommitMsg || StateTimeout");
          return GoTo(CohortState.Commited);
        }

        if (state.FsmEvent is AbortMsg)
        {
          Log("received AbortMsg");
          return GoTo(CohortState.Aborted);
        }
        return null;
      }, TimeSpan.FromSeconds(10));

      When(CohortState.Commited, _ =>
      {
        Log("Already commited");
        return Stay();
      });

      When(CohortState.Aborted, _ =>
      {
        Log("Already aborted");
        return Stay();
      });

      WhenUnhandled(state =>
      {
        Log($"Unexpected event: {state.FsmEvent}");
        return Stay();
      });

      OnTransition((_, next) =>
      {
        Log($">> {next}");
      });
    }

    private void Log(string str)
    {
      Console.WriteLine($"[{Self.Path.Name}:{StateName}] :: {str}");
    }
  }
}
