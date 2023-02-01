using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public class CustomWindowModel : INotifyPropertyChanged
{
    private string name;
    private string type;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get { return name; }
        set { name = value; NotifyPropertyChanged(); }
    }

    public string Type
    {
        get { return type; }
        set { type = value; NotifyPropertyChanged(); }
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class CustomWindow : Window
{
    private UIObj UI;
    private GameSettingObj? Obj;
    private LoginObj User;

    private CustomWindowModel CustomModel = new();

    private LaunchState Last;
    private Image? HeadImg;
    private readonly Bitmap bitmap;
    private ServerMotdControl? Motd;

    public CustomWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        this.Closed += CustomWindow_Closed;

        CoreMain.GameLaunch = GameLunch;
        CoreMain.GameDownload = GameDownload;

        App.PicUpdate += Update;
        App.UserEdit += App_UserEdit;

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Pic.user.png");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets!.Open(uri);

        bitmap = new Bitmap(asset);

        Update();
    }

    private void App_UserEdit()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var user = UserBinding.GetLastUser();
            if (user == null)
                return;

            if (HeadImg != null)
            {
                HeadImg.Source = bitmap;
                CustomModel.Type = Localizer.Instance["MainWindow.Control.Info1"];
                CustomModel.Name = Localizer.Instance["MainWindow.Control.Info2"];
            }
            else
            {
                CustomModel.Type = User.AuthType.GetName();
                CustomModel.Name = User.UserName;

                LoadHead();
            }
        });
    }

    private async void LoadHead()
    {
        if (HeadImg == null)
        {
            return;
        }
        if (User == null)
        {
            HeadImg.Source = bitmap;
            return;
        }

        var file = await GetSkin.DownloadSkin(User);
        if (file == null)
        {
            HeadImg.Source = bitmap;
            return;
        }

        var data = await ImageUtils.MakeHeadImage(file);
        if (file == null)
        {
            HeadImg.Source = bitmap;
            return;
        }

        data.Seek(0, SeekOrigin.Begin);
        HeadImg.Source = new Bitmap(data);
        data.Close();
    }

    private void CustomWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        CoreMain.GameLaunch = null;
        CoreMain.GameDownload = null;

        App.CustomWindow = null;
    }

    public async void Launch()
    {
        Info1.Show(Localizer.Instance["MainWindow.Launch"]);
        var res = await GameBinding.Launch(Obj, false);
        Info1.Close();
        if (res.Item1 == false)
        {
            switch (Last)
            {
                case LaunchState.LoginFail:
                    Info.Show(Localizer.Instance["MainWindow.Error1"]);
                    break;
                case LaunchState.JvmError:
                    Info.Show(Localizer.Instance["MainWindow.Error2"]);
                    break;
                default:
                    Info.Show(res.Item2!);
                    break;
            }
        }
        else
        {
            Info2.Show("已启动");
        }
    }

    public void Load(UIObj ui)
    {
        UI = ui;

        if (ui.Views == null)
            return;

        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null)
        {
            return;
        }

        Grid2.Children.Clear();

        Obj = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
        if (Obj == null)
        {
            Grid2.Children.Add(new Label()
            {
                Content = "没有锁定的实例，请联系服务器管理员",
                Foreground = Brushes.Black,
                Background = Brush.Parse("#EEEEEE"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return;
        }

        HeadImg = null;

        Head.Title = UI.Title;

        foreach (var item in ui.Views)
        {
            var obj = MakeItem(item);
            if (obj != null)
            {
                Grid2.Children.Add(obj);
            }
        }

        App_UserEdit();

        Motd?.Load(config.Item2.ServerCustom!.IP, config.Item2.ServerCustom.Port);
    }

    private void MakeItems(Panel panel, ViewObj ui)
    {
        foreach (var item in ui.Views)
        {
            var obj = MakeItem(item);
            if (obj != null)
            {
                panel.Children.Add(obj);
            }
        }
    }

    private Control? MakeItem(ViewObj obj)
    {
        ViewType type;
        Control con;
        switch (obj.Type)
        {
            case "StackPanel":
                con = new StackPanel();
                type = ViewType.StackPanel;
                break;
            case "Grid":
                con = new Grid();
                type = ViewType.Grid;
                break;
            case "Button":
                con = new Button();
                type = ViewType.Button;
                break;
            case "Label":
                con = new Label();
                type = ViewType.Label;
                break;
            case "ServerMotd":
                con = new ServerMotdControl();
                type = ViewType.ServerMotd;
                Motd = con as ServerMotdControl;
                break;
            case "GameItem":
                con = new GameControl();
                type = ViewType.GameItem;
                break;
            case "UsearHead":
                con = new Image
                {
                    Width = 80,
                    Height = 80
                };
                type = ViewType.UsearHead;
                HeadImg = con as Image;
                break;
            default:
                return null;
        }

        if (!string.IsNullOrWhiteSpace(obj.VerticalAlignment))
        {
            if (Enum.TryParse(typeof(VerticalAlignment), obj.VerticalAlignment,
                true, out var arg))
            {
                con.VerticalAlignment = (VerticalAlignment)arg;
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.HorizontalAlignment))
        {
            if (Enum.TryParse(typeof(HorizontalAlignment), obj.HorizontalAlignment,
                true, out var arg))
            {
                con.HorizontalAlignment = (HorizontalAlignment)arg;
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.Content))
        {
            if (type == ViewType.Button || type == ViewType.Label)
            {
                (con as ContentControl).Content = obj.Content;
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.Funtion))
        {
            if (type == ViewType.Button)
            {
                (con as Button).Click += (s, e) =>
                {
                    ButtonClick(obj.Funtion);
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.Margin))
        {
            con.Margin = Thickness.Parse(obj.Margin);
        }

        if (!string.IsNullOrWhiteSpace(obj.Background))
        {
            var color = Brush.Parse(obj.Background);
            switch (type)
            {
                case ViewType.Button:
                    (con as Button).Background = color;
                    break;
                case ViewType.Label:
                    (con as Label).Background = color;
                    break;
                case ViewType.StackPanel:
                    (con as StackPanel).Background = color;
                    break;
                case ViewType.Grid:
                    (con as Grid).Background = color;
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(obj.Foreground))
        {
            var color = Brush.Parse(obj.Foreground);
            switch (type)
            {
                case ViewType.Button:
                    (con as Button).Foreground = color;
                    break;
                case ViewType.Label:
                    (con as Label).Foreground = color;
                    break;
            }
        }

        if (obj.Width != 0)
        {
            con.Width = obj.Width;
        }

        if (obj.Height != 0)
        {
            con.Height = obj.Height;
        }

        if (type == ViewType.GameItem)
        {
            (con as GameControl).SetItem(Obj!);
            (con as GameControl).SetSelect(true);
        }

        if (type == ViewType.Grid || type == ViewType.StackPanel)
        {
            MakeItems(con as Panel, obj);
        }

        if (type == ViewType.Label)
        {
            if (obj.Content == "{UserName}")
            {
                (con as Label).Bind(ContentProperty, new Binding
                {
                    Source = CustomModel,
                    Path = "Name"
                });
            }
            else if (obj.Content == "{UserType}")
            {
                (con as Label).Bind(ContentProperty, new Binding
                {
                    Source = CustomModel,
                    Path = "Type"
                });
            }
        }

        return con;
    }

    private Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return state switch
            {
                LaunchState.LostLib => await Info.ShowWait(Localizer.Instance["MainWindow.Lost2"]),
                LaunchState.LostLoader => await Info.ShowWait(Localizer.Instance["MainWindow.Lost3"]),
                LaunchState.LostLoginCore => await Info.ShowWait(Localizer.Instance["MainWindow.Lost4"]),
                _ => await Info.ShowWait(Localizer.Instance["MainWindow.Lost1"]),
            };
        });
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Last = state;
            switch (state)
            {
                case LaunchState.Login:
                    Info1.NextText(Localizer.Instance["MainWindow.Check1"]);
                    break;
                case LaunchState.Check:
                    Info1.NextText(Localizer.Instance["MainWindow.Check2"]);
                    break;
                case LaunchState.CheckVersion:
                    Info1.NextText(Localizer.Instance["MainWindow.Check3"]);
                    break;
                case LaunchState.CheckLib:
                    Info1.NextText(Localizer.Instance["MainWindow.Check4"]);
                    break;
                case LaunchState.CheckAssets:
                    Info1.NextText(Localizer.Instance["MainWindow.Check5"]);
                    break;
                case LaunchState.CheckLoader:
                    Info1.NextText(Localizer.Instance["MainWindow.Check6"]);
                    break;
                case LaunchState.CheckLoginCore:
                    Info1.NextText(Localizer.Instance["MainWindow.Check7"]);
                    break;
                case LaunchState.CheckMods:
                    Info1.NextText(Localizer.Instance["MainWindow.Check10"]);
                    break;
                case LaunchState.Download:
                    Info1.NextText(Localizer.Instance["MainWindow.Check8"]);
                    break;
                case LaunchState.JvmPrepare:
                    Info1.NextText(Localizer.Instance["MainWindow.Check9"]);
                    break;
            }
        });
    }

    private void ButtonClick(string name)
    {
        if (name.StartsWith("OpUrl"))
        {
            var arg = name.Split(" ");
            if (arg.Length != 2)
            {
                return;
            }

            BaseBinding.OpUrl(arg[1]);
        }
        else if (name == "Launch")
        {
            Launch();
        }
        else if (name == "UserEdit")
        {
            App.ShowUser(false);
        }
        else if (name == "UserAdd")
        {
            App.ShowUser(true);
        }
        else if (name == "Setting")
        {
            App.ShowSetting();
        }
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }
}
