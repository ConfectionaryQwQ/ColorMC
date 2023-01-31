using Avalonia.Controls;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;
using System.Runtime.InteropServices;
using System.Numerics;
using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Skin.Model;
using Avalonia.SourceGenerator;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : Window
{
    public SkinWindow()
    {
        InitializeComponent();
    }
}

public class OpenGlPageControl : OpenGlControlBase
{
    private float _yaw;

    public static readonly DirectProperty<OpenGlPageControl, float> YawProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);

    public float Yaw
    {
        get => _yaw;
        set => SetAndRaise(YawProperty, ref _yaw, value);
    }

    private float _pitch;

    public static readonly DirectProperty<OpenGlPageControl, float> PitchProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Pitch", o => o.Pitch, (o, v) => o.Pitch = v);

    public float Pitch
    {
        get => _pitch;
        set => SetAndRaise(PitchProperty, ref _pitch, value);
    }


    private float _roll;

    public static readonly DirectProperty<OpenGlPageControl, float> RollProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Roll", o => o.Roll, (o, v) => o.Roll = v);

    public float Roll
    {
        get => _roll;
        set => SetAndRaise(RollProperty, ref _roll, value);
    }

    private string _info = string.Empty;

    public static readonly DirectProperty<OpenGlPageControl, string> InfoProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info", o => o.Info, (o, v) => o.Info = v);

    public string Info
    {
        get => _info;
        private set => SetAndRaise(InfoProperty, ref _info, value);
    }

    private string VertexShaderSource => GetShader(false, @"
        attribute vec3 a_Position;
        attribute vec2 a_texCoord;
        uniform mat4 uModel;
        uniform mat4 uProjection;
        uniform mat4 uView;
        varying vec2 v_texCoord;
        void main()
        {
            v_texCoord = a_texCoord;
            
            gl_Position = uProjection * uView * uModel * vec4(a_Position, 1.0);
        }
");

    private string FragmentShaderSource => GetShader(true, @"
        varying vec2 v_texCoord;
        uniform sampler2D texture0;
        void main()
        {
            gl_FragColor = texture2D(texture0, v_texCoord);
        }
");

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
    }

    private readonly Vertex[] points;
    private readonly Vertex[] pointsTop;
    private Image<Rgba32> _Image;
    private int texture;

    private ModelSourceTextureType steveModelType;
    private ushort[] steveModelDrawOrder;
    private ushort[] steveModelDrawOrderTop;

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;

    private int _vertexBufferObject;
    private int _indexBufferObject;
    private int _vertexArrayObject;

    private int _vertexBufferObjectTop;
    private int _indexBufferObjectTop;
    private int _vertexArrayObjectTop;

    public delegate void GlFunc1(int v1, int v2);
    public delegate void GlFunc2(int v1);
    public delegate void GlFunc3(float v1);
    public delegate void GlFunc4(bool v1);

    public GlFunc2 glDepthFunc;
    public GlFunc3 glClearDepth;
    public GlFunc1 glBlendFunc;
    public GlFunc4 glDepthMask;
    public GlFunc2 glCullFace;
    public GlFunc2 glDisable;

    public OpenGlPageControl()
    {
        _Image = Image.Load<Rgba32>("C:\\Users\\40206\\Desktop\\color_yr.png");

        steveModelType = SkinUtil.GetTextType(_Image);
        steveModelType = ModelSourceTextureType.RATIO_1_1_SLIM;

        {
            var steve = SteveC.GetSteve(steveModelType);
            var steveModelCoords = steve.Item1;
            steveModelDrawOrder = steve.Item2;
            var steveTextureCoords = Steve3DTexture.GetSteveTexture(steveModelType);

            points = new Vertex[steveModelCoords.Length / 3];

            for (var primitive = 0; primitive < steveModelCoords.Length / 3; primitive++)
            {
                var srci = primitive * 3;
                var srci1 = primitive * 2;
                points[primitive] = new Vertex
                {
                    Position = new Vector3(steveModelCoords[srci], steveModelCoords[srci + 1], steveModelCoords[srci + 2]),
                    UV = new Vector2(steveTextureCoords[srci1], steveTextureCoords[srci1 + 1])
                };
            }
        }

        {
            var steve1 = SteveC.GetSteveTop(steveModelType);
            var steveModelCoords1 = steve1.Item1;
            steveModelDrawOrderTop = steve1.Item2;
            var steveTextureCoords1 = Steve3DTexture.GetSteveTextureTop(steveModelType);

            pointsTop = new Vertex[steveModelCoords1.Length / 3];

            for (var primitive = 0; primitive < steveModelCoords1.Length / 3; primitive++)
            {
                var srci = primitive * 3;
                var srci1 = primitive * 2;
                pointsTop[primitive] = new Vertex
                {
                    Position = new Vector3(steveModelCoords1[srci], steveModelCoords1[srci + 1], steveModelCoords1[srci + 2]),
                    UV = new Vector2(steveTextureCoords1[srci1], steveTextureCoords1[srci1 + 1])
                };
            }
        }
    }

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
        IntPtr temp = GL.GetProcAddress("glDepthFunc");
        glDepthFunc = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = GL.GetProcAddress("glClearDepth");
        glClearDepth = (GlFunc3)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc3));
        temp = GL.GetProcAddress("glBlendFunc");
        glBlendFunc = (GlFunc1)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc1));
        temp = GL.GetProcAddress("glDepthMask");
        glDepthMask = (GlFunc4)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc4));
        temp = GL.GetProcAddress("glCullFace");
        glCullFace = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = GL.GetProcAddress("glDisable");
        glDisable = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));

        GL.ClearColor(0, 0, 0, 1);
        GL.Enable(GL_DEPTH_TEST);

        //GL_BLEND
        GL.Enable(3042);

        //gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA
        glBlendFunc(770, 771);

        //gl.GL_BACK
        glCullFace(1029);

        //gl.GL_CULL_FACE
        GL.Enable(2884);

        CheckError(GL);

        Info = $"Renderer: {GL.GetString(GL_RENDERER)} Version: {GL.GetString(GL_VERSION)}";

        // Load the source of the vertex shader and compile it.
        _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
        var error = GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource);
        Console.WriteLine(error);

        // Load the source of the fragment shader and compile it.
        _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        error = GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource);
        Console.WriteLine(error);

        // Create the shader program, attach the vertex and fragment shaders and link the program.
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        error = GL.LinkProgramAndGetError(_shaderProgram);
        Console.WriteLine(error);
        CheckError(GL);

        int[] temp2 = new int[2];

        fixed (int* pdata = temp2)
            GL.GenVertexArrays(2, pdata);
        _vertexArrayObject = temp2[0];
        _vertexArrayObjectTop = temp2[1];

        {
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = points)
                GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            _indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
            fixed (void* pdata = steveModelDrawOrder)
                GL.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(steveModelDrawOrder.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);
            CheckError(GL);

            int positionLocation = GL.GetAttribLocationString(_shaderProgram, "a_Position");
            int uv = GL.GetAttribLocationString(_shaderProgram, "a_texCoord");
            GL.VertexAttribPointer(positionLocation, 3, GL_FLOAT,
                0, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(uv, 2, GL_FLOAT,
                0, 5 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(uv);

            GL.BindVertexArray(0);
        }

        {
            GL.BindVertexArray(_vertexArrayObjectTop);

            _vertexBufferObjectTop = GL.GenBuffer();
            GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObjectTop);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = pointsTop)
                GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            _indexBufferObjectTop = GL.GenBuffer();
            GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObjectTop);
            fixed (void* pdata = steveModelDrawOrderTop)
                GL.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(steveModelDrawOrder.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);
            CheckError(GL);

            int positionLocation = GL.GetAttribLocationString(_shaderProgram, "a_Position");
            int uv = GL.GetAttribLocationString(_shaderProgram, "a_texCoord");
            GL.VertexAttribPointer(positionLocation, 3, GL_FLOAT,
                0, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(uv, 2, GL_FLOAT,
                0, 5 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(uv);

            GL.BindVertexArray(0);
        }

        GL.UseProgram(_shaderProgram);

        
        CheckError(GL);

        texture = GL.GenTexture();
        GL.ActiveTexture(GL_TEXTURE0);
        GL.BindTexture(GL_TEXTURE_2D, texture);

        GL.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MIN_FILTER, GL_LINEAR
        );
        GL.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MAG_FILTER, GL_NEAREST
        );
        //TextureWrapS ClampToEdge
        GL.TexParameteri(
            GL_TEXTURE_2D,
            10242, 33071
        );
        //TextureWrapT ClampToEdge
        GL.TexParameteri(
            GL_TEXTURE_2D,
            10243, 33071
        );

        var pixels = new byte[4 * _Image.Width * _Image.Height];
        _Image.CopyPixelDataTo(pixels);

        fixed (void* pdata = pixels)
            GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _Image.Width, _Image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(pdata));

        CheckError(GL);
    }

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        // Unbind everything
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all resources.
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteBuffer(_indexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);

        GL.DeleteBuffer(_vertexBufferObjectTop);
        GL.DeleteBuffer(_indexBufferObjectTop);

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.ClearColor(0, 0, 0, 1);

        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        var GL = gl;

        GL.ActiveTexture(GL_TEXTURE0);
        GL.BindTexture(GL_TEXTURE_2D, texture);
       
        GL.UseProgram(_shaderProgram);
        CheckError(GL);

        var projection =
            Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
                0.01f, 1000);

        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 7), new Vector3(), new Vector3(0, 1, 0));
        var model = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
        var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
        var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
        var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);
        GL.UniformMatrix4fv(viewLoc, 1, false, &view);
        GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);

        CheckError(GL);

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(GL_TRIANGLES, steveModelDrawOrder.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);

        GL.Enable(3042); //GL_BLEND
        glBlendFunc(770, 771); //gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA
        glDepthMask(false);
        GL.BindVertexArray(_vertexArrayObjectTop);
        GL.DrawElements(GL_TRIANGLES, steveModelDrawOrderTop.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        glDisable(3042);
        glDepthMask(true);

        CheckError(GL);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == YawProperty || change.Property == RollProperty || change.Property == PitchProperty)
            InvalidateVisual();
        base.OnPropertyChanged(change);
    }

    private string GetShader(bool fragment, string shader)
    {
        var version = (GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100);
        var data = "#version " + version + "\n";
        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor");
            else
                shader = shader.Replace("varying", "out");
        }

        data += shader;

        return data;
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
            Console.WriteLine(err);
    }
}
