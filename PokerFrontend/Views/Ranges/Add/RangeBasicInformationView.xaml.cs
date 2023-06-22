using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace PokerFrontend.Views.Ranges.Add
{
    /// <summary>
    /// Interaction logic for RangeBasicInformationView.xaml
    /// </summary>
    public partial class RangeBasicInformationView : UserControl
    {
        public RangeBasicInformationView()
        {
            InitializeComponent();
        }

      
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void NumberValidationDecimalTextBox(object sender, TextCompositionEventArgs e)
        {
           var result =  e.Text.Select(x => int.TryParse(x.ToString(), out var s) || x == '.');
           e.Handled = result.Any(x => x == false) && e.Text.Count(x => x== '.') <2;
        }
    }
}
