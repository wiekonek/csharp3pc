using Akka.Actor;
using csharp3pc.Data;
using csharp3pc.Messages;
using System;
using System.Threading.Tasks;

namespace csharp3pc.Actor
{
  public enum CoordinatorState
  {
    Uninitialized,
    Initialized,
    Waiting,
    PreCommit,
    Commited,
    Aborted
  }

  public class CoordinatorActor : FSM<CoordinatorState, CoordinatorData>
  {
    public CoordinatorActor()
    {
      StartWith(CoordinatorState.Uninitialized, new CoordinatorData());

      When(CoordinatorState.Uninitialized, state =>
      {
        if (state.FsmEvent is CoordinatorInitMsg)
        {
          Log("received CoordinatorInitMsg");
          var data = state.FsmEvent as CoordinatorInitMsg;
          return GoTo(CoordinatorState.Initialized).Using(state.StateData.Copy(data.Cohorts));
        }
        return null;
      });

      When(CoordinatorState.Initialized, state =>
      {
        if (state.FsmEvent is StartTransactionMsg)
        {
          Log("START TRANSACTION");
          Log("sending CanCommitMsgs");
          foreach (var cohort in state.StateData.Cohorts)
          {
            cohort.Tell(new CanCommitMsg());
          }
          return GoTo(CoordinatorState.Waiting);
        }

        return null;
      });

      When(CoordinatorState.Waiting, state =>
      {
        if (state.FsmEvent is YesMsg)
        {
          Log($"received YesMsg from {Sender.Path.Name}");
          var replCount = state.StateData.AgreeReplies;
          if (replCount + 1 < state.StateData.Cohorts.Count)
          {
            return Stay().Using(state.StateData.Copy(agreeReplies: replCount + 1));
          }
          else if (replCount + 1 == state.StateData.Cohorts.Count)
          {
            Log("received all YesMsgs, sending PreCommitMsgs");
            foreach (var cohort in state.StateData.Cohorts)
            {
              cohort.Tell(new PreCommitMsg());
            }
            return GoTo(CoordinatorState.PreCommit).Using(state.StateData.Copy(agreeReplies: replCount + 1));
          }
        }

        if (state.FsmEvent is AbortMsg || state.FsmEvent is StateTimeout)
        {
          Log("received AbortMsg || StateTimeout, sending AbortMsgs");
          foreach (var cohort in state.StateData.Cohorts)
          {
            cohort.Tell(new AbortMsg());
          }
          return GoTo(CoordinatorState.Aborted);
        }

        return null;
      }, TimeSpan.FromSeconds(10));


      When(CoordinatorState.PreCommit, state =>
      {
        if (state.FsmEvent is AckMsg)
        {
          Log($"received AckMsg from {Sender.Path.Name}");
          var ackCount = state.StateData.AckReplies;
          if (ackCount + 1 < state.StateData.Cohorts.Count)
          {
            return Stay().Using(state.StateData.Copy(ackReplies: ackCount + 1));
          }
          else if (ackCount + 1 == state.StateData.Cohorts.Count)
          {
            //Task.Delay(TimeSpan.FromSeconds(15)).Wait();
            Log("received all AckMsgs, sending DoCommitMsgs");
            foreach (var cohort in state.StateData.Cohorts)
            {
              cohort.Tell(new DoCommitMsg());
            }
            return GoTo(CoordinatorState.Commited).Using(state.StateData.Copy(ackReplies: ackCount + 1));
          }
        }

        if (state.FsmEvent is StateTimeout)
        {
          Log("received StateTimeout, sending AborMsgs");
          foreach (var cohort in state.StateData.Cohorts)
          {
            Log($"Sending abort to {cohort.Path}");
            cohort.Tell(new AbortMsg());
          }
          return GoTo(CoordinatorState.Aborted);
        }

        return null;
      }, TimeSpan.FromSeconds(5));


      When(CoordinatorState.Commited, _ =>
      {
        Log("Already commited");
        return Stay();
      });

      When(CoordinatorState.Aborted, _ =>
      {
        Log("Already aborted");
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
