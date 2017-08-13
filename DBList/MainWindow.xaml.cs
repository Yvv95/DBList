using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace DBList
{
    //Data Source=.\SQLEXPRESS; Integrated Security=True

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string _tableCatalog = "",
            _tableSchema = "",//dbo
            _tableName = ""
;

        private static DataTable allColumnsSchemaTable = new DataTable();
        private static byte[] byteData;
        public MainWindow()
        {
            InitializeComponent();
            condGrid.ItemsSource = FieldsClass.fields;
            HideInfo();
        }

        public void HideInfo()
        {
            tablesLabel.Visibility = Visibility.Hidden;
            tableBox.Visibility = Visibility.Hidden;
            fieldLabel.Visibility = Visibility.Hidden;
            labelBinary.Visibility = Visibility.Hidden;
            fieldBox.Visibility = Visibility.Hidden;
            binList.Visibility = Visibility.Hidden;
            dbListBox.Visibility = Visibility.Hidden;
            labelList.Visibility = Visibility.Hidden;
            textError.Visibility = Visibility.Hidden;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            HideInfo();
            using (SqlConnection conn = new SqlConnection(connectionBox.Text))
            {
                try
                {
                    conn.Open();
                    string fieldBox = "";
                    DataTable metaDataTable = conn.GetSchema("MetaDataCollections");
                    List<string> b = new List<string>();
                    foreach (DataColumn col in metaDataTable.Columns)
                        b.Add(metaDataTable + "-" + col.ColumnName);

                    fieldBox = ShowDataTable(metaDataTable, 1);

                    DataTable databasesSchemaTable = conn.GetSchema("Databases");
                    List<string> c = new List<string>();

                    c = ShowCols(databasesSchemaTable);
                    dbListBox.ItemsSource = c;
                    fieldBox += ShowDataTable(databasesSchemaTable, 2);

                    DataTable allTablesSchemaTable = conn.GetSchema("Tables");
                    List<string> d = new List<string>();
                    foreach (DataColumn col in allTablesSchemaTable.Columns)
                        d.Add(allTablesSchemaTable + "-" + col.ColumnName);

                    fieldBox += ShowDataTable(allTablesSchemaTable, 3);
                    textBox.Text = fieldBox;
                    dbListBox.Visibility = Visibility.Visible;
                    labelList.Visibility = Visibility.Visible;
                }
                catch (Exception err)
                {
                    textError.Visibility = Visibility.Visible;
                    MessageBox.Show(err.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private static List<string> ShowCols(DataTable table, string field)
        {
            List<string> dbs = new List<string>();
            var selectedRows = from info in table.AsEnumerable()
                               select new
                               {
                                   TableName = info[field],
                                   TableSchema = info["TABLE_SCHEMA"]
                               };
            foreach (var row in selectedRows)
            {
                if (row.TableName != null)
                    dbs.Add(row.TableName.ToString());
                _tableSchema = row.TableSchema.ToString();

            }
            return dbs;
        }
        private static List<string> ShowCols(DataTable table)
        {
            List<string> dbs = new List<string>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] != null)
                    dbs.Add(table.Rows[i][0].ToString());
            }
            return dbs;
        }

        private static string ShowDataTable(DataTable table, Int32 length)
        {
            StringBuilder tmp = new StringBuilder();
            foreach (DataColumn col in table.Columns)
                tmp.Append("{0,-" + length + "}" + col.ColumnName);

            tmp.AppendLine();
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (col.DataType == typeof(DateTime))
                        tmp.Append("{0,-" + length + ":d}" + row[col]);
                    else if (col.DataType == typeof(Decimal))
                        tmp.Append("{0,-" + length + ":C}" + row[col]);
                    else
                        tmp.Append("{0,-" + length + "}" + row[col]);
                }
                tmp.AppendLine();
            }
            return tmp.ToString();
        }

        private void buttonShow_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("Catalog: " + _tableCatalog);
            str.AppendLine("Schema: " + _tableSchema);
            str.AppendLine("Table: " + _tableName);
            str.AppendLine("--------");
            foreach (var item in FieldsClass.GetSelected())
                str.AppendLine(item.Key + ": " + item.Value);
            if (binList.SelectedItem != null)
                str.AppendLine("Бин. поле: " + binList.SelectedItem.ToString());
            else
                str.AppendLine("Бин. поле: не выбрано");
            MessageBox.Show(str.ToString());
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (binList.SelectedItem != null)
            {
                byteData = null;
                StringBuilder query = new StringBuilder();
                query.Append("SELECT " + binList.SelectedItem.ToString() + " FROM [" + _tableCatalog + "]." + "[" + _tableSchema + "].[" + _tableName + "] WHERE ");
                Dictionary<string, string> qrs = new Dictionary<string, string>();
                qrs = FieldsClass.GetSelected();
                int counter = 0;
                foreach (var item in qrs)
                {
                    counter++;
                    query.Append(item.Key + " = '" + item.Value + "'");
                    if (counter != qrs.Count)
                        query.Append(" AND ");
                }
                MessageBox.Show(query.ToString());
                using (SqlConnection sqlConn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _tableCatalog))
                {
                    try
                    {
                        var sqlCmd = new SqlCommand(query.ToString(), sqlConn);
                        sqlConn.Open();
                        sqlCmd.StatementCompleted += (s, e2) => MessageBox.Show(e2.RecordCount + " строк выбрано");
                        var reader = sqlCmd.ExecuteReader();
                        StringBuilder table = new StringBuilder();
                        if (reader.HasRows)
                        {
                            int fieldCount = reader.FieldCount;
                            table.Append("|   ");
                            table.AppendLine("|");
                            table.AppendLine("-----------------");
                            while (reader.Read())
                            {
                                table.Append("|   ");
                                for (int i = 0; i < fieldCount; i++)
                                {
                                    if (reader.GetValue(i) != null)
                                    {
                                        if (reader.GetValue(i) is byte[])
                                            byteData = (byte[]) reader.GetValue(i);
                                        table.Append(reader.GetValue(i) + "   ");
                                    }
                                    else
                                        table.Append("null   ");
                                    table.AppendLine("|");
                                }
                            }
                        }
                        MessageBox.Show(table.ToString());
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString());
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "";
                saveFileDialog1.Title = "Сохранение файла на компьютер";
                if (byteData == null)
                    MessageBox.Show("Выбранная запись не содержит бинарных данных");
                else
            if (saveFileDialog1.ShowDialog() == true)
                    File.WriteAllBytes(saveFileDialog1.FileName, byteData);
                else
                    MessageBox.Show("Не удалось сохранить");
            }
            else
                MessageBox.Show("Не выбрано бинарное поле");
        }

        private void uplBtn_Click(object sender, RoutedEventArgs e)
        {
            if (binList.SelectedItem != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = "Изменить файл в базе данных";
                dlg.Filter = "";
                StringBuilder query = new StringBuilder();
                byte[] array = null;
                //Show open file dialog box
                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    string filename = dlg.FileName;
                    using (FileStream fstream = File.OpenRead(filename))
                    {
                        // преобразуем строку в байты
                        array = new byte[fstream.Length];
                        //считываем данные
                        fstream.Read(array, 0, array.Length);

                        query.Append("UPDATE [" + _tableCatalog + "]." + "[" + _tableSchema + "].[" + _tableName +
                                     "] SET [" + binList.SelectedItem.ToString() + "] = @binaryValue  WHERE ");
                        Dictionary<string, string> qrs = new Dictionary<string, string>();
                        qrs = FieldsClass.GetSelected();
                        int counter = 0;
                        foreach (var item in qrs)
                        {
                            counter++;
                            query.Append(item.Key + " = '" + item.Value + "'");
                            if (counter != qrs.Count)
                                query.Append(" AND ");
                        }
                        MessageBox.Show(query.ToString());
                    }
                }
                using (SqlConnection conn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _tableCatalog))
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                        cmd.StatementCompleted += (s, e2) => MessageBox.Show(e2.RecordCount + " строк изменено");
                        cmd.Parameters.Add("@binaryValue", SqlDbType.VarBinary, array.Count()).Value = array;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
                MessageBox.Show("Не выбрано бинарное поле");
        }

        private void tableBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //загружаем все поля выбранной таблицы
            if (tableBox.SelectedItem != null)
            {
                fieldLabel.Visibility = Visibility.Visible;
                fieldBox.Visibility = Visibility.Visible;
                labelBinary.Visibility = Visibility.Visible;
                binList.Visibility = Visibility.Visible;
                string _table = tableBox.SelectedItem.ToString();
                _tableName = _table;
                string _catalog = dbListBox.SelectedItem.ToString();
                FieldsClass.ClearList();
                condGrid.Items.Refresh();
                using (SqlConnection conn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _catalog))
                {
                    try
                    {
                        conn.Open();
                        string[] restrictions = new string[4];
                        restrictions[2] = _table;
                        DataTable table = conn.GetSchema("Tables", restrictions);
                        string sql = "SELECT * FROM " + _table + " WHERE 1 = 0";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        DataTable schema = reader.GetSchemaTable();
                        fieldBox.Items.Clear();
                        binList.Items.Clear();
                        for (int i = 0; i < schema.Rows.Count; i++)
                            if (schema.Rows[i][0] != null)
                            {
                                FieldsClass.AddValue(schema.Rows[i][0].ToString(), "");
                                fieldBox.Items.Add(schema.Rows[i][0].ToString());
                                binList.Items.Add(schema.Rows[i][0].ToString());
                            }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                fieldLabel.Visibility = Visibility.Hidden;
                labelBinary.Visibility = Visibility.Hidden;
                fieldBox.Visibility = Visibility.Hidden;
                binList.Visibility = Visibility.Hidden;
            }
        }

        private void dbListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fieldBox.Items.Clear();
            binList.Items.Clear();
            tableBox.Items.Clear();
            byteData = null;
            FieldsClass.ClearList();
            condGrid.Items.Refresh();
            tablesLabel.Visibility = Visibility.Visible;
            tableBox.Visibility = Visibility.Visible;
            ComboBox comboBox = (ComboBox)sender;
            string _table = comboBox.SelectedItem.ToString();
            _tableCatalog = _table;
            using (SqlConnection conn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _table))
            {
                try
                {
                    conn.Open();
                    allColumnsSchemaTable = conn.GetSchema("Tables");
                    List<string> tablesList = ShowCols(allColumnsSchemaTable, "TABLE_NAME");
                    foreach (string cur in tablesList)
                        tableBox.Items.Add(cur);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        private void extraFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fieldBox.SelectedItem != null)
            {
                if (!FieldsClass.IsField(fieldBox.SelectedItem.ToString()))
                    FieldsClass.AddValue(fieldBox.SelectedItem.ToString(), "");
                UpdateWindow a = new UpdateWindow(fieldBox.SelectedItem.ToString());
                a.Show();
                a.Closed += A_Closed;//для обновления таблицы
            }
        }
        private void A_Closed(object sender, EventArgs e)
        {
            condGrid.Items.Refresh();
        }
    }
}
