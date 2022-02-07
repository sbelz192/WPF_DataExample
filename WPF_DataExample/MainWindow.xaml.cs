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
        private void FillDataGrid(Tabletype tabletype, DataGrid DG)
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
                        DG.ItemsSource = dt.DefaultView;
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// Deletes the selected employee identified by 'm_id' from the database.
        /// </summary>
        /// <param name="m_id"></param>
        public void DeleteSelectedMitarbeiter(UInt32 m_id)
        {
            try
            {
                string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM mitarbeiter WHERE m_id =" + m_id + ";";
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
            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }

        private void MenuItem_Mitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Felder.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Labels.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }

        private void MenuItem_Beruf_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Felder.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Labels.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Beruf, dataGrid);
        }

        private void MenuItem_Abteilung_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Felder.Visibility = Visibility.Hidden;
            SP_Mitarbeiter_Neu_Labels.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Abteilung, dataGrid);

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_MitarbeiterNeu_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Hidden;
            dataGridNeu.Visibility = Visibility.Visible;
            saveNewMitarbeiter.Visibility = Visibility.Visible;
            SP_Mitarbeiter_Neu_Felder.Visibility = Visibility.Visible;
            SP_Mitarbeiter_Neu_Labels.Visibility = Visibility.Visible;
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
                            cmd.CommandText = "INSERT INTO mitarbeiter (vorname,nachname,gehalt) VALUES(" + MitarbeiterToCSVString(item) + ");";
                            Debug.WriteLine(cmd.CommandText);
                            Debug.WriteLine("MySQL table mitarbeiter: " + cmd.ExecuteNonQuery() + " rows affected!");
                        }
                    }
                    conn.Close();
                }
                FillDataGrid(Tabletype.Mitarbeiter, dataGridNeu);
            }
        }
        /// <summary>
        /// Deletes the selected employee identified by 'm_id' from the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            int i = dataGrid.SelectedIndex;
            DataRowView v = (DataRowView)dataGrid.Items[i];
            UInt32 s = (UInt32)v[0];
            DeleteSelectedMitarbeiter(s);
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
        /// Converts an employee object into a format for SQL insert statement values (in between brackets, separated with commas,...).
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public string MitarbeiterToCSVString(Mitarbeiter m)
        {
            List<string> s = new List<string>
            {
                "\'" + m.vorname + "\'",
                "\'" + m.nachname + "\'",

                "\'" + m.gehalt + "\'"
            };
            return string.Join(',', s);
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
    }
}
