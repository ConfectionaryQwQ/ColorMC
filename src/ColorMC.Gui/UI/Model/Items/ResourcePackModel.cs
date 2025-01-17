using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ResourcePackModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelect;

    public readonly GameEditTab8Model Top;
    public readonly ResourcepackObj Pack;

    public string Local => Pack.Local;
    public string PackFormat => Pack.pack_format.ToString();
    public string Description => Pack.description;
    public string Broken => Pack.Broken ?
            App.GetLanguage("GameEditWindow.Tab8.Info4") : "";

    public Bitmap Pic { get; }

    public ResourcePackModel(GameEditTab8Model top, ResourcepackObj pack)
    {
        Top = top;
        Pack = pack;
        Pic = Pack.Icon == null ? App.GameIcon : GetImage();
    }

    public Bitmap GetImage()
    {
        using var stream = new MemoryStream(Pack.Icon);
        return new Bitmap(stream);
    }

    public void Select()
    {
        Top.SetSelect(this);
    }

    public void Close()
    {
        if (Pic != App.GameIcon)
        {
            Pic.Dispose();
        }
    }
}