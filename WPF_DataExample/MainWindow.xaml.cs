// NOTE:add nuget mysql.data.dll

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MySql.Data.MySqlClient;

namespace WPF_DataExample
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            currentTable = Tabletype.NONE;
        }
        // Globale Variablen:
        public Tabletype currentTable;

        #region Methoden / Helper:
        /// <summary>
        /// Creates a database connection, selects * from tabletype and fills the main datagrid with the selection.
        /// </summary>
        /// <param name="tabletype">the table name that will fill the datagrid</param>
        private void FillDataGrid(Tabletype tabletype, DataGrid DG)
        {
            try
            {
                string connstr = ConfigurationManager.ConnectionStrings["DBconnectionString_mv2"].ConnectionString;
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
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Füllt das Mitarbeiterformular mit Auswahlmöglichkeiten.
        /// </summary>
        private void CreateMitarbeiterFormular()
        {
            dataGrid.Visibility = Visibility.Hidden;
            dataGridNeu.Visibility = Visibility.Visible;
            saveNewMitarbeiter.Visibility = Visibility.Visible;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Visible;

            // Comboboxen füllen:
            string connstr = ConfigurationManager.ConnectionStrings["DBconnectionString_mv2"].ConnectionString;
            try
            {
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
                    CB_Vorgesetzter.Items.Add("0 kein Vorgesetzter");
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
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Versucht, den im Formular eingetragenen Mitarbeiter in der Datenbank zu speichern.
        /// </summary>
        private void SaveCreatedMitarbeiter()
        {
            MessageBoxResult mbr = MessageBox.Show("Möchten Sie diesen Mitarbeiter speichern?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (mbr == MessageBoxResult.Yes)
            {
                {
                    string connstr = ConfigurationManager.ConnectionStrings["DBconnectionString_mv2"].ConnectionString;
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connstr))
                        {
                            conn.Open();

                            using (MySqlCommand cmd = conn.CreateCommand())
                            {
                                if (CB_Vorgesetzter.SelectedValue == null || CB_Abteilung.SelectedValue == null || CB_Beruf.SelectedValue == null)
                                {
                                    MessageBox.Show("Fehlende Auswahl: Vorgesetzter, Abteilung oder Beruf!");
                                }
                                else
                                {
                                    UInt32 vid = UInt32.Parse(CB_Vorgesetzter.SelectedValue.ToString().Split(' ')[0]);
                                    UInt32 aid = UInt32.Parse(CB_Abteilung.SelectedValue.ToString().Split(' ')[0]);
                                    UInt32 bid = UInt32.Parse(CB_Beruf.SelectedValue.ToString().Split(' ')[0]);
                                    Mitarbeiter m = new Mitarbeiter()
                                    {
                                        vorname = TB_Vorname.Text,
                                        nachname = TB_Nachname.Text,
                                        geb_dat = TB_Geburtsdatum.Text,
                                        gehalt = TB_Gehalt.Text,
                                        vorgesetzter_id = vid,
                                        a_id = aid,
                                        b_id = bid

                                    };
                                    if (ValidateMitarbeiter(m))
                                    {
                                        cmd.CommandText = "INSERT INTO mitarbeiter (vorname,nachname,geb_dat,gehalt,vorgesetzter_id,a_id,b_id) VALUES(@vn,@nn,@gd,@gh,@vi,@ai,@bi);";
                                        cmd.Parameters.AddWithValue("@vn", m.vorname);
                                        cmd.Parameters.AddWithValue("@nn", m.nachname);
                                        cmd.Parameters.AddWithValue("@gd", m.geb_dat);
                                        cmd.Parameters.AddWithValue("@gh", m.gehalt);
                                        string vi = m.vorgesetzter_id == 0 ? null : m.vorgesetzter_id.ToString();
                                        if(vi == null)
                                        {
                                            cmd.Parameters.AddWithValue("@vi", null);
                                        }
                                        else
                                        {
                                            cmd.Parameters.AddWithValue("@vi", UInt32.Parse(vi));
                                        }
                                        cmd.Parameters.AddWithValue("@ai", m.a_id);
                                        cmd.Parameters.AddWithValue("@bi", m.b_id);

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
                    catch (MySqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Übernimmt die Änderungen in der ausgewählten Reihe und überträgt diese an die SQL Datenbank.
        /// </summary>
        private void UpdateSelectedRow()
        {
            if (currentTable == Tabletype.Mitarbeiter)
            {
                int i = dataGrid.SelectedIndex;
                DataRowView v = (DataRowView)dataGrid.Items[i];
                UInt32 m_id = (UInt32)v[0];
                string vorname = (string)v[1];
                string nachname = (string)v[2];
                string geb_dat = DateTime.Parse(v[3].ToString()).ToString("yyyy-MM-dd");
                string gehalt = v[4].ToString();
                UInt32 vorgesetzter_id = v[5] == DBNull.Value ? (UInt32)0 : (UInt32)v[5];
                UInt32 a_id = (UInt32)v[6];
                UInt32 b_id = (UInt32)v[7];
                Mitarbeiter m = new Mitarbeiter()
                {
                    m_id = m_id,
                    vorname = vorname,
                    nachname = nachname,
                    geb_dat = geb_dat,
                    gehalt = gehalt,
                    vorgesetzter_id = vorgesetzter_id,
                    a_id = a_id,
                    b_id = b_id
                };
                if (ValidateMitarbeiter(m))
                {
                    MessageBoxResult mbr = MessageBox.Show("Sind Sie sicher, dass Sie die Änderungen speichern wollen?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        UpdateSelectedMitarbeiter(m);
                    }
                }
                else
                {
                    MessageBox.Show("Update des Mitarbeiters nicht möglich - fehlerhafte Einträge!");
                }
            }
        }
        /// <summary>
        /// Deletes the selected employee identified by 'm_id' from the database.
        /// </summary>
        /// <param name="m_id"></param>
        private void DeleteSelectedMitarbeiter(UInt32 m_id)
        {
            MessageBoxResult mbr = MessageBox.Show("Soll der gewählte Mitarbeiter wirklich gelöscht werden?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (mbr == MessageBoxResult.Yes)
            {

                try
                {
                    string connstr = ConfigurationManager.ConnectionStrings["DBconnectionString_mv2"].ConnectionString;
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
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
            }
        }
        //TODO: Delete-Methoden für andere Tabellen erstellen
        /// <summary>
        /// Ändert Daten innerhalb eines Mitarbeiters in der Datenbank permanent.
        /// </summary>
        /// <param name="m"></param>
        private void UpdateSelectedMitarbeiter(Mitarbeiter m)
        {
            try
            {
                string connstr = ConfigurationManager.ConnectionStrings["DBconnectionString_mv2"].ConnectionString;
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE mitarbeiter SET vorname = @vn, nachname = @nn, geb_dat = @gd, gehalt = @gh, vorgesetzter_id = @vi, a_id = @ai, b_id = @bi WHERE m_id =" + m.m_id + ";";
                        cmd.Parameters.AddWithValue("@vn", m.vorname);
                        cmd.Parameters.AddWithValue("@nn", m.nachname);
                        cmd.Parameters.AddWithValue("@gd", m.geb_dat);
                        cmd.Parameters.AddWithValue("@gh", m.gehalt);
                        string vi = m.vorgesetzter_id == 0 ? null : m.vorgesetzter_id.ToString();
                        if (vi == null)
                        {
                            cmd.Parameters.AddWithValue("@vi", null);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@vi", UInt32.Parse(vi));
                        }
                        cmd.Parameters.AddWithValue("@ai", m.a_id);
                        cmd.Parameters.AddWithValue("@bi", m.b_id);
                        Debug.WriteLine("MySQL table mitarbeiter: " + cmd.ExecuteNonQuery() + " rows affected!");
                    }

                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }
        //TODO: Update-Methoden für andere Tabellen erstellen

        /// <summary>
        /// Helper-Methode zum formatieren eines IDataRecord in einen String mit dem Format: "{0} {1}".
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <returns></returns>
        private string ReadSingleRowAsStringFormatted(IDataRecord dataRecord)
        {
            return String.Format("{0} {1}", dataRecord[0], dataRecord[1]);
        }
        /// <summary>
        /// Helper-Methode zum formatieren eines IDataRecord in einen UInt32".
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <returns></returns>
        private UInt32 ReadSingleRowAsUInt(IDataRecord dataRecord)
        {
            return (UInt32)dataRecord[0];
        }
        /// <summary>
        /// Helper-Methode zum formatieren eines IDataRecord in eine List<string> mit dem Format{dataRecord[1],dataRecord[2]}".
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <returns></returns>
        private List<string> ReadSingleRowAsListString(IDataRecord dataRecord)
        {
            return new List<string>() { dataRecord[0].ToString(), (string)dataRecord[1], (string)dataRecord[2] };
        }

        //TODO: Validierung verbessern
        /// <summary>
        /// Validiert einen Mitarbeiter für die Datenbank.
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        private bool ValidateMitarbeiter(Mitarbeiter M)
        {
            if (M.vorname.Length == 0) { Debug.WriteLine(M.vorname + " fehlerhaft!"); return false; }
            if (M.nachname.Length == 0) { Debug.WriteLine(M.nachname + " fehlerhaft!"); return false; }
            if (M.geb_dat.Length != 10) { Debug.WriteLine(M.geb_dat + " fehlerhaft!"); return false; } // Beispiel: 1992-01-29
            if (M.gehalt.Length == 0) { Debug.WriteLine(M.gehalt + " fehlerhaft!"); return false; }
            //TODO: UInt32 validieren möglich?
            ///if (M.vorgesetzter_id == null) { Debug.WriteLine(M.vorgesetzter_id + " fehlerhaft!"); return false; }
            if (M.a_id == 0) { Debug.WriteLine(M.a_id + " fehlerhaft!"); return false; }
            if (M.b_id == 0) { Debug.WriteLine(M.b_id + " fehlerhaft!"); return false; }
            return true;
        }
        #endregion

        #region Events:
        private void MenuItem_Mitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Mitarbeiter;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;

            FillDataGrid(Tabletype.Mitarbeiter, dataGrid);
        }
        private void MenuItem_Beruf_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Beruf;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;

            FillDataGrid(Tabletype.Beruf, dataGrid);
        }
        private void MenuItem_Abteilung_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Abteilung;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;

            FillDataGrid(Tabletype.Abteilung, dataGrid);

        }
        private void MenuItem_Standort_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Standort;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;
            
            FillDataGrid(Tabletype.Standort, dataGrid);

        }
        private void MenuItem_Land_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Land;

            dataGrid.Visibility = Visibility.Visible;
            dataGridNeu.Visibility = Visibility.Hidden;
            saveNewMitarbeiter.Visibility = Visibility.Hidden;
            Grid_Mitarbeiter_Neu.Visibility = Visibility.Hidden;

            FillDataGrid(Tabletype.Standort, dataGrid);

        }
        private void MenuItem_Region_Click(object sender, RoutedEventArgs e)
        {
            currentTable = Tabletype.Region;

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
            CreateMitarbeiterFormular();
        }
        private void SaveNewMitarbeiter_Click(object sender, RoutedEventArgs e)
        {
            SaveCreatedMitarbeiter();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if(currentTable == Tabletype.Mitarbeiter)
            {
                int i = dataGrid.SelectedIndex;
                DataRowView v = (DataRowView)dataGrid.Items[i];
                UInt32 s = (UInt32)v[0];
                DeleteSelectedMitarbeiter(s);
            }
        }
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedRow();
        }
        private void DatePicker_Geburtsdatum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Speichert das Datum in einem versteckten Feld im richtigen Format:
            TB_Geburtsdatum.Text = DatePicker_Geburtsdatum.SelectedDate?.ToString("yyyy-MM-dd");
        }

        private void DataGrids_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Passe Datumsformat an Tabellenstandard an:
            DataGridTextColumn column = e.Column as DataGridTextColumn;
            if (column.Header.ToString() == "geb_dat")
            {
                Binding binding = column.Binding as Binding;
                binding.StringFormat = "yyyy-MM-dd";
            }

            // Ersetze leere Felder in der Tabelle mit string.Empty anstatt null, um Fehler beim Editieren zu verhindern:
            System.Windows.Controls.DataGridBoundColumn textCol = e.Column as System.Windows.Controls.DataGridBoundColumn;
            if (textCol != null)
            {
                textCol.Binding.TargetNullValue = string.Empty;
            }
        }
        #endregion

        #region Objekte, Klassen, Enums:
        /// <summary>
        /// Stellt einen Mitarbeiter in der Mitarbeitertabelle dar.
        /// </summary>
        public class Mitarbeiter
        {
            public UInt32 m_id { get; set; }
            public string vorname { get; set; }
            public string nachname { get; set; }
            public string geb_dat { get; set; }
            public string gehalt { get; set; }
            public UInt32 vorgesetzter_id { get; set; }
            public UInt32 a_id { get; set; }
            public UInt32 b_id { get; set; }
        }
        /// <summary>
        /// Enum zur Selektierung der Datenbank.
        /// </summary>
        public enum Tabletype
        {
            Mitarbeiter,
            Beruf,
            Abteilung,
            Standort,
            Land,
            Region,
            NONE
        }
        #endregion
    }
}
