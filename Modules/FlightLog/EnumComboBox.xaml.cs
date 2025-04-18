using ESystem.Asserting;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for EnumComboBox.xaml
  /// </summary>
  public partial class EnumComboBox : UserControl
  {
    public record EnumComboBoxItem(string Display, Enum Value) { public override string ToString() => Display + " (X)"; }

    private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is EnumComboBox ecb)
        ecb.OnEnumTypeChanged();
    }

    private void OnEnumTypeChanged()
    {
      List<EnumComboBoxItem> vals;
      if (this.EnumType == null)
        vals = new();
      else
      {
        EAssert.IsTrue(this.EnumType.IsEnum, "EnumType must be an enum type");
        vals = Enum.GetValues(this.EnumType)
          .Cast<Enum>()
          .Select(q => new EnumComboBoxItem(ESystem.Enums.GetDisplayName(q), q))
          .OrderBy(q => q.Display)
          .ToList();
      }
      cmb.ItemsSource = vals;
      cmb.SelectedItem = null;
    }

    private static readonly DependencyProperty EnumTypeProperty =
      DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumComboBox), 
        new PropertyMetadata(null, OnEnumTypeChanged));
    public Type? EnumType
    {
      get => (Type?)GetValue(EnumTypeProperty);
      set => SetValue(EnumTypeProperty, value);
    }


    private static readonly DependencyProperty SelectedItemProperty =
      DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(EnumComboBox), 
        new FrameworkPropertyMetadata(null, OnSelectedItemChanged)
        {
           BindsTwoWayByDefault=true
        });

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is EnumComboBox ecb)
      {
        ecb.OnSelectedItemChanged();
      }
    }

    private void OnSelectedItemChanged()
    {
      object? val = this.SelectedItem;
      if (val == null)
        cmb.SelectedItem = null;
      else
      {
        EnumComboBoxItem? ecbi = cmb.Items
          .OfType<EnumComboBoxItem>()
          .FirstOrDefault(q => q.Value.Equals(val));
        cmb.SelectedItem = ecbi;
      }
    }

    public object? SelectedItem
    {
      get
      {
        var ret = (object?)GetValue(SelectedItemProperty);
        return ret;
      }

      set => SetValue(SelectedItemProperty, value);
    }

    public EnumComboBox()
    {
      InitializeComponent();
      this.cmb.SelectionChanged += (s, e) =>
      {
        if (this.cmb.SelectedItem == null)
          this.SelectedItem = null;
        else
          this.SelectedItem = ((EnumComboBoxItem)this.cmb.SelectedItem).Value;
      };
    }
  }
}
