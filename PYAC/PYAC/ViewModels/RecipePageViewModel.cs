using Oracle.ManagedDataAccess.Client;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Views;
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
using System.Windows.Controls;

namespace PYAC.ViewModels
{
    public class RecipePageViewModel : BindableBase
    {
        //CLASS VARIABLES
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;

        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();
        public DelegateCommand<object> RefreshRecipesCommand { get; set; }
        public DelegateCommand<object> NewRecipeCommand { get; set; }
        public DelegateCommand<object> AddSegmentCommand { get; set; }
        public DelegateCommand<object> RemoveSegmentCommand { get; set; }
        public DelegateCommand<object> EditSegmentCommand { get; set; }
        public DelegateCommand<object> DeleteRecipeCommand { get; set; }

        public int idSegment { get; set; }
        public int idRecipe { get; set; }//<---------^

        //CONSTRUCTOR
        public RecipePageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            RefreshRecipesCommand = new DelegateCommand<object>(RefreshRecipes);
            NewRecipeCommand = new DelegateCommand<object>(NewRecipe);
            DeleteRecipeCommand = new DelegateCommand<object>(CheckDeleteRecipe);
            AddSegmentCommand = new DelegateCommand<object>(AddSegment);
            RemoveSegmentCommand = new DelegateCommand<object>(RemoveSegment);
            EditSegmentCommand = new DelegateCommand<object>(EditSegment);
            RefreshRecipes(null);
        }

        //METHODS
        private void CheckDeleteRecipe(object obj)
        {
            if (CurrentSelectedRecipe == null)
            {
                MessageBox.Show("Please select a recipe to be deleted");
                return;
            }

            //IF specific admin user is logged in, no need to ask for login
            if (TitleMenuViewModel._username.Equals("Admin3"))
            {
                DeleteRecipe();
                return;
            }


            //IF no user is logged in, ask user to login and make sure admin user with permissions

            var login = new Login();
            var loginVM = new LoginViewModel();
            bool isPassCorrect = false;

            //IF login is successful, do following code (SAME CODE AS THE )
            loginVM.LoginCompleted += (sender, args) =>
            {
                DeleteRecipe();
                login.Close();

                isPassCorrect = true;
                _eventAggregator.GetEvent<LoginUsernameEvent>().Publish(LoginViewModel._username);
                Username = LoginViewModel._username;

            };
            login.DataContext = loginVM;
            login.ShowDialog();
            login.Activate();

            if (!isPassCorrect)
            {
                MessageBox.Show("Incorrect Login Information");
                login.Close();
            }


        }
        private void DeleteRecipe()
        {
            if (MessageBox.Show("Are you sure you would like to delete the recipe: " + CurrentSelectedRecipe.Recipe_Name + "?", "Delete Recipe?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        //1-remove segments from recipesegment. This does not delete the segments though.
                        string removeFromRecipeSegment = string.Format("DELETE FROM segments WHERE id_recipe='{0}'", CurrentSelectedRecipe.Recipe_ID);
                        OracleCommand command1 = new OracleCommand(removeFromRecipeSegment, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        string removeRecipe = string.Format("DELETE FROM recipes WHERE id='{0}'", CurrentSelectedRecipe.Recipe_ID);
                        OracleCommand command2 = new OracleCommand(removeRecipe, connection);
                        command2.ExecuteNonQuery();


                        RefreshRecipes(null);
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

        private void EditSegment(object obj)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    string updateSegment = string.Format("update segments set Segment_number = '{0}', ramp_sp = '{1}', min_ramp = '{2}', max_ramp = '{3}', soak_sp = '{4}', min_soak = '{5}', max_soak = '{6}', soak_time = '{7}', low_temp_mode_sp = '{8}', alarm_temp_th = '{9}', low_temp_mode_en = '{10}' where id='{11}'", CurrentSelectedSegment.Segment_Number, CurrentSelectedSegment.RAMP_SP, CurrentSelectedSegment.MIN_RAMP, CurrentSelectedSegment.MAX_RAMP, CurrentSelectedSegment.SOAK_SP, CurrentSelectedSegment.MIN_SOAK, CurrentSelectedSegment.MAX_SOAK, CurrentSelectedSegment.SOAK_TIME, CurrentSelectedSegment.LOW_TEMP_MODE_SP, CurrentSelectedSegment.ALARM_TEMP_TH, Convert.ToChar(CurrentSelectedSegment.LOW_TEMP_MODE_EN), CurrentSelectedSegment.ID);
                    OracleCommand command1 = new OracleCommand(updateSegment, connection);
                    connection.Open();
                    command1.ExecuteNonQuery();

                    RefreshSegments(SelectedRecipeID);
                }

                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Add Segments\n" + Ex.ToString());
                }
                finally
                {
                    connection.Close();
                }

            
        }
            
        }

        private void RemoveSegment(object obj)
        {
            try
            {

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    try
                    {
                        //1-remove parts from the batch. This does not delete the sensors though.
                        string removeFromRecipeSegment = string.Format("DELETE FROM segments WHERE id='{0}'", CurrentSelectedSegment.ID);
                        OracleCommand command1 = new OracleCommand(removeFromRecipeSegment, connection);
                        connection.Open();
                        command1.ExecuteNonQuery();

                        RefreshSegments(SelectedRecipeID);
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

        private void AddSegment(object obj)
        {
            string messageBoxDuplicates = "";
            ObservableCollection<Segment_Details> Segments = (ObservableCollection<Segment_Details>) obj;
            if(Segments.Count == 0)
            {
                MessageBox.Show("No Segments To be Added");
                return;
            }
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    foreach (Segment_Details segment in Segments)
                    {
                        //1-Check if Segment already exists (needs to be done to avoid re-adding the same segments)
                        string queryString = String.Format("SELECT * FROM SEGMENTS INNER JOIN RECIPES ON SEGMENTS.ID_RECIPE = RECIPES.ID where segments.segment_number = '{0}' and recipes.id='{1}' and segments.ramp_SP='{2}'", segment.Segment_Number, SelectedRecipeID, segment.RAMP_SP);
                        OracleCommand command1 = new OracleCommand(queryString, connection);
                        using (OracleDataReader reader = command1.ExecuteReader())
                        { 
                            if (reader.HasRows)
                            {
                                if(messageBoxDuplicates == "")
                                {
                                    messageBoxDuplicates += (String.Format("Segments with the following numbers already exist: \n{0}", segment.Segment_Number));

                                }
                                else
                                {
                                    messageBoxDuplicates += ", " + segment.Segment_Number;
                                }
                                continue;
                            }
                        }


                        //2-Add segment if does not exist in current segments
                        string addSegment = string.Format("INSERT INTO segments (ID_RECIPE, Segment_number, ramp_sp, min_ramp, max_ramp, soak_sp, min_soak, max_soak, soak_time, low_temp_mode_sp, alarm_temp_th, low_temp_mode_en) VALUES ('{0}', '{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')", 
                            SelectedRecipeID, segment.Segment_Number, segment.RAMP_SP, segment.MIN_RAMP, segment.MAX_RAMP, segment.SOAK_SP, segment.MIN_SOAK, segment.MAX_SOAK, segment.SOAK_TIME, segment.LOW_TEMP_MODE_SP, segment.ALARM_TEMP_TH, Convert.ToChar(segment.LOW_TEMP_MODE_EN));
                        OracleCommand command2 = new OracleCommand(addSegment, connection);
                        command2.ExecuteNonQuery();


                        ////3-Get ID of inserted Segment
                        //string getSegmentID = String.Format("SELECT id from ( select a.*, max(id) over () as max_id from segments a) where id = max_id");
                        //OracleCommand command3 = new OracleCommand(getSegmentID, connection);
                      
                        //using (OracleDataReader reader = command3.ExecuteReader())
                        //{
                        //    while (reader.Read())
                        //    {
                        //        idSegment = reader.GetInt32(0);
                        //    }
                        //    reader.Close();

                        //}


                        ////4-Link Segments to the selected recipe
                        //string linkSegment = string.Format("INSERT INTO recipesegment (id_recipe, id_segment) VALUES ('{0}', '{1}')", SelectedRecipeID, idSegment);
                        //OracleCommand command4 = new OracleCommand(linkSegment, connection);
                        //command4.ExecuteNonQuery();
                    }
                    if (!string.IsNullOrWhiteSpace(messageBoxDuplicates))
                    {
                        MessageBox.Show(messageBoxDuplicates);
                    }
                    RefreshSegments(SelectedRecipeID);
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Add Segments\n" + Ex.Message);
                    RefreshSegments(SelectedRecipeID);

                }
                finally
                {
                    connection.Close();
                }

            }

        }

        private void NewRecipe(object obj)
        {

            //1-Check if selected item was already added
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    string CheckIfRecipe = string.Format("Select * from recipes where recipe_number='{0}' and recipe_name='{1}'", CurrentSelectedRecipe.Recipe_Number, CurrentSelectedRecipe.Recipe_Name);
                    OracleCommand command1 = new OracleCommand(CheckIfRecipe, connection);
                    connection.Open();
                    OracleDataReader reader = command1.ExecuteReader();

                    if (reader.HasRows)
                    {
                        MessageBox.Show("A Recipe with this number already exists");
                        return; 
                    }


                    //2-Add if not already present in recipes
                    string queryString = string.Format("INSERT INTO recipes (recipe_name, recipe_NUMBER) VALUES ('{0}', '{1}')", CurrentSelectedRecipe.Recipe_Name, CurrentSelectedRecipe.Recipe_Number);
                    OracleCommand command2 = new OracleCommand(queryString, connection);
                    command2.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to Create Recipe\n" + Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
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
                string queryString = String.Format("SELECT * FROM SEGMENTS INNER JOIN RECIPES on SEGMENTS.ID_recipe = recipes.id where recipes.id = '{0}' order by segment_number", id);

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
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                Segment_Number = dataTable.Rows[i][2].ToString(),
                                RAMP_SP   = dataTable.Rows[i][3].ToString(),
                                MIN_RAMP  = dataTable.Rows[i][4].ToString(),
                                MAX_RAMP  = dataTable.Rows[i][5].ToString(),
                                SOAK_SP   = dataTable.Rows[i][6].ToString(),
                                MIN_SOAK  = dataTable.Rows[i][7].ToString(),
                                MAX_SOAK  = dataTable.Rows[i][8].ToString(),
                                SOAK_TIME  = dataTable.Rows[i][9].ToString(),
                                LOW_TEMP_MODE_SP= dataTable.Rows[i][10].ToString(),
                                ALARM_TEMP_TH  = dataTable.Rows[i][11].ToString(),
                                LOW_TEMP_MODE_EN = Convert.ToChar(dataTable.Rows[i][12]),

                            });
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
        private void RefreshRecipes(object obj)
        {
            Recipes.Clear();
            SelectedRecipeName = "";
            SelectedRecipeNumber = "";
            Segments.Clear();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string queryString = "SELECT * from RECIPES";
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
                                Recipe_Name = dataTable.Rows[i][1].ToString(),
                                Recipe_Number = dataTable.Rows[i][2].ToString(),

                            });
                    }
                    
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
        }


        //PROPERTIES
        public class Recipe_Details
        {
         
            public int Recipe_ID { get; set; }
            public string Recipe_Name { get; set; }
            public string Recipe_Number { get; set; }
        }
        public class Segment_Details
        {
            public int ID { get; set; }
            public string Segment_Number { get; set; }
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
                try
                {
                    SelectedRecipeName = CurrentSelectedRecipe.Recipe_Name.ToString();
                    SelectedRecipeNumber = CurrentSelectedRecipe.Recipe_Number;
                    SelectedRecipeID = CurrentSelectedRecipe.Recipe_ID;
                }
                catch (Exception)
                {

                }
            }
        }
        private int _selectedRecipeID;
        public int SelectedRecipeID
        {
            get { return _selectedRecipeID; }
            set { SetProperty(ref _selectedRecipeID, value); }
        }
        private string _selectedRecipeString;
        public string SelectedRecipeName
        {
            get { return _selectedRecipeString; }
            set { SetProperty(ref _selectedRecipeString, value); }
        }
        private string _selectedRecipeNumber;
        public string SelectedRecipeNumber
        {
            get { return _selectedRecipeNumber; }
            set { SetProperty(ref _selectedRecipeNumber, value); }
        }
        private Segment_Details _currentSelectedSegment;
        public Segment_Details CurrentSelectedSegment
        {
            get
            {
                return _currentSelectedSegment;
            }
            set
            {
                try
                {
                    SetProperty(ref _currentSelectedSegment, value);
                }
                catch (Exception)
                {

                }
            }
        }
        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }


    }
}
