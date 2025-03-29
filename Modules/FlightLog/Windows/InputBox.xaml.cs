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
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for InputBox.xaml
  /// </summary>
  public partial class InputBox : Window
  {
    private class ViewModel : NotifyPropertyChanged
    {
      private Func<string, bool> _Validator = q => true;
      public Func<string, bool> Validator
      {
        get => _Validator;
        set
        {
          this._Validator = value;
          CheckValidity();
        }
      }

      public string Title
      {
        get => base.GetProperty<string>(nameof(Title))!;
        set => base.UpdateProperty(nameof(Title), value);
      }

      public string Prompt
      {
        get => base.GetProperty<string>(nameof(Prompt))!;
        set => base.UpdateProperty(nameof(Prompt), value);
      }

      public string Input
      {
        get => base.GetProperty<string>(nameof(Input))!;
        set
        {
          base.UpdateProperty(nameof(Input), value);
          CheckValidity();
        }
      }

      private void CheckValidity()
      {
        if (this.Validator(this.Input))
        {
          this.IsValid = true;
          this.IsInvalid = false;
        }
        else
        {
          this.IsValid = false;
          this.IsInvalid = true;
        }
      }

      public bool IsValid
      {
        get => base.GetProperty<bool>(nameof(IsValid))!;
        set => base.UpdateProperty(nameof(IsValid), value);
      }

      public bool IsInvalid
      {
        get => base.GetProperty<bool>(nameof(IsInvalid))!;
        set => base.UpdateProperty(nameof(IsInvalid), value);
      }
    }

    private readonly ViewModel vm;

    public string? Input => this.vm.Input;

    public InputBox()
    {
      InitializeComponent();
      this.DataContext = this.vm = new ViewModel()
      {
        Title = "Not initialized",
        Prompt = "Not initialized",
        Input = "",
      };
    }

    public InputBox(string prompt, string title, string? defaultInput = null,
      bool isMultiLine = false,
      Func<string, bool>? validator = null, string? validationErrorMessage = "Value is not valid.") : this()
    {
      this.vm.Title = title;
      this.vm.Prompt = prompt;
      this.vm.Input = defaultInput ?? "";
      this.vm.Validator = validator ?? (q => true);

      this.lblError.Text = validationErrorMessage;

      if (isMultiLine)
      {
        this.txtInput.TextWrapping = TextWrapping.Wrap;
        this.txtInput.AcceptsReturn = true;
      }
      else
      {
        this.txtInput.TextWrapping = TextWrapping.NoWrap;
        this.txtInput.AcceptsReturn = false;
      }
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }
  }
}
