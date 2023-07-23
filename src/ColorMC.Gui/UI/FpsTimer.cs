﻿using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI;

public class FpsTimer
{
    private readonly OpenGlControlBase _render;
    private readonly Timer t_timer;

    public int Fps { get; set; } = 60;
    public Action<int>? FpsTick { private get; init; }
    public bool Pause { get; set; }
    public int NowFps { get; private set; }

    private int _time;
    private bool _run;
    
    public FpsTimer(OpenGlControlBase render)
    {
        _render = render;
        _run = true;
        _time = (int)((double)1000 / Fps);
        t_timer = new(Tick);
        t_timer.Change(0, 1000);
        new Thread(() => 
        {
            while (_run)
            {
                if (Pause)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Dispatcher.UIThread.Invoke(() => _render.RequestNextFrameRendering());
                NowFps++;
                Thread.Sleep(_time);
            }
        })
        {
            Name = "ColorMC_Render_Timer"   
        }.Start();
    }

    private void Tick(object? state)
    {
        if (NowFps != Fps && _time > 1)
        {
            if (NowFps > Fps)
            {
                _time++;
            }
            else
            {
                _time--;
            }
        }
        FpsTick?.Invoke(NowFps);
        NowFps = 0;
    }

    public void Close()
    {
        _run = false;
        t_timer.Dispose();
    }
}