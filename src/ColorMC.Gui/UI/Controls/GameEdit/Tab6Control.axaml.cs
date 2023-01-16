using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System.Collections.Generic;
using System;
using System.Formats.Tar;
using System.IO;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UIBinding;
using Avalonia.Interactivity;
using ColorMC.Gui.Utils.LaunchSetting;
using ColorMC.Core;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Avalonia;
using System.Globalization;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Selection;
using System.ComponentModel;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public class FileTreeNodeModel : ReactiveObject
{
    private string _path;
    private string _name;
    private long? _size;
    private DateTimeOffset? _modified;
    private FileTreeNodeModel? _top;
    private ObservableCollection<FileTreeNodeModel>? _children;
    private bool _hasChildren = true;
    private bool _isExpanded;
    private bool _isChecked;

    public FileTreeNodeModel(
            string path,
            FileTreeNodeModel? top,
            bool isDirectory,
            bool isRoot = false)
    {
        _top = top;
        _path = path;
        _name = isRoot ? path : System.IO.Path.GetFileName(Path);
        _isExpanded = isRoot;
        _isChecked = true;
        IsDirectory = isDirectory;
        HasChildren = isDirectory;

        if (!isDirectory)
        {
            var info = new FileInfo(path);
            Size = info.Length;
            Modified = info.LastWriteTimeUtc;
        }
    }

    public string Path
    {
        get => _path;
        private set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    public string Name
    {
        get => _name;
        private set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public long? Size
    {
        get => _size;
        private set => this.RaiseAndSetIfChanged(ref _size, value);
    }

    public DateTimeOffset? Modified
    {
        get => _modified;
        private set => this.RaiseAndSetIfChanged(ref _modified, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        private set => this.RaiseAndSetIfChanged(ref _hasChildren, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            this.RaiseAndSetIfChanged(ref _isChecked, value);
            if (_isChecked == true)
            {
                if (_children != null)
                {
                    foreach (var item in _children)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            else if (_isChecked == false)
            {
                if (_children != null)
                {
                    foreach (var item in _children)
                    {
                        item.IsChecked = false;
                    }
                }
            }
        }
    }

    public bool IsDirectory { get; }
    public IReadOnlyList<FileTreeNodeModel> Children => _children ??= LoadChildren();

    private ObservableCollection<FileTreeNodeModel> LoadChildren()
    {
        if (!IsDirectory)
        {
            return new();
        }

        var options = new EnumerationOptions { IgnoreInaccessible = true };
        var result = new ObservableCollection<FileTreeNodeModel>();

        foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
        {
            result.Add(new FileTreeNodeModel(d, this, true));
        }

        foreach (var f in Directory.EnumerateFiles(Path, "*", options))
        {
            result.Add(new FileTreeNodeModel(f, this, false));
        }

        if (result.Count == 0)
            HasChildren = false;

        return result;
    }

    public static Comparison<FileTreeNodeModel?> SortAscending<T>(Func<FileTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return -1;
            else if (y is null)
                return 1;
            if (x.IsDirectory == y.IsDirectory)
                return Comparer<T>.Default.Compare(selector(x), selector(y));
            else if (x.IsDirectory)
                return -1;
            else
                return 1;
        };
    }

    public static Comparison<FileTreeNodeModel?> SortDescending<T>(Func<FileTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return 1;
            else if (y is null)
                return -1;
            if (x.IsDirectory == y.IsDirectory)
                return Comparer<T>.Default.Compare(selector(y), selector(x));
            else if (x.IsDirectory)
                return -1;
            else
                return 1;
        };
    }

    public List<string> GetUnSelectItems()
    {
        var list = new List<string>();
        if (_children != null)
        {
            foreach (var item in _children)
            {
                list.AddRange(item.GetUnSelectItems());
            }
        }
        
        if (!IsChecked && !IsDirectory)
        {
            list.Add(System.IO.Path.GetFullPath(Path));
        }

        return list;
    }
}

public class FilesPageViewModel : ReactiveObject
{
    private static IconConverter? s_iconConverter;
    private FileTreeNodeModel _root;
    private GameSettingObj Obj;

    public FilesPageViewModel(GameSettingObj obj)
    {
        Obj = obj;

        Source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
        {
            Columns =
            {
                new TemplateColumn<FileTreeNodeModel>(
                    null,
                    "FileNameCell1",
                    options: new ColumnOptions<FileTreeNodeModel>
                    {
                        CanUserResizeColumn = false
                    }),
                new HierarchicalExpanderColumn<FileTreeNodeModel>(
                    new TemplateColumn<FileTreeNodeModel>(
                        "Name",
                        "FileNameCell",
                        new GridLength(1, GridUnitType.Star),
                        new ColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                        })
                    {
                        IsTextSearchEnabled = true,
                        TextSearchValueSelector = x => x.Name
                    },
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded),
                new TextColumn<FileTreeNodeModel, long?>(
                    "Size",
                    x => x.Size,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                    "Modified",
                    x => x.Modified,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                    }),
            }
        };

        Source.RowSelection!.SingleSelect = false;

        _root = new FileTreeNodeModel(obj.GetBaseDir(), null, true, true);
        Source.Items = new[] { _root };
    }

    public List<string> GetUnSelectItems() 
    {
        return _root.GetUnSelectItems();
    }

    public HierarchicalTreeDataGridSource<FileTreeNodeModel> Source { get; }

    public static IMultiValueConverter FileIconConverter
    {
        get
        {
            if (s_iconConverter is null)
            {
                var assetLoader = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();

                using var fileStream = assetLoader.Open(
                    new Uri("resm:ColorMC.Gui.Resource.Pic.file.png"));
                using var folderStream = assetLoader.Open(
                    new Uri("resm:ColorMC.Gui.Resource.Pic.folder.png"));
                using var folderOpenStream = assetLoader.Open(
                    new Uri("resm:ColorMC.Gui.Resource.Pic.folder-open.png"));

                var fileIcon = new Bitmap(fileStream);
                var folderIcon = new Bitmap(folderStream);
                var folderOpenIcon = new Bitmap(folderOpenStream);

                s_iconConverter = new IconConverter(fileIcon, folderOpenIcon, folderIcon);
            }

            return s_iconConverter;
        }
    }

    private class IconConverter : IMultiValueConverter
    {
        private readonly Bitmap _file;
        private readonly Bitmap _folderExpanded;
        private readonly Bitmap _folderCollapsed;

        public IconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
        {
            _file = file;
            _folderExpanded = folderExpanded;
            _folderCollapsed = folderCollapsed;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 &&
                values[0] is bool isDirectory &&
                values[1] is bool isExpanded)
            {
                if (!isDirectory)
                    return _file;
                else
                    return isExpanded ? _folderExpanded : _folderCollapsed;
            }

            return null;
        }
    }
}

public partial class Tab6Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private FilesPageViewModel FilesPageViewModel;

    public FilesPageViewModel Files
    {
        get => FilesPageViewModel;
    }

    public Tab6Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        SaveFileDialog openFile = new()
        {
            Title = Localizer.Instance["GameEditWindow.Tab6.Info2"],
            DefaultExtension = ".zip"
        };

        var file = await openFile.ShowAsync(Window);
        if (!string.IsNullOrWhiteSpace(file))
        {
            Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab6.Info3"]);
            var list = FilesPageViewModel.GetUnSelectItems();
            bool error = false;
            try
            {
                await GameBinding.ExportGame(Obj, file, list);
                GameBinding.OpFile(file);
            }
            catch (Exception e1)
            {
                Logs.Error(Localizer.Instance["GameEditWindow.Tab6.Error1"], e1);
                error = true;
            }
            Window.Info1.Close();
            if (error)
            {
                Window.Info.Show(Localizer.Instance["GameEditWindow.Tab6.Error1"]);
            }
            else
            {
                Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab6.Info4"]);
            }
        }
    }

    private void Load() 
    {
        FilesPageViewModel = new FilesPageViewModel(Obj);
        FileViewer.Source = Files.Source;
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}