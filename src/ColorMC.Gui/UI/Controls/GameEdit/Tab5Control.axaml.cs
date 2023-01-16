using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab5Control : UserControl
{
    private readonly List<WorldControl> List = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private WorldControl? Last;
    private AddWorldWindow? AddWorldWindow;

    public Tab5Control()
    {
        InitializeComponent();

        Button_A1.PointerLeave += Button_A1_PointerLeave;
        Button_A.PointerEnter += Button_A_PointerEnter;

        Button_I1.PointerLeave += Button_I1_PointerLeave;
        Button_I.PointerEnter += Button_I_PointerEnter;

        Button_R1.PointerLeave += Button_R1_PointerLeave;
        Button_R.PointerEnter += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_A1.Click += Button_A1_Click;
        Button_I1.Click += Button_I1_Click;

        LayoutUpdated += Tab5Control_LayoutUpdated1;
    }

    public void CloseAddWorld()
    {
        AddWorldWindow = null;
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        if (AddWorldWindow == null)
        {
            AddWorldWindow = new();
            AddWorldWindow.SetTab5Control(Obj, this);
            AddWorldWindow.Show();
        }
        else
        {
            AddWorldWindow.Activate();
        }
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = Localizer.Instance["GameEditWindow.Tab5.Info2"],
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "zip"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var res = await GameBinding.AddWorld(Obj, file[0]);
            if (!res)
            {
                Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
                return;
            }
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
            Load();
        }
    }

    private void Tab5Control_LayoutUpdated1(object? sender, EventArgs e)
    {
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    private void Button_I1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = false;
    }

    private void Button_I_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = true;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    public async void Export(WorldDisplayObj obj)
    {
        SaveFileDialog openFile = new()
        {
            Title = Localizer.Instance["GameEditWindow.Tab5.Info2"],
            DefaultExtension = ".zip"
        };

        var file = await openFile.ShowAsync(Window);
        if (!string.IsNullOrWhiteSpace(file))
        {
            //if (File.Exists(file) && await Window.Info.ShowWait(string.Format(
            //        Localizer.Instance["GameEditWindow.Tab5.Info3"], file)) == false)
            //{
            //    return;
            //}

            Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab5.Info5"]);
            bool error = false;
            try
            {
                await GameBinding.ExportWorld(obj.World, file);
            }
            catch (Exception e)
            {
                Logs.Error(Localizer.Instance["GameEditWindow.Tab5.Error1"], e);
                error = true;
            }
            Window.Info1.Close();
            if (error)
            {
                Window.Info.Show(Localizer.Instance["GameEditWindow.Tab5.Error1"]);
            }
            else
            {
                Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab5.Info4"]);
            }
        }
    }

    public async void AddWorld(CurseForgeObj.Data.LatestFiles data)
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab5.Info6"]);
        var res = await GameBinding.DownloadWorld(Obj, data);
        Window.Info1.Close();
        if (res)
        {
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab5.Info7"]);
            Load();
        }
        else
        {
            Window.Info.Show(Localizer.Instance["GameEditWindow.Tab5.Error2"]);
        }
    }

    public async void Delete(WorldDisplayObj obj)
    {
        var res = await Window.Info.ShowWait(
            string.Format(Localizer.Instance["GameEditWindow.Tab5.Info1"], obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info3"]);
        Load();
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void SetSelect(WorldControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab5.Info8"]);
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetWorlds(Obj);
        Window.Info1.Close();
        foreach (var item in res)
        {
            var con = new WorldControl();
            con.SetWindow(this);
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}