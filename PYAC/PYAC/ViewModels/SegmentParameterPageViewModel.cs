using Kepware.ClientAce.OpcDaClient;
using Oracle.ManagedDataAccess.Client;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    public class SegmentParameterPageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        static bool isInstantiated;
        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();

        public SegmentParameterPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<UpdatedValueEvent>().Subscribe(GetItemValue);
            eventAggregator.GetEvent<CookInfoEvent>().Subscribe(GetCookInfo);


            if (!isInstantiated)
            {
                isInstantiated = true;

                eventAggregator.GetEvent<IsSubscriptionCompleted>().Publish(true);
            }

        }
        private void GetCookInfo(Cook cook_obj)
        {
            //batch_id
            //recipe_name
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    //1-Get Batch number from ID
                    string getBatchNumber = string.Format("select batch_number from batch where ID='{0}'", cook_obj.batch_ID);
                    OracleCommand command1 = new OracleCommand(getBatchNumber, connection);
                    connection.Open();
                    using (OracleDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BatchNumber = reader.GetString(0);
                        }
                        reader.Close();
                    }

                    //2-Update recipe Name binding
                    RecipeName = cook_obj.recipe_name;

                    //3-Get recipe number from name
                    string getRecipeNumber = string.Format("select recipe_number from recipe where recipe_name='{0}'", cook_obj.recipe_name);
                    OracleCommand command2 = new OracleCommand(getRecipeNumber, connection);
                    using (OracleDataReader reader = command2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RecipeNumber = reader.GetString(0);
                        }
                        reader.Close();
                    }

                    //4-Get recipe id from recipe number
                    string getRecipeID = string.Format("select id from recipe where recipe_name='{0}'", cook_obj.recipe_name);
                    OracleCommand command3 = new OracleCommand(getRecipeID, connection);
                    using (OracleDataReader reader = command3.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RecipeID = reader.GetInt32(0).ToString();
                        }
                        reader.Close();
                    }


                    //5-Count number of segments by coutning number of rows in data table
                    string countRows = String.Format("SELECT recipesegment.id_segment FROM RECIPESEGMENT INNER JOIN segment ON RECIPESEGMENT.ID_segment = segment.id INNER JOIN recipe ON RECIPESEGMENT.ID_recipe = recipe.id where recipe.id = '{0}'", RecipeID);

                    OracleCommand command4 = new OracleCommand(countRows, connection);
                    int NbSegmentsINT = 0;


                    using (OracleDataReader reader = command4.ExecuteReader())

                        while (reader.Read())
                        {
                            NbSegmentsINT++;
                        }
                    NbSegments = NbSegmentsINT.ToString();
                    StartTime = DateTime.Now.ToString("HH: mm:ss tt");

                }

                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to get Recipe Data\n" + Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void GetItemValue(ItemValueCallback obj)
        {
            string name = (string) obj.ClientHandle;
            string value = (string) obj.Value;

            if (Hardware.SegmentParameterTagsList.Contains(name))
            {
                try
                {
                    int index = Hardware.SegmentParameterTagsList.IndexOf(name);
                    string propToUpdate = Hardware.SegmentParameterPropertiesList[index];

                    this[propToUpdate] = value;

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Failed to Update" + name + "\n" + Ex.Message);
                }
            }

        }
        public object this[string propToUpdate]
        {
            get
            {
                Type propType = typeof(SegmentParameterPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type propType = typeof(SegmentParameterPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                myPropInfo.SetValue(this, value, null);
            }
        }



        //PROPERTIES
        private string _zone1Temp;
        public string Zone1Temp
        {
            get { return _zone1Temp; }
            set { SetProperty(ref _zone1Temp, value); }
        }
        private string _zone2Temp;
        public string Zone2Temp
        {
            get { return _zone2Temp; }
            set { SetProperty(ref _zone2Temp, value); }
        }
        private string _lowTempLabel = "Low Temp On";
        public string LowTempLabel
        {
            get { return _lowTempLabel; }
            set { SetProperty(ref _lowTempLabel, value); }
        }
        private string _zone1OP;
        public string Zone1OP
        {
            get { return _zone1OP; }
            set { SetProperty(ref _zone1OP, value); }
        }
        private string _zone2OP;
        public string Zone2OP
        {
            get { return _zone2OP; }
            set { SetProperty(ref _zone2OP, value); }
        }
        private string _zoneCool;
        public string ZoneCool
        {
            get { return _zoneCool; }
            set { SetProperty(ref _zoneCool, value); }
        }
        private string _setPt;
        public string SetPt
        {
            get { return _setPt; }
            set { SetProperty(ref _setPt, value); }
        }
        private string _nbSegments;
        public string NbSegments
        {
            get { return _nbSegments; }
            set { SetProperty(ref _nbSegments, value); }
        }
        private string _batchNumber;
        public string BatchNumber
        {
            get { return _batchNumber; }
            set { SetProperty(ref _batchNumber, value); }
        }
        private string _recipeNumber;
        public string RecipeNumber
        {
            get { return _recipeNumber; }
            set { SetProperty(ref _recipeNumber, value); }
        }
        private string _recipeName;
        public string RecipeName
        {
            get { return _recipeName; }
            set { SetProperty(ref _recipeName, value); }
        }
        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }
        public string RecipeID { get; private set; }


    }

}
