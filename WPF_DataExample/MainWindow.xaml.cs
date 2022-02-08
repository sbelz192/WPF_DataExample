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
                        cmd.CommandText = "SELECT * FROM " + tabletype.ToString().ToLower() + ";";
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
        public string ReadSingleRowAsStringFormatted(IDataRecord dataRecord)
        {
            return String.Format("{0} {1}", dataRecord[0], dataRecord[1]);
        }
        public UInt32 ReadSingleRowAsUInt(IDataRecord dataRecord)
        {
            return (UInt32)dataRecord[0];
        }
        public List<string> ReadSingleRowAsListString(IDataRecord dataRecord)
        {
            return new List<string>() { dataRecord[0].ToString(), (string)dataRecord[1], (string)dataRecord[2] };
        }
        //TODO: Validierung verbessern!
        public bool ValidateMitarbeiter(Mitarbeiter M)
        {
            if (M.vorname.Length == 0) return false;
            if (M.nachname.Length == 0) return false;
            if (M.geb_dat.Length != 10) return false; // Beispiel: 1992-01-29
            if (M.gehalt.Length == 0) return false;
            if (M.vorgesetzter_id == 0) return false;
            if (M.a_id == 0) return false;
            if (M.b_id == 0) return false;
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

        #region Event Listeners:

        private void DatePicker_Geburtsdatum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TB_Geburtsdatum.Text = String.Format("{0:yyyy-MM-dd}", DatePicker_Geburtsdatum.SelectedDate);
        }
        private void MenuItem_Mitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Mitarbeiter;
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }

        private void MenuItem_Beruf_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Beruf;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Beruf, dataGrid);
        }

        private void MenuItem_Abteilung_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Abteilung;
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Abteilung, dataGrid);

        }
        private void MenuItem_Standort_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Standort;
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Standort, dataGrid);

        }
        private void MenuItem_Land_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Land;
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Standort, dataGrid);

        }
        private void MenuItem_Region_Click(object sender, RoutedEventArgs e)
        {
            DataGrid_Template.Header = Tabletype.Region;
            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            FillDataGrid(Tabletype.Region, dataGrid);

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
                            CB_Abteilung.Items.Add(ReadSingleRowAsStringFormatted(reader));
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
                            CB_Beruf.Items.Add(ReadSingleRowAsStringFormatted(reader));
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
                            vorgesetzte_ids.Add(ReadSingleRowAsUInt(reader));
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
                            if (vorgesetzte_ids.Contains(UInt32.Parse(ReadSingleRowAsListString(reader)[0])))
                            {
                                CB_Vorgesetzter.Items.Add(string.Join(' ', ReadSingleRowAsListString(reader)));
                            }
                        }
                    }
                }
                conn.Close();
            }
            
        }

        private void SaveNewMitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            string connstr = "Server=127.0.0.1;Port=3306;Uid=root;Pwd=;database=mv2";
            using (MySqlConnection conn = new MySqlConnection(connstr))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    if(CB_Vorgesetzter.SelectedValue == null || CB_Abteilung.SelectedValue == null || CB_Beruf.SelectedValue == null)
                    {
                        MessageBox.Show("Fehlende Auswahl: Vorgesetzter, Abteilung oder Beruf!");
                    }
                    else
                    {
                        var vid = UInt32.Parse(CB_Vorgesetzter.SelectedValue.ToString().Split(' ')[0]);
                        var aid = UInt32.Parse(CB_Abteilung.SelectedValue.ToString().Split(' ')[0]);
                        var bid = UInt32.Parse(CB_Beruf.SelectedValue.ToString().Split(' ')[0]);
                        Mitarbeiter m = new Mitarbeiter()
                        {
                            vorname = TB_Vorname.Text,
                            nachname = TB_Nachname.Text,
                            geb_dat = TB_Geburtsdatum.Text,
                            gehalt = TB_Gehalt.Text,
                            vorgesetzter_id = vid == 0 ? 0 : vid,
                            a_id = aid == 0 ? 0 : aid,
                            b_id = bid == 0 ? 0 : bid

                        };
                        if (ValidateMitarbeiter(m))
                        {
                            cmd.CommandText = "INSERT INTO mitarbeiter (vorname,nachname,geb_dat,gehalt,vorgesetzter_id,a_id,b_id) VALUES(@vn,@nn,@gd,@gh,@vi,@ai,@bi);";
                            cmd.Parameters.AddWithValue("vn",m.vorname);
                            cmd.Parameters.AddWithValue("nn", m.nachname);
                            cmd.Parameters.AddWithValue("gd", m.geb_dat);
                            cmd.Parameters.AddWithValue("gh", m.gehalt);
                            cmd.Parameters.AddWithValue("vi", m.vorgesetzter_id);
                            cmd.Parameters.AddWithValue("ai", m.a_id);
                            cmd.Parameters.AddWithValue("bi", m.b_id);

                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            MessageBox.Show("Die angegebenen Mitarbeiterdaten können nicht gespeichert werden - sie sind unvollständig oder nicht richtig formatiert.");
                        }
                    }
                    
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
            if(DataGrid_Template.Header?.ToString() == "Mitarbeiter")
            {
                int i = dataGrid.SelectedIndex;
                DataRowView v = (DataRowView)dataGrid.Items[i];
                UInt32 s = (UInt32)v[0];
                DeleteSelectedMitarbeiter(s);
            }

        }
        #endregion

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

    }
}
