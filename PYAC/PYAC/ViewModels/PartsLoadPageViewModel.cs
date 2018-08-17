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
        //CLASS VARIABLES
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        public DelegateCommand NavigateToAddPartPageCommand { get; private set; }
        public DelegateCommand NavigateToNewCurePageCommand { get; private set; }
        public DelegateCommand EndCureCommand { get; private set; }
        public DelegateCommand<object> AddTCCommand { get; set; }
        public DelegateCommand<object> AddTRCommand { get; set; }
        public DelegateCommand<object> AddSourceCommand { get; set; }
        public DelegateCommand<object> RemoveTCCommand { get; set; }
        public DelegateCommand<object> RemoveTRCommand { get; set; }
        public DelegateCommand<object> RemoveSourceCommand { get; set; }
        public DelegateCommand<object> RefreshPartsCommand { get; set; }
        public DelegateCommand<object> RemovePartCommand { get; set; }
        public DelegateCommand<object> RefreshRecipesCommand { get; set; }
        public DelegateCommand<object> CommitLoadCommand { get; set; }
        public DelegateCommand<object> OnlineLoadCommand { get; set; }
        public DelegateCommand<object> OfflineLoadCommand { get; set; }
        public DelegateCommand<object> PrintPartsCommand { get; set; }
        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();

        int batchID;
        int partID;
        int sensorID;//<---------^

        //CONSTRUCTOR
        public PartsLoadPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<SendAddPartInfoEvent>().Subscribe(GetNewPartDetails);
            eventAggregator.GetEvent<SendNewCureInfoEvent>().Subscribe(GetNewCureDetails);

            NavigateToAddPartPageCommand = new DelegateCommand(AddPart, () => IsAddPartEnabled).ObservesProperty(() => IsAddPartEnabled);
            NavigateToNewCurePageCommand = new DelegateCommand(NewCure, () => IsNewCureEnabled).ObservesProperty(() => IsNewCureEnabled);
            EndCureCommand = new DelegateCommand(EndCure, () => IsEndCureEnabled).ObservesProperty(() => IsEndCureEnabled); ;
            AddTCCommand = new DelegateCommand<object>(AddTC, (object obj) => IsAddTCEnabled).ObservesProperty(() => IsAddTCEnabled);
            AddTRCommand = new DelegateCommand<object>(AddTR, (object obj) => IsAddTREnabled).ObservesProperty(() => IsAddTREnabled);
            AddSourceCommand = new DelegateCommand<object>(AddSource, (object obj) => IsAddSourceEnabled ).ObservesProperty(() => IsAddSourceEnabled);
            RemoveTCCommand = new DelegateCommand<object>(RemoveTC, (object obj) => IsRemoveTCEnabled).ObservesProperty(() => IsRemoveTCEnabled);
            RemoveTRCommand = new DelegateCommand<object>(RemoveTR, (object obj) => IsRemoveTREnabled).ObservesProperty(() => IsRemoveTREnabled);
            RemoveSourceCommand = new DelegateCommand<object>(RemoveSource, (object obj) => IsRemoveSourceEnabled).ObservesProperty(() => IsRemoveSourceEnabled);
            RefreshPartsCommand = new DelegateCommand<object>(RefreshParts);
            RemovePartCommand = new DelegateCommand<object>(RemovePart, (object obj) => IsRemovePartEnabled).ObservesProperty(() => IsRemovePartEnabled); ;
            RefreshRecipesCommand = new DelegateCommand<object>(RefreshRecipes);
            CommitLoadCommand = new DelegateCommand<object>(CommitLoad, (object obj) => IsCommitLoadEnabled).ObservesProperty(() => IsCommitLoadEnabled);
            OnlineLoadCommand = new DelegateCommand<object>(OnlineLoad, (object obj) => IsOnlineLoadEnabled).ObservesProperty(() => IsOnlineLoadEnabled);
            OfflineLoadCommand = new DelegateCommand<object>(OfflineLoad, (object obj) => IsOfflineLoadEnabled).ObservesProperty(() => IsOfflineLoadEnabled);
            PrintPartsCommand = new DelegateCommand<object>(PrintParts, (object obj) => IsPrintPartsEnabled).ObservesProperty(() => IsPrintPartsEnabled);

            RefreshRecipes(null);
        }

        //METHODS
        private void PrintParts(object obj)
        {

        }
        private void OfflineLoad(object obj)
        {
            IsOfflineLoadEnabled = false;
            IsOnlineLoadEnabled = true;
            IsAddPartEnabled = true;
            IsRemovePartEnabled = true;
            IsRecipeComboBoxSelectable = true;
        }
        private void OnlineLoad(object obj)
        {
            IsOnlineLoadEnabled = false;
            IsOfflineLoadEnabled = true;
            IsAddPartEnabled = false;
            IsRemovePartEnabled = false;
            IsRecipeComboBoxSelectable = false;
        }
        private void CommitLoad(object obj)
        {
            if (string.IsNullOrWhiteSpace(CurrentSelectedRecipe))
            {
                MessageBox.Show("No Recipe Selected");
                return;
            }
            _eventAggregator.GetEvent<CookInfoEvent>().Publish(new Cook(batchID, CurrentSelectedRecipe));
            IsCommitLoadEnabled = false;
            IsOfflineLoadEnabled = true;
            IsAddPartEnabled = false;
            IsRemovePartEnabled = false;
            IsRecipeComboBoxSelectable = false;
        }

        private void AddPart()
        {
            NavigateTo("AddPartPage");
        }
        private void EndCure()
        {
        }
        private void NewCure()
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
                IsPrintPartsEnabled = true;
                IsCommitLoadEnabled = true;
                IsOnlineLoadEnabled = false; //still false
                IsOfflineLoadEnabled = false;
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
                        //check if cure exists already
                        string queryString0 = string.Format("select * from BATCH where BATCH_NUMBER='{0}'", Cure_Object.batch_Number);
                        OracleCommand command0 = new OracleCommand(queryString0, connection);
                        connection.Open();
                        using (OracleDataReader reader = command0.ExecuteReader())
                        {
                           if (reader.Read())
                            {
                                MessageBox.Show("A Cure with this number already exists");
                                return;
                            }
                            reader.Close();

                        }


                        //if doesnt exist, create new cure
                        string queryString = string.Format("INSERT INTO BATCH (BATCH_NUMBER) VALUES ('{0}')", Cure_Object.batch_Number);
                        OracleCommand command = new OracleCommand(queryString, connection);
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
                        RefreshParts(null);

                        NewCureToggle();



                        _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");

                    }
                    else
                    {
                        MessageBox.Show("Please Fill in All Fields");
                    }
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Find Batch\n" + Ex.Message);
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
                    MessageBox.Show("Failed to Write Data into Parts List\n" + Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");
            IsCommitLoadEnabled = true;
            RefreshParts(null);
        }

        private void RefreshRecipes(object obj)
        {
            Recipes.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string queryString = "SELECT recipe_name from RECIPES";
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                try
                {
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {   
                            while (reader.Read())
                            {
                              Recipes.Add(reader.GetString(0));
                            }
                        }
                        reader.Close();
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void RefreshParts(object obj)
        {
            Parts.Clear();
            CurrentTCS.Clear();
            CurrentTRS.Clear();
            CurrentSources.Clear();

            ActiveTCSensors.Clear();
            ActiveTRSensors.Clear();
            ActiveSourceSensors.Clear();

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
                    IsAddTCEnabled = false;
                    IsAddTREnabled = false;
                    IsAddSourceEnabled = false;

                    IsRemoveTCEnabled = false;
                    IsRemoveTREnabled = false;
                    IsRemoveSourceEnabled = false;
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
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
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void RefreshTR(object obj)
        {
            CurrentTRS.Clear();

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
                        "and sensor.type = 'TR'", partID);


                    OracleCommand command = new OracleCommand(getSensorFromPart, connection);
                    connection.Open();

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            CurrentTRS.Add(new TR_Details
                            {
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                sensor_Number = dataTable.Rows[i][1].ToString(),
                            });
                        reader.Close();
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void RefreshSource(object obj)
        {
            CurrentSources.Clear();

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
                        "and sensor.type = 'Source'", partID);


                    OracleCommand command = new OracleCommand(getSensorFromPart, connection);
                    connection.Open();

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            CurrentSources.Add(new Source_Details
                            {
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                sensor_Number = dataTable.Rows[i][1].ToString(),
                            });
                        reader.Close();
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void SelectPart(Parts_Details currentSelectedPart)
        {
            //1-Refresh Sensors in UI ListView
            RefreshTC(null);
            RefreshTR(null);
            RefreshSource(null);

            ActiveTCSensors.Clear();
            ActiveTRSensors.Clear();
            ActiveSourceSensors.Clear();

            //2-Display Combo Box Options to add sensor
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                try
                {


                    //1-get Active TC Sensors 
                    string getTCSensors = "select sensor_number from activesensors where sensor_type='TC' order by sensor_number";

                    OracleCommand command = new OracleCommand(getTCSensors, connection);
                    connection.Open();

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                        {
                            ActiveTCSensors.Add(dataTable.Rows[i][0].ToString());
                        }
                        reader.Close();
                    }




                    //2-get Active TR Sensors 
                    string getTRSensors = "select sensor_number from activesensors where sensor_type='TR' order by sensor_number";

                    OracleCommand command1 = new OracleCommand(getTRSensors, connection);

                    using (OracleDataReader reader = command1.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                        {
                            ActiveTRSensors.Add(dataTable.Rows[i][0].ToString());
                        }
                        reader.Close();
                    }


                    //3-get Active Source Sensors 
                    string getSourceSensors = "select sensor_number from activesensors where sensor_type='Source' order by sensor_number";

                    OracleCommand command2 = new OracleCommand(getSourceSensors, connection);

                    using (OracleDataReader reader = command2.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                        {
                            ActiveSourceSensors.Add(dataTable.Rows[i][0].ToString());
                        }
                        reader.Close();
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void AddTC(object obj)
        {

            if(String.IsNullOrWhiteSpace(TCPartToAdd))
            {
                MessageBox.Show("Error: No Sensor Selected");
                return;
            }


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    //0-before adding sensor, check if this sensor was already added to the part
                    partID = CurrentSelectedPart.ID;

                    string checkSensorDuplicate = string.Format(
                        "SELECT sensor.id, sensor.sensor_number FROM partsensor " +
                        "INNER JOIN part " +
                        "ON partsensor.ID_part = part.id " +
                        "INNER JOIN sensor " +
                        "ON partsensor.ID_sensor = sensor.id " +
                        "where part.id = '{0}' " +
                        "and sensor.type = 'TC'" +
                        "and sensor.sensor_number = '{1}'"
                        , partID, TCPartToAdd);

                    OracleCommand command0 = new OracleCommand(checkSensorDuplicate, connection);
                    connection.Open();
                    using (OracleDataReader reader = command0.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("This sensor was already added to this part");
                            return;
                        }
                    }

                    //1-add new sensor
                    string queryString = String.Format("INSERT INTO SENSOR (TYPE, SENSOR_NUMBER) VALUES ('TC', '{0}')", TCPartToAdd);
                    OracleCommand command = new OracleCommand(queryString, connection);
               
                    command.ExecuteNonQuery();

                    //2-get id of added sensor
                    string getSensorID = String.Format("SELECT id from ( select a.*, max(id) over () as max_id from sensor a) where id = max_id");
                    OracleCommand command1 = new OracleCommand(getSensorID, connection);

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

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }




        }
        private void AddTR(object obj)
        {

            if (String.IsNullOrWhiteSpace(TRPartToAdd))
            {
                MessageBox.Show("Error: No Sensor Selected");
                return;
            }


            CurrentTRS.Clear();
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    //0-before adding sensor, check if this sensor was already added to the part
                    partID = CurrentSelectedPart.ID;

                    string checkSensorDuplicate = string.Format(
                        "SELECT sensor.id, sensor.sensor_number FROM partsensor " +
                        "INNER JOIN part " +
                        "ON partsensor.ID_part = part.id " +
                        "INNER JOIN sensor " +
                        "ON partsensor.ID_sensor = sensor.id " +
                        "where part.id = '{0}' " +
                        "and sensor.type = 'TR'" +
                        "and sensor.sensor_number = '{1}'"
                        , partID, TRPartToAdd);

                    OracleCommand command0 = new OracleCommand(checkSensorDuplicate, connection);
                    connection.Open();
                    using (OracleDataReader reader = command0.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("This sensor was already added to this part");
                            return;
                        }
                    }



                    //1-add new sensor
                    string queryString = String.Format("INSERT INTO SENSOR (TYPE, SENSOR_NUMBER) VALUES ('TR', '{0}')", TRPartToAdd);
                    OracleCommand command = new OracleCommand(queryString, connection);             
                    command.ExecuteNonQuery();

                    //2-get id of added sensor
                    string getSensorID = String.Format("SELECT id from ( select a.*, max(id) over () as max_id from sensor a) where id = max_id");
                    OracleCommand command1 = new OracleCommand(getSensorID, connection);

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


                    RefreshTR(null);

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }




        }
        private void AddSource(object obj)
        {

            if (String.IsNullOrWhiteSpace(SourcePartToAdd))
            {
                MessageBox.Show("Error: No Sensor Selected");
                return;
            }


            CurrentSources.Clear();
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    //0-before adding sensor, check if this sensor was already added to the part
                    partID = CurrentSelectedPart.ID;

                    string checkSensorDuplicate = string.Format(
                        "SELECT sensor.id, sensor.sensor_number FROM partsensor " +
                        "INNER JOIN part " +
                        "ON partsensor.ID_part = part.id " +
                        "INNER JOIN sensor " +
                        "ON partsensor.ID_sensor = sensor.id " +
                        "where part.id = '{0}' " +
                        "and sensor.type = 'Source'" +
                        "and sensor.sensor_number = '{1}'"
                        , partID, SourcePartToAdd);

                    OracleCommand command0 = new OracleCommand(checkSensorDuplicate, connection);
                    connection.Open();
                    using (OracleDataReader reader = command0.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("This sensor was already added to this part");
                            return;
                        }
                    }
                    //1-add new sensor
                    string queryString = String.Format("INSERT INTO SENSOR (TYPE, SENSOR_NUMBER) VALUES ('Source', '{0}')", SourcePartToAdd);
                    OracleCommand command = new OracleCommand(queryString, connection);
                    command.ExecuteNonQuery();

                    //2-get id of added sensor
                    string getSensorID = String.Format("SELECT id from ( select a.*, max(id) over () as max_id from sensor a) where id = max_id");
                    OracleCommand command1 = new OracleCommand(getSensorID, connection);

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


                    RefreshSource(null);

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
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
                        MessageBox.Show("Error\n" + Ex.Message);
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
            CurrentTRS.Clear();
            CurrentSources.Clear();

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
                        if (!CurrentTCS.Any())
                        {
                            IsRemoveTCEnabled = false;
                        }
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("Error\n" + Ex.Message);
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
        private void RemoveTR(object objToRemove)
        {
            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        string removeFromPartSensor = string.Format("DELETE FROM partsensor WHERE id_sensor='{0}'", CurrentSelectedTR.ID);
                        OracleCommand command1 = new OracleCommand(removeFromPartSensor, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        string removeFromSensor = string.Format("DELETE FROM sensor WHERE id='{0}'", CurrentSelectedTR.ID);
                        OracleCommand command2 = new OracleCommand(removeFromSensor, connection);
                        command2.ExecuteNonQuery();

                        CurrentTRS.Remove(CurrentSelectedTR);
                        RefreshTR(null);
                        if (!CurrentTRS.Any())
                        {
                            IsRemoveTREnabled = false;
                        }
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("Error\n" + Ex.Message);
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
        private void RemoveSource(object objToRemove)
        {
            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        string removeFromPartSensor = string.Format("DELETE FROM partsensor WHERE id_sensor='{0}'", CurrentSelectedSource.ID);
                        OracleCommand command1 = new OracleCommand(removeFromPartSensor, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        string removeFromSensor = string.Format("DELETE FROM sensor WHERE id='{0}'", CurrentSelectedSource.ID);
                        OracleCommand command2 = new OracleCommand(removeFromSensor, connection);
                        command2.ExecuteNonQuery();

                        CurrentSources.Remove(CurrentSelectedSource);
                        RefreshSource(null);
                        if (!CurrentSources.Any())
                        {
                            IsRemoveSourceEnabled = false;
                        }
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("Error\n" + Ex.Message);
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

        //BOOLEAN RETURN METHODS

        //private bool CanDoRemoveSource(object arg)
        //{
        //    return IsRemoveSourceEnabled;
        //}
        //private bool CanDoRemoveTR(object arg)
        //{
        //    return IsRemoveTREnabled;
        //}
        //private bool CanDoRemoveTC(object arg)
        //{
        //    return IsRemoveTCEnabled;
        //}
        //private bool CanDoAddSource(object arg)
        //{
        //    return IsAddSourceEnabled;
        //}
        //private bool CanDoAddTR(object arg)
        //{
        //    return IsAddTREnabled;
        //}
        //private bool CanDoAddTC(object arg)
        //{
        //    return IsAddTCEnabled;
        //}
        //private bool CanDoPrintParts(object arg)
        //{
        //    return IsPrintPartsEnabled;
        //}
        //private bool CanDoOfflineLoad(object arg)
        //{
        //    return IsOfflineLoadEnabled;
        //}
        //private bool CanDoOnlineLoad(object arg)
        //{
        //    return IsOnlineLoadEnabled;
        //}
        //private bool CanDoCommitLoad(object arg)
        //{
        //    return IsCommitLoadEnabled;
        //}
        //private bool CanDoRemovePart(object arg)
        //{
        //    return IsRemovePartEnabled;
        //}
        //private bool CanDoAddPart()
        //{
        //    return IsAddPartEnabled;
        //}
        //private bool CanDoEndCure()
        //{
        //    return IsEndCureEnabled;
        //}
        //private bool CanDoNewCure()
        //{
        //    return IsNewCureEnabled;
        //}


        // MVVM PROPERTIES
        private string _tCPartToAdd;
        public string TCPartToAdd
        {
            get { return _tCPartToAdd; }
            set { SetProperty(ref _tCPartToAdd, value); }
        }
        private string _tRPartToAdd;
        public string TRPartToAdd
        {
            get { return _tRPartToAdd; }
            set { SetProperty(ref _tRPartToAdd, value); }
        }
        private string _sourcePartToAdd;
        public string SourcePartToAdd
        {
            get { return _sourcePartToAdd; }
            set { SetProperty(ref _sourcePartToAdd, value); }
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
        private bool _isCommitLoadEnabled = false;
        public bool IsCommitLoadEnabled
        {
            get { return _isCommitLoadEnabled; }
            set
            {
                SetProperty(ref _isCommitLoadEnabled, value);
                CommitLoadCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isOnlineLoadEnabled = false;
        public bool IsOnlineLoadEnabled
        {
            get { return _isOnlineLoadEnabled; }
            set
            {
                SetProperty(ref _isOnlineLoadEnabled, value);
                OnlineLoadCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isOfflineLoadEnabled = false;
        public bool IsOfflineLoadEnabled
        {
            get { return _isOfflineLoadEnabled; }
            set
            {
                SetProperty(ref _isOfflineLoadEnabled, value);
                OfflineLoadCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isPrintPartsEnabled = false;
        public bool IsPrintPartsEnabled
        {
            get { return _isPrintPartsEnabled; }
            set
            {
                SetProperty(ref _isPrintPartsEnabled, value);
                PrintPartsCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isAddTCEnabled = false;
        public bool IsAddTCEnabled
        {
            get { return _isAddTCEnabled; }
            set
            {
                SetProperty(ref _isAddTCEnabled, value);
                AddTCCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isAddTREnabled = false;
        public bool IsAddTREnabled
        {
            get { return _isAddTREnabled; }
            set
            {
                SetProperty(ref _isAddTREnabled, value);
                AddTRCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isAddSourceEnabled = false;
        public bool IsAddSourceEnabled
        {
            get { return _isAddSourceEnabled; }
            set
            {
                SetProperty(ref _isAddSourceEnabled, value);
                AddSourceCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRemoveTCEnabled = false;
        public bool IsRemoveTCEnabled
        {
            get { return _isRemoveTCEnabled; }
            set
            {
                SetProperty(ref _isRemoveTCEnabled, value);
                RemoveTCCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRemoveTREnabled = false;
        public bool IsRemoveTREnabled
        {
            get { return _isRemoveTREnabled; }
            set
            {
                SetProperty(ref _isRemoveTREnabled, value);
                RemoveTRCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRemoveSourceEnabled = false;
        public bool IsRemoveSourceEnabled
        {
            get { return _isRemoveSourceEnabled; }
            set
            {
                SetProperty(ref _isRemoveSourceEnabled, value);
                RemoveSourceCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRecipeComboBoxSelectable = true;
        public bool IsRecipeComboBoxSelectable
        {
            get { return _isRecipeComboBoxSelectable; }
            set
            {
                SetProperty(ref _isRecipeComboBoxSelectable, value);
            }
        }


        //OTHER PROPERTIES        
        /*Using ObservableCollection instead of List.
        Lists do not notify the View to update itself when an item is added to it.
        ObservableCollection do.*/
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
        private ObservableCollection<TR_Details> _currentTRS = new ObservableCollection<TR_Details>();
        public ObservableCollection<TR_Details> CurrentTRS
        {
            get { return _currentTRS; }
            set { _currentTRS = value; }
        }
        private ObservableCollection<Source_Details> _currentSources = new ObservableCollection<Source_Details>();
        public ObservableCollection<Source_Details> CurrentSources
        {
            get { return _currentSources; }
            set { _currentSources = value; }
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
                if (!string.IsNullOrWhiteSpace(BatchNumber))
                {
                    //IsCommitLoadEnabled = true;
                }
            }
        }
        private ObservableCollection<string> _activeTCSensors = new ObservableCollection<string>();
        public ObservableCollection<string> ActiveTCSensors
        {
            get { return _activeTCSensors; }
            set { _activeTCSensors = value; }
        }
        private ObservableCollection<string> _activeTRSensors = new ObservableCollection<string>();
        public ObservableCollection<string> ActiveTRSensors
        {
            get { return _activeTRSensors; }
            set { _activeTRSensors = value; }
        }
        private ObservableCollection<string> _activeSourceSensors = new ObservableCollection<string>();
        public ObservableCollection<string> ActiveSourceSensors
        {
            get { return _activeSourceSensors; }
            set { _activeSourceSensors = value; }
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
                    IsAddTCEnabled = true;
                    IsAddTREnabled = true;
                    IsAddSourceEnabled = true;

                    IsRemoveTCEnabled = false;
                    IsRemoveTREnabled = false;
                    IsRemoveSourceEnabled = false;
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
                IsRemoveTCEnabled = true;
            }
        }
        private TR_Details _currentSelectedTR;
        public TR_Details CurrentSelectedTR
        {
            get
            {
                return _currentSelectedTR;
            }
            set
            {
                SetProperty(ref _currentSelectedTR, value);
                IsRemoveTREnabled = true;
            }
        }
        private Source_Details _currentSelectedSource;
        public Source_Details CurrentSelectedSource
        {
            get
            {
                return _currentSelectedSource;
            }
            set
            {
                SetProperty(ref _currentSelectedSource, value);
                IsRemoveSourceEnabled = true;
            }
        }
    }

    //DECLARED CLASSES 
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
    public class TR_Details
    {
        public int ID { get; set; }
        public string sensor_Number { get; set; }

    }
    public class Source_Details
    {
        public int ID { get; set; }
        public string sensor_Number { get; set; }

    }
}
