using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PYAC.StateMachineV2
{
    public enum ProcessState
    {
        Idle,
        Segment1,
        Segment2,
        Segment3,
        Segment4,
        Segment5,
        Segment6,
        Segment7,
        Segment8,
    }
    public enum Command
    {
        NextSegment
    }
    public class StateMachineV2
    {
        class StateTransition
        {
            readonly ProcessState CurrentState;
            readonly Command Command;

            public StateTransition(ProcessState currentState, Command command)
            {
                CurrentState = currentState;
                Command = command;
            }

            public override int GetHashCode()
            {
                return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                StateTransition other = obj as StateTransition;
                return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
            }
        }

        Dictionary<StateTransition, ProcessState> transitions;
        public ProcessState CurrentState { get; private set; }

        public StateMachineV2()
        {
            CurrentState = ProcessState.Idle;
            transitions = new Dictionary<StateTransition, ProcessState>
            {
                { new StateTransition(ProcessState.Idle,     Command.NextSegment), ProcessState.Segment1 },
                { new StateTransition(ProcessState.Segment1, Command.NextSegment), ProcessState.Segment2 },
                { new StateTransition(ProcessState.Segment2, Command.NextSegment), ProcessState.Segment3 },
                { new StateTransition(ProcessState.Segment3, Command.NextSegment), ProcessState.Segment4 },
                { new StateTransition(ProcessState.Segment4, Command.NextSegment), ProcessState.Segment5 },
                { new StateTransition(ProcessState.Segment5, Command.NextSegment), ProcessState.Segment6 },
                { new StateTransition(ProcessState.Segment6, Command.NextSegment), ProcessState.Segment7 },
                { new StateTransition(ProcessState.Segment7, Command.NextSegment), ProcessState.Segment8 },
                { new StateTransition(ProcessState.Segment8, Command.NextSegment), ProcessState.Idle },
            };
        }

        public ProcessState GetNext(Command command)
        {
            StateTransition transition = new StateTransition(CurrentState, command);
            ProcessState nextState;
            if (!transitions.TryGetValue(transition, out nextState))
                throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
            return nextState;
        }

        public ProcessState MoveNext(Command command)
        {
            CurrentState = GetNext(command);
            MessageBox.Show(CurrentState.ToString());
            return CurrentState;
        }
    }


    //public class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        StateMachineV2 p = new StateMachineV2();
    //        Console.WriteLine("Current State = " + p.CurrentState);
    //        Console.WriteLine("Command.Begin: Current State = " + p.MoveNext(Command.Begin));
    //        Console.WriteLine("Command.Pause: Current State = " + p.MoveNext(Command.Pause));
    //        Console.WriteLine("Command.End: Current State = " + p.MoveNext(Command.End));
    //        Console.WriteLine("Command.Exit: Current State = " + p.MoveNext(Command.Exit));
    //        Console.ReadLine();
    //    }
    //}
}

