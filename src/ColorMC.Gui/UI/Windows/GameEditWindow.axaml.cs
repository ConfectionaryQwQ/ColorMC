using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class GameEditWindow : Window, IBaseWindow
{
    private bool switch1 = false;

    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();
    private readonly Tab6Control tab6 = new();
    private readonly Tab7Control tab7 = new();
    private readonly Tab8Control tab8 = new();
    private readonly Tab9Control tab9 = new();
    private readonly Tab10Control tab10 = new();
    private readonly Tab11Control tab11 = new();
    private readonly Tab12Control tab12 = new();

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();
    private CancellationTokenSource cancel = new();

    private int now;

    private GameSettingObj? Obj;

    Info3Control IBaseWindow.Info3 => Info3;

    Info1Control IBaseWindow.Info1 => Info1;

    Info4Control IBaseWindow.Info => Info;

    Info2Control IBaseWindow.Info2 => Info2;

    public GameEditWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;

        Closed += SettingWindow_Closed;
        Opened += GameEditWindow_Opened;

        App.PicUpdate += Update;

        Update();
        Activated += Window_Activated;
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
        }
        else if (e.Delta.Y < 0)
        {
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
        }
    }

    private void GameEditWindow_Opened(object? sender, EventArgs e)
    {
        tab1.Update();
    }

    public void SetType(int type)
    {
        switch (type)
        {

            case 1:
                Tabs.SelectedIndex = 3;
                break;

            case 2:
                Tabs.SelectedIndex = 2;
                break;

            case 3:
                Tabs.SelectedIndex = 4;
                break;

            case 5:
                Tabs.SelectedIndex = 10;
                break;

            case 6:
                Tabs.SelectedIndex = 11;
                break;
        }
    }

    public void ClearLog()
    {
        tab7.Clear();
    }

    public void Log(string? data)
    {
        if (data == null)
            return;

        tab7.Log(data);
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
        Head.Title = Title = string.Format(App.GetLanguage("GameEditWindow.Title"), obj.Name);

        tab1.SetGame(obj);
        tab2.SetGame(obj);
        tab3.SetGame(obj);
        tab4.SetGame(obj);
        tab5.SetGame(obj);
        tab6.SetGame(obj);
        tab7.SetGame(obj);
        tab8.SetGame(obj);
        tab9.SetGame(obj);
        tab10.SetGame(obj);
        tab11.SetGame(obj);
        tab12.SetGame(obj);
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                tab1.Update();
                break;
            case 1:
                Go(tab2);
                tab2.Update();
                break;
            case 2:
                Go(tab3);
                tab3.Update();
                break;
            case 3:
                Go(tab4);
                tab4.Update();
                break;
            case 4:
                Go(tab5);
                tab5.Update();
                break;
            case 5:
                Go(tab8);
                tab8.Update();
                break;
            case 6:
                Go(tab9);
                tab9.Update();
                break;
            case 7:
                Go(tab10);
                tab10.Update();
                break;
            case 8:
                Go(tab11);
                tab11.Update();
                break;
            case 9:
                Go(tab12);
                tab12.Update();
                break;
            case 10:
                Go(tab6);
                tab6.Update();
                break;
            case 11:
                Go(tab7);
                tab7.Update();
                break;
        }

        now = Tabs.SelectedIndex;
    }

    private void Go(UserControl to)
    {
        cancel.Cancel();
        cancel = new();
        Tabs.IsEnabled = false;

        if (!switch1)
        {
            content2.Content = to;
            App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex, cancel.Token);
        }
        else
        {
            content1.Content = to;
            App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex, cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    private void SettingWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.GameEditWindows.Remove(Obj!);

        if (App.LastWindow == this)
        {
            App.LastWindow = null;
        }
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
