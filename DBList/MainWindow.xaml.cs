using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

namespace DBList
{
    //TODO
    //Защита от дурака
    //DataGrid с текущими условиями
    //Row affected при update
    //отлов ошибок
    //

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string _tableCatalog = "",
            _tableSchema = "",//dbo
            _tableName = "",
            _columnName = "",
            _dataType = "";
        private static DataTable allColumnsSchemaTable = new DataTable();
        private static byte[] byteData;
        public MainWindow()
        {
            InitializeComponent();
            tablesLabel.Visibility = Visibility.Hidden;
            tableBox.Visibility = Visibility.Hidden;
            fieldLabel.Visibility = Visibility.Hidden;
            fieldBox.Visibility = Visibility.Hidden;
            labelBinary.Visibility = Visibility.Hidden;
            binList.Visibility = Visibility.Hidden;
            dbListBox.Visibility = Visibility.Hidden;
            labelList.Visibility = Visibility.Hidden;
        }

        private void button_Click(object sender, RoutedEventArgs e)
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
            textField.Text = "";

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


        //отобразить все поля binary в БД
        /*  private static Dictionary<string, string> ShowColumns(DataTable columnsTable, string tableCatalog)
         {
             Dictionary<string, string> colsTypes = new Dictionary<string, string>();
             var selectedRows = from info in columnsTable.AsEnumerable()
                                select new
                                {
                                    TableCatalog = info["TABLE_CATALOG"],//ExchRates 
                                    TableSchema = info["TABLE_SCHEMA"],//dbo
                                    TableName = info["TABLE_NAME"],
                                    ColumnName = info["COLUMN_NAME"],
                                    DataType = info["DATA_TYPE"]
                                };
             foreach (var row in selectedRows)
             {
                 if ((row.TableCatalog.ToString() == tableCatalog) && (row.ColumnName != null) && (row.DataType != null) && (!colsTypes.ContainsKey(row.ColumnName.ToString())) && ((row.DataType.ToString() == "varbinary") || (row.DataType.ToString() == "binary")))
                 {
                     colsTypes.Add(row.ColumnName.ToString(), row.DataType.ToString());
                     _tableCatalog = row.TableCatalog.ToString();
                     _tableSchema = row.TableSchema.ToString();
                     _tableName = row.TableName.ToString();
                 }
             }
             return colsTypes;
         }

        private static Dictionary<string, string> ShowColumns(DataTable columnsTable, string tableCatalog, string tableSchema, string tableName)
         {
             Dictionary<string, string> colsTypes = new Dictionary<string, string>();
             var selectedRows = from info in columnsTable.AsEnumerable()
                                select new
                                {
                                    TableCatalog = info["TABLE_CATALOG"],//ExchRates 
                                    TableSchema = info["TABLE_SCHEMA"],//dbo
                                    TableName = info["TABLE_NAME"],
                                    ColumnName = info["COLUMN_NAME"],
                                    DataType = info["DATA_TYPE"]
                                };
             foreach (var row in selectedRows)
             {
                 if ((row.TableSchema.ToString() == tableSchema) && (row.TableName.ToString() == tableName) && (row.TableCatalog.ToString() == tableCatalog) && (row.ColumnName != null) && (row.DataType != null) && (!colsTypes.ContainsKey(row.ColumnName.ToString())) && ((row.DataType.ToString() != "varbinary") || (row.DataType.ToString() != "binary")))
                 {
                     colsTypes.Add(row.ColumnName.ToString(), row.DataType.ToString());
                     _tableCatalog = row.TableCatalog.ToString();
                     _tableSchema = row.TableSchema.ToString();
                     _tableName = row.TableName.ToString();
                 }
             }
             return colsTypes;
         }*/
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
                    if (col.DataType.Equals(typeof(DateTime)))
                        tmp.Append("{0,-" + length + ":d}" + row[col]);
                    else if (col.DataType.Equals(typeof(Decimal)))
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
            //if (binList.SelectedItem != null)
            //{
            //    StringBuilder query = new StringBuilder();
            //    query.Append("SELECT " + binList.SelectedItem.ToString() + " FROM [" + _tableCatalog + "]." + "[" + _tableSchema + "].[" + _tableName + "] WHERE ");
            //    Dictionary<string, string> qrs = new Dictionary<string, string>();
            //    qrs = FieldsClass.GetSelected();
            //    int counter = 0;
            //    foreach (var item in qrs)
            //    {
            //        counter++;
            //        query.Append(item.Key + " = '" + item.Value + "'");
            //        if (counter != qrs.Count)
            //            query.Append(" AND ");
            //    }
            //    MessageBox.Show(query.ToString());
            //    string strData = "";
            //    using (SqlConnection sqlConn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _tableCatalog))
            //    {
            //        try
            //        {
            //            var sqlCmd = new SqlCommand(query.ToString(), sqlConn);
            //            sqlConn.Open();
            //            var reader = sqlCmd.ExecuteReader();
            //            bool success = false;
            //            StringBuilder table = new StringBuilder();
            //            if (reader.HasRows)
            //            {
            //                int fieldCount = reader.FieldCount;
            //                table.Append("|   ");
            //                table.AppendLine("|");
            //                table.AppendLine("-----------------");
            //                string result = "";
            //                while (reader.Read())
            //                {
            //                    table.Append("|   ");
            //                    for (int i = 0; i < fieldCount; i++)
            //                        if (reader.GetValue(i) != null)
            //                        {
            //                            if (reader.GetValue(i) is byte[])
            //                            {
            //                                byteData = (byte[])reader.GetValue(i);
            //                                result = Convert.ToBase64String(byteData, 0, byteData.Length);
            //                                success = true;
            //                            }
            //                            if (success)
            //                                table.Append(result);
            //                            else
            //                                table.Append(reader.GetValue(i) + "   ");
            //                        }
            //                        else
            //                            table.Append("null   ");
            //                    table.AppendLine("|");
            //                }
            //            }
            //            MessageBox.Show(table.ToString());
            //        }
            //        catch (Exception error)
            //        {
            //            MessageBox.Show(error.ToString());
            //        }
            //        finally
            //        {
            //            sqlConn.Close();
            //        }
            //    }
            //}
            //else
            //    MessageBox.Show("Не выбрано бинарное поле");
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (binList.SelectedItem != null)
            {
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
                string strData = "";
                using (SqlConnection sqlConn = new SqlConnection(connectionBox.Text + "Initial Catalog =" + _tableCatalog))
                {
                    try
                    {
                        var sqlCmd = new SqlCommand(query.ToString(), sqlConn);
                        sqlConn.Open();
                        var reader = sqlCmd.ExecuteReader();
                        bool success = false;
                        StringBuilder table = new StringBuilder();
                        if (reader.HasRows)
                        {
                            int fieldCount = reader.FieldCount;
                            table.Append("|   ");
                            table.AppendLine("|");
                            table.AppendLine("-----------------");
                            string result = "";
                            while (reader.Read())
                            {
                                table.Append("|   ");
                                for (int i = 0; i < fieldCount; i++)
                                    if (reader.GetValue(i) != null)
                                    {
                                        if (reader.GetValue(i) is byte[])
                                        {
                                            byteData = (byte[])reader.GetValue(i);
                                            result = Convert.ToBase64String(byteData, 0, byteData.Length);
                                            success = true;
                                        }
                                        if (success)
                                            table.Append(result);
                                        else
                                            table.Append(reader.GetValue(i) + "   ");
                                    }
                                    else
                                        table.Append("null   ");
                                table.AppendLine("|");
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
            //открыть окно с чтением файла с ПК
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; //Default file name
            dlg.DefaultExt = ""; //Default file extension
            dlg.Filter = ""; //Filter files by extension
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

                    string str = Convert.ToBase64String(array, 0, array.Length);//'" + str + "'
                    query.Append("UPDATE [" + _tableCatalog + "]." + "[" + _tableSchema + "].[" + _tableName + "] SET [" + binList.SelectedItem.ToString() + "] = @binaryValue  WHERE ");
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
                    cmd.Parameters.Add("@binaryValue", SqlDbType.VarBinary, array.Count()).Value = array;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
                finally { conn.Close(); }
            }
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
            textField.Text = "";
            fieldBox.Items.Clear();
            binList.Items.Clear();
            tableBox.Items.Clear();
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
                    {
                        textField.Text += cur + Environment.NewLine;
                        tableBox.Items.Add(cur);
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



        private void extraFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //здесь должно открываться окно для редактирования значений
            if (fieldBox.SelectedItem != null)
            {
                if (!FieldsClass.IsField(fieldBox.SelectedItem.ToString()))
                    FieldsClass.AddValue(fieldBox.SelectedItem.ToString(), "");
                UpdateWindow a = new UpdateWindow(fieldBox.SelectedItem.ToString());
                a.Show();
            }
        }
    }
}
