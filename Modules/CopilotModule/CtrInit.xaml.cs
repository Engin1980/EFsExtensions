﻿using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Storable;
using Eng.EFsExtensions.Modules.CopilotModule;
using Eng.EFsExtensions.Modules.CopilotModule.Types;
using ESystem.Exceptions;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace Eng.EFsExtensions.Modules.CopilotModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private const string AUDIO_CHANNEL_NAME = AudioPlayManager.CHANNEL_COPILOT;
    private readonly InitContext context;
    private string recentXmlFile = "";
    private readonly AudioPlayManager autoPlaybackManager = AudioPlayManagerProvider.Instance;

    public CtrInit()
    {
      InitializeComponent();
      this.context = null!;
    }

    internal CtrInit(InitContext initContext) : this()
    {
      this.context = initContext;
      this.DataContext = context;
    }

    private void btnLoadChecklistFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentXmlFile,
        Multiselect = false,
        Title = "Select XML file with copilot speeches data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Copilot files", "copilot.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      recentXmlFile = dialog.FileName;
      this.context.LoadFile(recentXmlFile);
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      new CtrSettings(context.Settings).ShowDialog();
    }
    private void pnlSpeech_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      StackPanel panel = (StackPanel)sender;
      SpeechDefinition sd = (SpeechDefinition)panel.Tag;
      autoPlaybackManager.Enqueue(sd.Speech.Bytes, AUDIO_CHANNEL_NAME);
    }
  }
}
