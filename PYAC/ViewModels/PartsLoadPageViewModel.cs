using Oracle.ManagedDataAccess.Client;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    class PartsLoadPageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;

        public DelegateCommand NavigateToAddPartPageCommand { get; private set; }
        public DelegateCommand NavigateToNewCurePageCommand { get; private set; }
        public DelegateCommand EndCureCommand { get; private set; }
        public DelegateCommand<object> AddTCCommand { get; set; }
        public DelegateCommand<object> RemoveTCCommand { get; set; }
        public DelegateCommand<object> RefreshPartsCommand { get; set; }
        public DelegateCommand<object> RemovePartCommand { get; set; }
        public DelegateCommand<object> RefreshRecipesCommand { get; set; }

        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();

        int batchID;
        int partID;
        int sensorID;

        public PartsLoadPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<SendAddPartInfoEvent>().Subscribe(GetNewPartDetails);
            eventAggregator.GetEvent<SendNewCureInfoEvent>().Subscribe(GetNewCureDetails);

            NavigateToAddPartPageCommand = new DelegateCommand(DoAddPart, CanDoAddPart).ObservesProperty(() => IsAddPartEnabled);
            NavigateToNewCurePageCommand = new DelegateCommand(DoNewCure, CanDoNewCure).ObservesProperty(() => IsNewCureEnabled);
            EndCureCommand = new DelegateCommand(DoEndCure, CanDoEndCure).ObservesProperty(() => IsEndCureEnabled); ;
            AddTCCommand = new DelegateCommand<object>(AddTC);
            RemoveTCCommand = new DelegateCommand<object>(RemoveTC);
            RefreshPartsCommand = new DelegateCommand<object>(RefreshParts);
            RemovePartCommand = new DelegateCommand<object>(RemovePart, CanDoRemovePart).ObservesProperty(() => IsRemovePartEnabled); ;
            RefreshRecipesCommand = new DelegateCommand<object>(RefreshRecipes);

            RefreshRecipes(null);
        }
        private bool CanDoRemovePart(object arg)
        {
            return IsRemovePartEnabled;
        }
        private bool CanDoAddPart()
        {
            return IsAddPartEnabled;
        }
        private void DoAddPart()
        {
            NavigateTo("AddPartPage");
        }
        private bool CanDoEndCure()
        {
            return IsEndCureEnabled;
        }
        private void DoEndCure()
        {
        }
        private bool CanDoNewCure()
        {
            return IsNewCureEnabled;
        }
        private void DoNewCure()
        {
            NavigateTo("NewCurePage");
        }
        private void NewCureToggle()
        {
            IsNewCureEnabled = false;
            IsEndCureEnabled = true;
            IsAddPartEnabled = true;
            if (Parts.Any()) //if parts are empty (when removing)
            {
                IsRemovePartEnabled = true;
            }
            else
            {
                IsRemovePartEnabled = false;
            }
        }
        private void EndCureToggle()
        {
            IsNewCureEnabled = true;
            IsEndCureEnabled = false;
            IsAddPartEnabled = false;
            IsRemovePartEnabled = false;
        }


        private void GetNewCureDetails(InfoToSendCure Cure_Object)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                try
                {
                    // Create New Cure
                    if (Cure_Object.isNewCure == true && !string.IsNullOrWhiteSpace(Cure_Object.batch_Number))
                    {
                        string queryString = string.Format("INSERT INTO BATCH (BATCH_NUMBER) VALUES ('{0}')", Cure_Object.batch_Number);
                        OracleCommand command = new OracleCommand(queryString, connection);
                        connection.Open();
                        command.ExecuteNonQuery();

                        NewCureToggle();

                        BatchNumber = Cure_Object.batch_Number;

                        _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");

                    }

                    // Restart Cure with Same Parts
                    else if (Cure_Object.isNewCure == false && !string.IsNullOrWhiteSpace(Cure_Object.batch_Number))
                    {
                        BatchNumber = Cure_Object.batch_Number;

                        string getIDFromBatchNumber = string.Format("select id from batch where BATCH_NUMBER='{0}'", BatchNumber);
                        connection.Open();

                        OracleCommand command1 = new OracleCommand(getIDFromBatchNumber, connection);
                        using (OracleDataReader reader = command1.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("Specified Batch Not Found");
                                return;
                            }
                            while (reader.Read())
                            {
                                batchID = reader.GetInt32(0);
                            }
                            reader.Close();
                        }
                        NewCureToggle();

                        RefreshParts(null);

                        _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");
                    }
                    else
                    {
                        MessageBox.Show("Please Fill in All Fields");
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Find Batch\n" + Ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void GetNewPartDetails(InfoToSendParts Part_Object)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(Part_Object.partNumber) && !string.IsNullOrWhiteSpace(Part_Object.travellerNumber) && !string.IsNullOrWhiteSpace(Part_Object.operation))
                    {
                        string getIDFromBatchNumber = string.Format("select id from batch where BATCH_NUMBER='{0}'", BatchNumber);
                        OracleCommand command1 = new OracleCommand(getIDFromBatchNumber, connection);
                        connection.Open();
                        using (OracleDataReader reader = command1.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                batchID = reader.GetInt32(0);
                            }
                            reader.Close();
                        }

                        string queryString = string.Format("INSERT INTO PART (PART_NUMBER, TRAVELLER_NUMBER, OPERATION, ID_BATCH) VALUES ('{0}', '{1}', '{2}', '{3}')", Part_Object.partNumber, Part_Object.travellerNumber, Part_Object.operation, batchID);
                        OracleCommand command2 = new OracleCommand(queryString, connection);
                        command2.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show("Please Fill in All Fields");
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Write Data into Parts List\n" + Ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
            _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");
            RefreshParts(null);
        }

        private void RefreshParts(object obj)
        {
            Parts.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string queryString = String.Format("SELECT * from PART where id_batch='{0}'", batchID);
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                try
                {

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            foreach (var item in dataRow.ItemArray)
                            {
                                Console.WriteLine(item);
                            }
                        }

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            Parts.Add(new Parts_Details
                            {
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                part_Number = dataTable.Rows[i][1].ToString(),
                                traveller_Number = dataTable.Rows[i][2].ToString(),
                                operation = dataTable.Rows[i][3].ToString(),

                            });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
            if (Parts.Any())
            {
                IsRemovePartEnabled = true;
            }
        }
        private void RemovePart(object obj)
        {
            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        //1-remove parts from the batch. This does not delete the sensors though.
                        string removeFromPartSensor = string.Format("DELETE FROM partsensor WHERE id_part='{0}'", CurrentSelectedPart.ID);
                        OracleCommand command1 = new OracleCommand(removeFromPartSensor, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        string removePart = string.Format("DELETE FROM part WHERE id='{0}'", CurrentSelectedPart.ID);
                        OracleCommand command2 = new OracleCommand(removePart, connection);
                        command2.ExecuteNonQuery();

                        //2-(DID NOT DO)delete sensors: chose to keep all sensors in db

                        RefreshParts(null);
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("Error\n" + Ex.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("List is Empty or No Item Selected");
            }
            CurrentTCS.Clear();

        }

        private void RefreshTC(object obj)
        {
            CurrentTCS.Clear();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                try
                {
                    //3-get id of added part
                    partID = CurrentSelectedPart.ID;

                    //5-get Sensors linked to Parts to be able to Display in UI
                    string getSensorFromPart = string.Format(
                        "SELECT sensor.id, sensor.sensor_number FROM partsensor " +
                        "INNER JOIN part " +
                        "ON partsensor.ID_part = part.id " +
                        "INNER JOIN sensor " +
                        "ON partsensor.ID_sensor = sensor.id " +
                        "where part.id = '{0}' " +
                        "and sensor.type = 'TC'", partID);


                    OracleCommand command = new OracleCommand(getSensorFromPart, connection);
                    connection.Open();

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            CurrentTCS.Add(new TC_Details
                            {
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                sensor_Number = dataTable.Rows[i][1].ToString(),
                            });
                        reader.Close();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void RefreshRecipes(object obj)
        {
            Recipes.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string queryString = "SELECT recipe_name from RECIPE_LIST";
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Recipes.Add(reader.GetString(0));
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private void SelectPart(Parts_Details currentSelectedPart)
        {
            //5-get Sensors linked to Parts to be able to Display in UI
            RefreshTC(null);
        }

        private void AddTC(object obj)
        {
            if (CurrentSelectedPart == null)
            {
                //+ button not enabled
            }
            if(String.IsNullOrWhiteSpace(TCPartToAdd))
            {
                MessageBox.Show("Error: Blank Text Field");
                return;
            }


            CurrentTCS.Clear();
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                //1-add new sensor
                string queryString = String.Format("INSERT INTO SENSOR (TYPE, SENSOR_NUMBER) VALUES ('TC', '{0}')", TCPartToAdd);
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                try
                {
                    command.ExecuteNonQuery();

                    //2-get id of added sensor
                    string getIDFromSensorNumber = string.Format("select id from sensor where SENSOR_NUMBER='{0}'", TCPartToAdd);

                    OracleCommand command1 = new OracleCommand(getIDFromSensorNumber, connection);
                    using (OracleDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sensorID = reader.GetInt32(0);
                        }
                        reader.Close();

                    }

                    //3-get id of added part
                    partID = CurrentSelectedPart.ID;


                    //4-link sensor to part
                    string LinkSensorToPart = string.Format("insert into partsensor (ID_PART, ID_SENSOR) VALUES ('{0}', '{1}')", partID, sensorID);

                    OracleCommand command3 = new OracleCommand(LinkSensorToPart, connection);
                    command3.ExecuteNonQuery();


                    RefreshTC(null);
                    TCPartToAdd = "";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }




        }
        private void RemoveTC(object objToRemove)
        {
            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        string removeFromPartSensor = string.Format("DELETE FROM partsensor WHERE id_sensor='{0}'", CurrentSelectedTC.ID);
                        OracleCommand command1 = new OracleCommand(removeFromPartSensor, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        string removeFromSensor = string.Format("DELETE FROM sensor WHERE id='{0}'", CurrentSelectedTC.ID);
                        OracleCommand command2 = new OracleCommand(removeFromSensor, connection);
                        command2.ExecuteNonQuery();

                        CurrentTCS.Remove(CurrentSelectedTC);
                        RefreshTC(null);
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("Error\n" + Ex.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("List is Empty or No Item Selected");
            }

        }




        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }

        // PROPERTIES
        private string _tCPartToAdd;
        public string TCPartToAdd
        {
            get { return _tCPartToAdd; }
            set { SetProperty(ref _tCPartToAdd, value); }
        }
        private string _offsetEnteredAdj;
        public string OffsetEnteredAdj
        {
            get { return _offsetEnteredAdj; }
            set { SetProperty(ref _offsetEnteredAdj, value); }
        }
        private string _batchNumber;
        public string BatchNumber
        {
            get { return _batchNumber; }
            set { SetProperty(ref _batchNumber, value); }
        }

        private bool _isNewCureEnabled = true;
        public bool IsNewCureEnabled
        {
            get { return _isNewCureEnabled; }
            set
            {
                SetProperty(ref _isNewCureEnabled, value);
                NavigateToNewCurePageCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isEndCureEnabled = false;
        public bool IsEndCureEnabled
        {
            get { return _isEndCureEnabled; }
            set
            {
                SetProperty(ref _isEndCureEnabled, value);
                EndCureCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isAddPartEnabled = false;
        public bool IsAddPartEnabled
        {
            get { return _isAddPartEnabled; }
            set
            {
                SetProperty(ref _isAddPartEnabled, value);
                NavigateToAddPartPageCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRemovePartEnabled = false;
        public bool IsRemovePartEnabled
        {
            get { return _isRemovePartEnabled; }
            set
            {
                SetProperty(ref _isRemovePartEnabled, value);
                RemovePartCommand.RaiseCanExecuteChanged();
            }
        }

        //Using ObservableCollection instead of List.
        //Lists do not notify the View to update itself when an item is added to it.
        //ObservableCollection do.

        public ObservableCollection<string> PartsTC { get; } = new ObservableCollection<string>();

        private ObservableCollection<Parts_Details> _parts = new ObservableCollection<Parts_Details>();
        public ObservableCollection<Parts_Details> Parts
        {
            get { return _parts; }
            set { _parts = value; }
        }

        private ObservableCollection<TC_Details> _currentTCS = new ObservableCollection<TC_Details>();
        public ObservableCollection<TC_Details> CurrentTCS
        {
            get { return _currentTCS; }
            set { _currentTCS = value; }
        }
        private ObservableCollection<string> _recipes = new ObservableCollection<string>();
        public ObservableCollection<string> Recipes
        {
            get { return _recipes; }
            set { _recipes = value; }
        }
        private string _currentSelectedRecipe;
        public string CurrentSelectedRecipe
        {
            get
            {
                return _currentSelectedRecipe;
            }
            set
            {
                SetProperty(ref _currentSelectedRecipe, value);
                if (CurrentSelectedRecipe != null)
                {
                    //SelectRecipe(CurrentSelectedRecipe);
                }
            }
        }
        private Parts_Details _currentSelectedPart;
        public Parts_Details CurrentSelectedPart
        {
            get
            {
                return _currentSelectedPart;
            }
            set
            {
                SetProperty(ref _currentSelectedPart, value);
                if (_currentSelectedPart != null)
                {
                    SelectPart(_currentSelectedPart);
                }
            }
        }
        private TC_Details _currentSelectedTC;
        public TC_Details CurrentSelectedTC
        {
            get
            {
                return _currentSelectedTC;
            }
            set
            {
                SetProperty(ref _currentSelectedTC, value);
            }
        }

    }
    public class Parts_Details
    {
        public int ID { get; set; }
        public string part_Number { get; set; }
        public string traveller_Number { get; set; }
        public string operation { get; set; }

    }
    public class TC_Details
    {
        public int ID { get; set; }
        public string sensor_Number { get; set; }

    }
}
