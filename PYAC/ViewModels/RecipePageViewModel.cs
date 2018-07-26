using Oracle.ManagedDataAccess.Client;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PYAC.ViewModels
{
    public class RecipePageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();
        public DelegateCommand<object> RefreshRecipesCommand { get; set; }
        //public DelegateCommand<object> SelectRecipeCommand { get; set; }

        public RecipePageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            RefreshRecipesCommand = new DelegateCommand<object>(RefreshRecipes);
            //SelectRecipeCommand = new DelegateCommand<object>(SelectRecipe);
            RefreshRecipes(null);

        }

        private void SelectRecipe(Recipe_Details selectedRecipe)

        {
            try
            {
                RefreshSegments(selectedRecipe.Recipe_ID);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error, no Item Selected\n" + Ex.Message);
            }
        }

        private void RefreshSegments(int id)
        {
            Segments.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                //string queryString = String.Format("SELECT * FROM RECIPESEGMENT " +
                //    "INNER JOIN segment ON RECIPESEGMENT.ID_segment=segment.id " +
                //    "INNER JOIN recipe ON RECIPESEGMENT.ID_recipe=recipe.id " +
                //    "where recipe.id={0};", id);
                string queryString = String.Format("SELECT * FROM RECIPESEGMENT INNER JOIN segment ON RECIPESEGMENT.ID_segment = segment.id INNER JOIN recipe ON RECIPESEGMENT.ID_recipe = recipe.id where recipe.id = '{0}'", id);

                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                try
                {

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        //dataTable.Rows[0][]
                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            Segments.Add(new Segment_Details
                            {
                                Segment_ID = Convert.ToInt32(dataTable.Rows[i][4]),
                                RAMP_SP   = dataTable.Rows[i][5].ToString(),
                                MIN_RAMP  = dataTable.Rows[i][6].ToString(),
                                MAX_RAMP  = dataTable.Rows[i][7].ToString(),
                                SOAK_SP   = dataTable.Rows[i][8].ToString(),
                                MIN_SOAK  = dataTable.Rows[i][9].ToString(),
                                MAX_SOAK  = dataTable.Rows[i][10].ToString(),
                                SOAK_TIME  = dataTable.Rows[i][11].ToString(),
                                LOW_TEMP_MODE_SP= dataTable.Rows[i][12].ToString(),
                                ALARM_TEMP_TH  = dataTable.Rows[i][13].ToString(),
                                LOW_TEMP_MODE_EN = Convert.ToChar(dataTable.Rows[i][14]),

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
        }
    

        private void RefreshRecipes(object obj)
        {
            Recipes.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string queryString = "SELECT * from RECIPE";
                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                try
                {

                    using (OracleDataReader da = command.ExecuteReader())
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
                            Recipes.Add(new Recipe_Details
                            {
                                Recipe_ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                Recipe_Number = dataTable.Rows[i][1].ToString(),
                                Recipe_Name = dataTable.Rows[i][2].ToString(),

                            });
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

        public class Recipe_Details
        {
         
            public int Recipe_ID { get; set; }
            public string Recipe_Name { get; set; }
            public string Recipe_Number { get; set; }
        }
        public class Segment_Details
        {
            public int Segment_ID { get; set; }
            public string RAMP_SP  { get; set; }
            public string MIN_RAMP { get; set; }
            public string MAX_RAMP { get; set; }
            public string SOAK_SP  { get; set; }
            public string MIN_SOAK { get; set; }
            public string MAX_SOAK { get; set; }
            public string SOAK_TIME { get; set; }
            public string LOW_TEMP_MODE_SP { get; set; }
            public string ALARM_TEMP_TH { get; set; }
            public char LOW_TEMP_MODE_EN { get; set; }

        }

        private ObservableCollection<Recipe_Details> _recipes = new ObservableCollection<Recipe_Details>();
        public ObservableCollection<Recipe_Details> Recipes
        {
            get { return _recipes; }
            set { _recipes = value; }
        }

        private ObservableCollection<Segment_Details> _segments = new ObservableCollection<Segment_Details>();
        public ObservableCollection<Segment_Details> Segments
        {
            get { return _segments; }
            set { _segments = value; }
        }
        private Recipe_Details _currentSelectedRecipe;
        public Recipe_Details CurrentSelectedRecipe
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
                    SelectRecipe(CurrentSelectedRecipe);
                }
            }
        }


    }
}
