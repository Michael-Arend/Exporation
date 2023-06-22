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
using Microsoft.WindowsAPICodePack.Dialogs;
using PokerFrontend.ViewModel.Ranges.Add;

namespace PokerFrontend.Views.Ranges.Add
{
    /// <summary>
    /// Interaction logic for RangeActionView.xaml
    /// </summary>
    public partial class RangeActionView : UserControl
    {
        public RangeActionView()
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
            var result = e.Text.Select(x => int.TryParse(x.ToString(), out var s) || x == '.');
            e.Handled = result.Any(x => x == false) && e.Text.Count(x => x == '.') < 2;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
           
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var vm = (RangeActionViewModel)this.DataContext;
                vm.FolderSelectedCommand.Execute(dialog.FileName);
            }
        }
    }
}
