using System;
using System.Linq;
using Godot;

public enum Platform
{
    HTML5,
    Windows,
    Mac,
    Linux,
    IOS,
    Android
}


public static class PlatformUtil
{
    private static Platform? _detectedPlatform = null;
    public static Platform DetectedPlatform => _detectedPlatform ?? (_detectedPlatform = DetectPlatform()).Value;

    public static bool IsMobile => new[] {Platform.IOS, Platform.Android}.Contains(DetectedPlatform);
    public static bool IsDesktop => !IsMobile;
    
    private static Platform DetectPlatform()
    {
        switch (OS.GetName())
        {
            case "Android": return Platform.Android;
            case "iOS": return Platform.IOS;
            case "HTML5": return Platform.HTML5;
            case "OSX": return Platform.Mac;
            case "X11": return Platform.Linux;
            case "Windows": return Platform.Windows;
            
            default: throw new NotImplementedException($"Unexpected platform {OS.GetName()}");
        }
    }
}
