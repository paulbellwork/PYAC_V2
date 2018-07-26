//using Prism.Mvvm;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace PYAC.StateMachine
//{
//    public enum RunningSegmentNumber
//    {
//        Idle,
//        Segment1,
//        Segment2,
//        Segment3,
//        Segment4,
//        Segment5,
//        Segment6,
//        Segment7,
//        Segment8
//    }
//    public enum UserAction 
//    {
//        NextSegment
//    }
//    public class StateMachine : Stateless.StateMachine<RunningSegmentNumber, UserAction>, INotifyPropertyChanged
//    {
//    public event PropertyChangedEventHandler PropertyChanged;

//    public StateMachine(Action searchAction) : base(RunningSegmentNumber.Idle)
//    {
//      Configure(RunningSegmentNumber.Idle)
//        .Permit(UserAction.NextSegment, RunningSegmentNumber.Searching);

//      Configure(RunningSegmentNumber.Searching)
//        .OnEntry(searchAction)
//        .Permit(UserAction.SearchSucceeded, RunningSegmentNumber.SearchComplete)
//        .Permit(UserAction.SearchFailed, RunningSegmentNumber.Idle)
//        .Ignore(UserAction.Select)
//        .Ignore(UserAction.DeSelect); 

//      Configure(RunningSegmentNumber.SearchComplete)
//        .SubstateOf(RunningSegmentNumber.Idle)
//        .Permit(UserAction.Select, RunningSegmentNumber.Selected)
//        .Permit(UserAction.DeSelect, RunningSegmentNumber.NoSelection);

//      Configure(RunningSegmentNumber.Selected)
//        .SubstateOf(RunningSegmentNumber.SearchComplete)
//        .Permit(UserAction.DeSelect, RunningSegmentNumber.NoSelection)
//        .Permit(UserAction.Edit, RunningSegmentNumber.Editing)
//        .Ignore(UserAction.Select);

//      Configure(RunningSegmentNumber.NoSelection)
//        .SubstateOf(RunningSegmentNumber.SearchComplete)
//        .Permit(UserAction.Select, RunningSegmentNumber.Selected)
//        .Ignore(UserAction.DeSelect);

//      Configure(RunningSegmentNumber.Editing)
//        .Permit(UserAction.EndEdit, RunningSegmentNumber.Selected);

//      OnTransitioned
//        (
//          (t) => 
//          {
//            OnPropertyChanged("State");
//            CommandManager.InvalidateRequerySuggested();
//          }
//        );
      
//      //used to debug commands and UI components
//      OnTransitioned
//        (
//          (t) => Debug.WriteLine
//            (
//              "State Machine transitioned from {0} -> {1} [{2}]", 
//              t.Source, t.Destination, t.Trigger
//            )
//        );
//    }

//        private void OnPropertyChanged(string propertyName)
//        {
//            if (PropertyChanged != null)
//            {
//                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }




//    }
//}
