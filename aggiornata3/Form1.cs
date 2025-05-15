using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WA_Progetto
{
    public partial class Form1 : Form
    {
        static string Database_name = "QueriesDb"; //viene utilizata nella stringa di connesione e nella scrittura della query
        static string Connection = $"Data Source=SARS-COV2;Initial Catalog={Database_name};Integrated Security=True";
        static SqlConnection cnn = new SqlConnection(Connection);
        //Variabili globali
        List <string> Tables_name = new List<string>();

        Dictionary<string, int> FreeIdTables = new Dictionary<string, int>();
        static Form frm_Querie = null;
        static DataGridViewRow row = null;
        static List<string> names = new List<string>();
        //libreria contenente le query utilizzate
        static LibraryQuery LQ = new LibraryQuery();

        public Form1()
        {
            InitializeComponent();
            Tables_name.Add("Queries");
            Tables_name.Add("Queries_CrossModules");
            Tables_name.Add("Queries_Parameter");
            string query = $"SELECT * FROM {Tables_name[0]}";
            dgv_Tabella.DataSource = LQ.ExecuteQ(query, cnn).Tables[0];
            FreeIdTables.Add(Tables_name[0],LQ.GetFirstFreeID(Tables_name[0], "ID_Queries", cnn));
            FreeIdTables.Add(Tables_name[1], LQ.GetFirstFreeID(Tables_name[1], LQ.GetAllColumnNames(Tables_name[1], cnn)[0], cnn));
            FreeIdTables.Add(Tables_name[2], LQ.GetFirstFreeID(Tables_name[2], LQ.GetAllColumnNames(Tables_name[2], cnn)[0], cnn));
            foreach (DataGridViewRow r in dgv_Tabella.Rows)
            {
                names.Add(r.Cells[1].Value.ToString());
            }
        }
        private void txb_searchBar_TextChanged(object sender, EventArgs e) //barra di ricerca per nome
        {
            string query = $"SELECT * FROM {Tables_name[0]} WHERE Name LIKE @searchText + '%'";
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam(query, new SqlParameter("@searchText", txb_searchBar.Text), cnn).Tables[0];
        }
        private void txb_SearchId_TextChanged(object sender, EventArgs e) //barra di ricerca per id
        {
            string query = $"SELECT * FROM {Tables_name[0]} WHERE ID_Queries LIKE @searchId + '%'";
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam(query, new SqlParameter("@searchId", txb_SearchId.Text), cnn).Tables[0];
        }
        private void dgv_Tabella_SelectionChanged(object sender, EventArgs e) //disattivazione e attivazione pulsante duplicazione
        {
            if (dgv_Tabella.SelectedRows.Count > 0)
            {
                btn_Duplicate.Enabled = true;
            }
            else
            {
                btn_Duplicate.Enabled = false;
            }
        }
        private void btn_Create_Form(object sender, EventArgs e) //metodo condiviso fra i pulsanti create new e duplicate
        {
            Button btnCreate = sender as Button;
            bool existing = btnCreate.Text == "Duplicate";
            row = dgv_Tabella.Rows[0];
            if (existing)
            {
                row = dgv_Tabella.Rows[dgv_Tabella.SelectedRows[0].Index];
            }
            frm_Querie = new Form //Creazione Form
            {
                Text = $"{dgv_Tabella.Columns[0].HeaderText} = {FreeIdTables[Tables_name[0]]}",
                Size = new Size(750, 50 + 30 * row.Cells.Count),
                StartPosition = FormStartPosition.CenterParent
            };

            int y = 10;
            for (int i = 1; i < row.Cells.Count; i++)
            {
                Label lbl = new Label //Creazione LAbel
                {
                    Text = dgv_Tabella.Columns[i].HeaderText,
                    Location = new Point(10, y + 3),
                    AutoSize = true
                };
                frm_Querie.Controls.Add(lbl);
                if (row.Cells[i].ValueType == typeof(bool)) //Controllo tipologia dati
                {
                    ComboBox cb = new ComboBox //Creazione Combobox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(170, y),
                        Width = 240
                    };
                    cb.Items.Add("True");
                    cb.Items.Add("False");
                    if (existing)
                    {
                        cb.SelectedIndex = row.Cells[i].Value != null && (bool)row.Cells[i].Value ? 0 : 1;
                    }
                    frm_Querie.Controls.Add(cb);
                }
                else
                {
                    TextBox txt = new TextBox //Creazione TextBox
                    {
                        Text = "",
                        Location = new Point(170, y),
                        Width = 240,
                    };
                    if (existing)
                    {
                        txt.Text = row.Cells[i].Value.ToString();
                    }
                    frm_Querie.Controls.Add(txt);
                }

                y = y + 30;
            }
            DataGridView dgv1 = new DataGridView //Creazione datagridview per tabella Query_CrossModules
            {
                Tag = "Queries_CrossModules",
                Location = new Point(430, 10),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQWithParam($"SELECT ID_Module, [Order] FROM Queries_CrossModules WHERE ID_Queries = @Id", new SqlParameter("@Id", FreeIdTables[Tables_name[0]]), cnn).Tables[0]

            };
            dgv1.DoubleClick += ModuleCreation;
            dgv1.KeyDown += (s, ev) => RowElimination(s, ev, dgv1); //Creazione datagridview per tabella Query_parameters
            frm_Querie.Controls.Add(dgv1);
            DataGridView dgv2 = new DataGridView
            {
                Tag = "Queries_Parameter",
                Location = new Point(430, frm_Querie.Height / 2 - 30),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQWithParam($"SELECT Name, Description, Id_Queries_Parameter_Type, [Order], Id_Queries_Parameter_Relation, Active, Mandatory FROM Queries_Parameter WHERE ID_Queries = @Id", new SqlParameter("@Id", FreeIdTables[Tables_name[0]]), cnn).Tables[0]

            };
            dgv2.DoubleClick += ModuleCreation;
            dgv2.KeyDown += (s, ev) => RowElimination(s, ev, dgv2);
            frm_Querie.Controls.Add(dgv2);

            Button btn_Confirm = new Button //pulsante per la creazione del file.sql
            {
                Text = "Confirm",
                Location = new Point(10, frm_Querie.Height - 75),
                Width = 710,
            };
            btn_Confirm.Click += btn_Confirm_onClick;
            frm_Querie.Controls.Add(btn_Confirm);
            frm_Querie.ShowDialog(this);

        }
        private void btn_Confirm_onClick(object sender, EventArgs e) //metodo creazione file.sql
        {
            bool correct = true;
            List<string> columnNames = new List<string>();
            List<string> values = new List<string>();
            List<string> queriesM = new List<string>();
            columnNames.Add(dgv_Tabella.Columns[0].HeaderText);
            List<string> s = LQ.GetRequiredColumns(Tables_name[0], cnn);
            s.RemoveAt(0);
            for (int i = 0; i < frm_Querie.Controls.Count; i++) //recupero dati inseriti
            {
                if (frm_Querie.Controls[i] is Label lbl) //Label
                {
                    if (lbl.Text == "Order")
                    {
                        columnNames.Add("[" + lbl.Text + "]");
                    }
                    else
                    {
                        columnNames.Add(lbl.Text);
                    }
                }

                else if (frm_Querie.Controls[i] is TextBox txt) //TextBox
                {
                    if (txt.Text != "")
                    {
                        values.Add("'" + txt.Text + "'");
                    }
                    else if (s.Contains(columnNames[columnNames.Count - 1]))
                    {
                        correct = false;
                    }
                    else
                    {
                        columnNames.RemoveAt(columnNames.Count - 1);
                    }
                }

                else if (frm_Querie.Controls[i] is ComboBox cbx) //Combobox
                {
                    if (cbx.Text != "")
                    {
                        values.Add("'" + cbx.Text + "'");
                    }
                    else if (s.Contains(columnNames[columnNames.Count - 1]))
                    {
                        correct = false;
                    }
                    else
                    {
                        columnNames.RemoveAt(columnNames.Count - 1);
                    }
                }
                else if (frm_Querie.Controls[i] is DataGridView dgv) //Datagridview
                {
                    string columns2 = string.Join(", ", LQ.GetAllColumnNames(dgv.Tag.ToString(), cnn));
                    for (int y = 0; y < dgv.Rows.Count - 1; y++)
                    {
                        string parameters2 = "";
                        for (int k = 0; k < dgv.Rows[y].Cells.Count; k++)
                        {
                            parameters2 += ", '" + dgv.Rows[y].Cells[k].Value + "'";
                        }
                        string qM = $"INSERT INTO [{Database_name}].[dbo].{dgv.Tag.ToString()} ({columns2}) VALUES ({FreeIdTables[dgv.Tag.ToString()]}, '{FreeIdTables[Tables_name[0]]}'{parameters2})";
                        queriesM.Add(qM);
                        FreeIdTables[dgv.Tag.ToString()]++;
                    }
                }
            }
            string control = values[0].Replace("'","");
            if (names.Contains(control))
            {
                correct = false;
            }
            else
            {
                names.Add(control);
            }

            if (correct) //composizione e creazione file.sql
            {
                string columns = string.Join(", ", columnNames);
                string parameters = FreeIdTables[Tables_name[0]].ToString() + ", " + string.Join(", ", values);
                string queriesM1 = string.Join("\n", queriesM);
                string query = $"INSERT INTO [{Database_name}].[dbo].{Tables_name[0]} ({columns}) VALUES ({parameters})\n" + queriesM1;
                values[0] = values[0].Replace(" ","_");
                string name = null;
                foreach (char c in values[0])
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                    {
                        name += c;
                    }
                }

                using (StreamWriter sw = new StreamWriter(name + ".SQL"))
                {
                    sw.WriteLine(query);
                }
                MessageBox.Show("SQL creata con successo");
                FreeIdTables[Tables_name[0]]++;

                frm_Querie.Close();
            }
            else //message di errore
            {
                string sd = string.Join(", ", s);
                MessageBox.Show($"Name must be new and {sd} are required");
            }

        }

        private void ModuleCreation(object sender, EventArgs e) //modulo per inserimento Queries_CrossModules e Queries_Parameters
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv != null)
            {
                Form frm_Module = new Form //Form
                {
                    Text = $"{dgv.Columns[0].HeaderText} = {FreeIdTables[Tables_name[0]]}",
                    Size = new Size(450, 100 + 30 * dgv.Rows[0].Cells.Count),
                    StartPosition = FormStartPosition.CenterParent
                };
                int y = 10;
                List<string> fkColumns = LQ.GetForeignKeyColumns(dgv.Tag.ToString(), cnn);
                for (int i = 0; i < dgv.Rows[0].Cells.Count; i++)
                {
                    Label lbl = new Label //Label
                    {
                        Text = dgv.Columns[i].HeaderText,
                        Location = new Point(10, y + 3),
                        AutoSize = true
                    };
                    frm_Module.Controls.Add(lbl);
                    if (fkColumns.Contains(dgv.Columns[i].HeaderText)) //Controllo campi associati a tabelle esterne
                    {
                        ComboBox cb = new ComboBox //Combobox campi esterni
                        {
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Location = new Point(170, y),
                            Width = 240
                        };

                        Tuple<string, string> refInfo = LQ.GetReferencedTableAndColumn(dgv.Tag.ToString(), dgv.Columns[i].HeaderText, cnn);
                        string query = $"SELECT {refInfo.Item2} FROM {refInfo.Item1}";
                        DataSet ds = LQ.ExecuteQ(query, cnn);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            cb.Items.Add(dr[0].ToString());
                        }
                        frm_Module.Controls.Add(cb);
                    }
                    else
                    {
                        if (dgv.Rows[0].Cells[i].ValueType == typeof(bool))
                        {
                            ComboBox cb = new ComboBox //combobox 
                            {
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Location = new Point(170, y),
                                Width = 240
                            };
                            cb.Items.Add("True");
                            cb.Items.Add("False");
                            frm_Module.Controls.Add(cb);
                        }
                        else
                        {
                            TextBox txt = new TextBox //textbox
                            {
                                Text = "",
                                Location = new Point(170, y),
                                Width = 240,
                            };
                            frm_Module.Controls.Add(txt);
                        }
                    }   
                    y = y + 30;
                }
                Button btn_ConfirmM = new Button //pulsante conferma
                {
                    Text = "Confirm",
                    Location = new Point(10, frm_Module.Height - 75),
                    Width = 410,
                };
                btn_ConfirmM.Click += (s, ev) => btn_ConfirmM_onClick(s, ev, frm_Module, dgv);
                frm_Module.Controls.Add(btn_ConfirmM);
                frm_Module.ShowDialog();
            }
        }
        private void RowElimination(object sender, KeyEventArgs e, DataGridView dgv) //Eliminazione Riga aggiunta
        {
            if (dgv.SelectedRows.Count > 0 && e.KeyCode == Keys.Back)
            {
                DataTable dt = dgv.DataSource as DataTable;
                dt.Rows.RemoveAt(dgv.SelectedRows[0].Index);
                dgv.DataSource = dt;
            }
        }

        private void btn_ConfirmM_onClick(object sender, EventArgs e, Form frm, DataGridView dgv) //conferma inserimento querie associata
        {
            DataTable dt = dgv.DataSource as DataTable;
            DataRow rowt = dt.NewRow();
            bool correct = true;
            int j = 0;
            for (int i = 0; i < frm.Controls.Count; i++)
            {
                if (frm.Controls[i] is TextBox txt)
                {
                    if (txt.Text != "")
                    {
                        rowt[j] = txt.Text;
                        j++;
                    }
                    else
                    {
                        correct = false;
                    }
                }
                else if (frm.Controls[i] is ComboBox cbx)
                {
                    if (cbx.Text != "")
                    {
                        rowt[j] = cbx.Text;
                        j++;
                    }
                    else
                    {
                        correct = false;
                    }
                }
            }
            if (correct)
            {
                dt.Rows.Add(rowt);
                dgv.DataSource = dt;
            }
            else
            {
                MessageBox.Show("Compilazione errata");
            }
        }
    }
}
