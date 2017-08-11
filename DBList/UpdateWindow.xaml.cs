using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DBList
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public string fName, fValue;
        public UpdateWindow(string _field)
        {
            InitializeComponent();
            fieldName.Content = _field;
            fName = _field;
            fValue = FieldsClass.LoadField(_field);
            fieldValue.Text = fValue;
        }

        private void rmvBtn_Click(object sender, RoutedEventArgs e)
        {
            FieldsClass.UnSelectValue(fName);
            this.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {

            FieldsClass.AddValue(fName, fieldValue.Text);
            this.Close();
        }
    }
}
