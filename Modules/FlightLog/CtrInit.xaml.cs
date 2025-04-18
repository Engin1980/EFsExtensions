using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.FlightLogModule;
using ESystem.Exceptions;
using ESystem.Miscelaneous;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private readonly InitContext Context = null!;
    private readonly Settings settings = null!;

    public CtrInit()
    {
      InitializeComponent();
    }

    public CtrInit(InitContext context, Settings settings) : this()
    {
      this.DataContext = this.Context = context;
      this.settings = settings;
    }

    private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (tabMain.SelectedIndex == 1)
        lblTakeLong.Visibility = Visibility.Collapsed;
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      new CtrSettings(this.settings).ShowDialog();
    }

    private void ctrNewProfile_Click(object sender, RoutedEventArgs e)
    {
      var box = new InputBox("Enter new profile name", "New profile", "Profile",
        validator: q => IsValidDictionaryName(q) && !Context.Profiles.Any(p => p.Name == q),
        validationErrorMessage: "Profile name already exists or is not valid.");
      box.ShowDialog();
      if (box.DialogResult != true) return;

      try
      {
        Context.CreateProfile(box.Input ?? throw new UnexpectedNullException());
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to create profile. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    static bool IsValidDictionaryName(string input)
    {
      return !string.IsNullOrEmpty(input) && input.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
  }
}
