using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace WPF_DataExample
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*mysql
        * 
        * 1. add nuget mysql.data.dll（desktop application）
        * 2. using MySql.Data.MySqlClient;
        */
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a database connection, selects * from tabletype and fills the main datagrid with the selection.
        /// </summary>
        /// <param name="tabletype">the table name that will fill the datagrid</param>
        private void LoadDatabaseTableCommand(Tabletype tabletype)
        {
            try
            {
                string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select * from " + tabletype + ";";
                        DataTable dt = new DataTable();
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        da.Fill(dt);
                        dataGrid.ItemsSource = dt.DefaultView;
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void DeleteSelectedMitarbeiter(UInt32 mid)
        {
            try
            {
                string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM mitarbeiter WHERE m_id =" + mid + ";";
                        Debug.WriteLine(cmd.CommandText);
                        Debug.WriteLine("MySQL table mitarbeiter: " + cmd.ExecuteNonQuery() + " rows affected!");
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            LoadDatabaseTableCommand(Tabletype.Mitarbeiter);
        }

        private void MenuItem_Mitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabaseTableCommand(Tabletype.Mitarbeiter);
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Beruf_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabaseTableCommand(Tabletype.Beruf);
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Abteilung_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabaseTableCommand(Tabletype.Abteilung);
            saveNewMitarbeiter.Visibility = Visibility.Hidden;

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_MitarbeiterNeu_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = new List<Mitarbeiter>() { };
            dataGrid.CanUserAddRows = true;
            saveNewMitarbeiter.Visibility = Visibility.Visible;
        }

        private void SaveNewMitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateNewMitarbeiter())
            {
                string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        foreach (Mitarbeiter item in dataGrid.Items.OfType<Mitarbeiter>().ToList())
                        {
                            cmd.CommandText = "insert into mitarbeiter (vorname,nachname,gehalt) values(" + MitarbeiterToSQLString(item) + ");";
                            Debug.WriteLine(cmd.CommandText);
                            Debug.WriteLine("MySQL table mitarbeiter: " + cmd.ExecuteNonQuery() + " rows affected!");
                        }
                    }
                    conn.Close();
                }
                LoadDatabaseTableCommand(Tabletype.Mitarbeiter);
                saveNewMitarbeiter.Visibility = Visibility.Hidden;
                dataGrid.CanUserAddRows = false;
            }
        }

        /// <summary>
        /// Makes sure, that the given User data is safe and fits the table requirements.
        /// </summary>
        /// <returns></returns>
        public bool ValidateNewMitarbeiter()
        {
            Window popup = new Window();
            foreach (var item in dataGrid.Items.OfType<Mitarbeiter>())
            {
                if (item.vorname.Length == 0)
                {
                    popup.Content = "invalid \"vorname\"";
                    popup.ShowDialog();
                    return false;

                }
                else if (item.nachname.Length == 0)
                {
                    popup.Content = "invalid \"nachname\"";
                    popup.ShowDialog();
                    return false;

                }
                else if (item.gehalt.Length == 0)
                {
                    popup.Content = "invalid \"gehalt\"";
                    popup.ShowDialog();
                    return false;

                }
            }
            return true;
        }

        /// <summary>
        /// Represents an employee.
        /// </summary>
        public class Mitarbeiter
        {
            //public string mid { get; set; }
            public string vorname { get; set; }
            public string nachname { get; set; }
            //public string gebdatum { get; set; }
            public string gehalt { get; set; }
            //public string vorgesetzterid { get; set; }
            // public string aid { get; set; }
            // public string bid { get; set; }
        }
        public enum Tabletype
        {
            Mitarbeiter,
            Beruf,
            Abteilung,
            Standort,
            Land,
            Region

        }

        /// <summary>
        /// Converts an employee object into a format for SQL insert statement values (in between brackets, separated with commas,...).
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public string MitarbeiterToSQLString(Mitarbeiter m)
        {
            List<string> s = new List<string>();
            s.Add("\'" + m.vorname + "\'");
            s.Add("\'" + m.nachname + "\'");

            s.Add("\'" + m.gehalt + "\'");
            return string.Join(',', s);
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            DataRowView v = (DataRowView)dataGrid.Items[i];
            UInt32 s = (UInt32)v[0];
            DeleteSelectedMitarbeiter(s);
            
        }
    }
}
