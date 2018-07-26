using Kepware.ClientAce.OpcDaClient;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PYAC.Events;
using PYAC.Infrastructure;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utl;

namespace PYAC.ViewModels
{
    public class SetPointOffsetWindowViewModel : BindableBase
    {
        //temp
        int newResult;
        //
        protected readonly IEventAggregator _eventAggregator;
        public DelegateCommand<Window> OffsetAdjustCommand { get; }
        public DelegateCommand<Window> OffsetCloseCommand { get; }
        private OPCUtil OPCLocal;
        private SetPointOffsetWindow setpLocal;
       
        public SetPointOffsetWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<InstanceOfWindowEvent>().Subscribe(getSetpointWindowInstance);


            eventAggregator.GetEvent<UpdatedValueEvent>().Subscribe(itemValueReceive);
            //eventAggregator.GetEvent<IsSubscriptionCompleted>().Publish(false);

            eventAggregator.GetEvent<OPCInstanceEvent>().Subscribe(OPCInstanceReceive);

            instantiateOPC();
             
            OffsetAdjustCommand = new DelegateCommand<Window>(YesCloseWindowCommand);
            OffsetCloseCommand = new DelegateCommand<Window>(NoCloseWindowCommand);
            UIOffsetOld = SetPt;
        }

        private void getSetpointWindowInstance(Window setp)
        {
            setpLocal = (SetPointOffsetWindow) setp;
            //setp.Show();
        }

        private void instantiateOPC()
        {
            try
            {
                if(OPCLocal != null)
                    OPCLocal.Subsribe(getItemIdentifier("Set_Pt"));
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error: SetPt failed to be subscribed to\n(" + ex.ToString() + ")");
            }
        }
        private ItemIdentifier[] getItemIdentifier(string tagName)
        {
            ItemIdentifier[] id = new ItemIdentifier[1];

            id[0] = new ItemIdentifier();

            id[0].ItemName = "AB.Pyradia" + "." + tagName;
            id[0].ClientHandle = tagName;
            id[0].DataType = Type.GetType("System.String");

            return id;

        }

        private void OPCInstanceReceive(OPCUtil _OPCUtl)
        {
            OPCLocal = _OPCUtl;
        }

        private void itemValueReceive(ItemValueCallback obj)
        {
            string name = (string)obj.ClientHandle;
            string value = (string)obj.Value;

            if (Hardware.OffsetTagsList.Contains(name))
            {
                try
                {
                    int index = Hardware.OffsetTagsList.IndexOf(name);
                    string propToUpdate = Hardware.OffsetPropertiesList[index];

                    this[propToUpdate] = value;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to Update(SetPt): " + name + "\n" + ex.ToString());
                }
            }
        }
        public object this[string propToUpdate]
        {
            get
            {
                Type propType = typeof(SetPointOffsetWindowViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type propType = typeof(SetPointOffsetWindowViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                if (myPropInfo != null)
                {
                    myPropInfo.SetValue(this, value, null);
                }
            }
        }



        private void YesCloseWindowCommand(Window obj)
        {
            UIOffsetOld = SetPt;

            if (Int32.Parse(OffsetEnteredAdj) >= -20 && Int32.Parse(OffsetEnteredAdj) <= 20)
            {
                try
                { 
                newResult = Int32.Parse(OffsetEnteredAdj) + Int32.Parse(SetPt);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error: SetPoint is null");
                }
                if (OPCLocal != null) 
                {
                    if (OPCLocal.Connect())
                    { 
                        SetPt = newResult.ToString();
                        UIOffsetNew = SetPt;
                        _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject("Set_Pt", SetPt));
                    }
                            
                            //obj.Close();
                }   
            }
            else
            {
                UIOffsetOld = SetPt;
                MessageBox.Show("Please enter value between -20 and 20.");
            }

            
        }
        private void NoCloseWindowCommand(Window obj)
        {
            obj.Visibility = Visibility.Hidden;
        }

        private string _offsetOriginalValue;
        public string OffsetOriginalValue
        {
            get { return _offsetOriginalValue; }
            set { SetProperty(ref _offsetOriginalValue, value); }
        }
        private string _offsetNewSP;
        public string OffsetNewSP
        {
            get { return _offsetNewSP; }
            set { SetProperty(ref _offsetNewSP, value); }
        }
        //private string _offsetNewValue;
        //public string OffsetNewValue
        //{
        //    get { return _offsetNewValue; }
        //    set { SetProperty(ref _offsetNewValue, value); }
        //}
        private string _offsetEnteredAdj;
        public string OffsetEnteredAdj
        {
            get { return _offsetEnteredAdj; }
            set { SetProperty(ref _offsetEnteredAdj, value); }
        }
        private string _setPt;
        public string SetPt
        {
            get { return _setPt; }
            set { SetProperty(ref _setPt, value); }
        }
        private string _uIOffsetOld;
        public string UIOffsetOld
        {
            get { return _uIOffsetOld; }
            set { SetProperty(ref _uIOffsetOld, value); }
        }
        private string _uIOffsetNew;
        public string UIOffsetNew
        {
            get { return _uIOffsetNew; }
            set { SetProperty(ref _uIOffsetNew, value); }
        }
    }
}
