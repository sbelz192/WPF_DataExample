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
                        cmd.CommandText = "SELECT * FROM " + tabletype + ";";
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
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }

        private void MenuItem_Beruf_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Beruf, dataGrid);
        }

        private void MenuItem_Abteilung_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
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
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Visible;

            // Comboboxen füllen:
            string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                conn.Open();

                // Abteilung:
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM abteilung WHERE abteilungsleiter IS NOT NULL;";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CB_Abteilung.Items.Add(ReadSingleRow(reader));
                        }
                    }

                }

                // Beruf:
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT b_id,bezeichnung FROM beruf;";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CB_Beruf.Items.Add(ReadSingleRow(reader));
                        }
                    }
                }

                // Vorgesetzter:
                List<UInt32> vorgesetzte_ids = new List<UInt32>();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT abteilungsleiter FROM abteilung WHERE abteilungsleiter IS NOT NULL;";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vorgesetzte_ids.Add(ReadSingleRow2(reader));
                        }
                    }
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT m_id,vorname,nachname FROM mitarbeiter;";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (vorgesetzte_ids.Contains(UInt32.Parse(ReadSingleRow3(reader)[0])))
                            {
                                CB_Vorgesetzter.Items.Add(string.Join(' ', ReadSingleRow3(reader)));
                            }
                        }
                    }
                }
                conn.Close();
            }
            
        }
        public string ReadSingleRow(IDataRecord dataRecord)
        {
            return String.Format("{0} {1}", dataRecord[0], dataRecord[1]);
        }
        public UInt32 ReadSingleRow2(IDataRecord dataRecord)
        {
            return (UInt32)dataRecord[0];
        }
        public List<string> ReadSingleRow3(IDataRecord dataRecord)
        {
            return new List<string>() { dataRecord[0].ToString(), (string)dataRecord[1], (string)dataRecord[2]};
        }
        private void SaveNewMitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    Mitarbeiter m = new Mitarbeiter()
                    {
                        vorname = TB_Vorname.Text,
                        nachname = TB_Nachname.Text,
                        geb_dat = TB_Geburtsdatum.Text,
                        gehalt = TB_Gehalt.Text,
                        vorgesetzter_id = UInt32.Parse(CB_Vorgesetzter.SelectedValue.ToString().Split(' ')[0]),
                        a_id = UInt32.Parse(CB_Abteilung.SelectedValue.ToString().Split(' ')[0]),
                        b_id = UInt32.Parse(CB_Beruf.SelectedValue.ToString().Split(' ')[0]),
 
                    };
                    cmd.CommandText = "INSERT INTO mitarbeiter (vorname,nachname,geb_dat,gehalt,vorgesetzter_id,a_id,b_id) VALUES(" + MitarbeiterToCSVString(m) + ");";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            FillDataGrid(Tabletype.Mitarbeiter, dataGridNeu);
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

        //TODO: Validierung fertig entwickeln!
        public bool ValidateMitarbeiter(Mitarbeiter M)
        {
            if(M.gehalt.Length == 0) return false;
            if(M.vorname.Length == 0) return false;
            if(M.nachname.Length == 0) return false;

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
                "\'" + m.geb_dat + "\'",
                "\'" + m.gehalt + "\'",
                "\'" + m.vorgesetzter_id + "\'",
                "\'" + m.a_id + "\'",
                "\'" + m.b_id + "\'"
            };
            return string.Join(',', s);
        }

        /// <summary>
        /// Represents an employee.
        /// </summary>
        public class Mitarbeiter
        {
            public UInt32 m_id { get; set; }
            public string vorname { get; set; }
            public string nachname { get; set; }
            public string geb_dat { get; set; }
            public string gehalt { get; set; }
            public string beruf { get; set; }
            public string abteilung { get; set; }
            public UInt32 vorgesetzter_id { get; set; }
            public UInt32 a_id { get; set; }
            public UInt32 b_id { get; set; }
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

        private void DatePicker_Geburtsdatum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TB_Geburtsdatum.Text = String.Format("{0:yyyy-MM-dd}", DatePicker_Geburtsdatum.SelectedDate);
        }
    }
}
