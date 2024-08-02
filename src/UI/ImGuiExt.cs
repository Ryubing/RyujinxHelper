using System;
using System.Numerics;
using ImGuiNET;
using Silk.NET.SDL;

namespace Volte.UI;

/**
 * A class that contains all helper extension methods for ImGui.
 * Named Gui instead of ImGui to avoid conflicts.
 */
public static class Gui
{
    public static void SameLineText(ReadOnlySpan<char> text)
    {
        ImGui.Text(text);
        ImGui.SameLine();
    }
    
    public static void SameLineText(ReadOnlySpan<char> text, Color color)
    {
        ImGui.TextColored(color.AsVec4(), text);
        ImGui.SameLine();
    }
    
    public static void SameLineText(ReadOnlySpan<char> text, System.Drawing.Color color)
    {
        ImGui.TextColored(color.AsVec4(), text);
        ImGui.SameLine();
    }
    
    public static void Text(ReadOnlySpan<char> text, Color color) 
        => ImGui.TextColored(color.AsVec4(), text);

    public static void Text(ReadOnlySpan<char> text, System.Drawing.Color color) 
        => ImGui.TextColored(color.AsVec4(), text);

    public static IDisposable PushValue(this ImGuiStyleVar styleVar, Vector2 value) => new ScopedStyleVar(styleVar, value);
    public static IDisposable PushValue(this ImGuiStyleVar styleVar, float value) => new ScopedStyleVar(styleVar, value);

    public static IDisposable PushValue(this ImGuiCol colorVar, Vector4 value) => new ScopedStyleColor(colorVar, value);
    public static IDisposable PushValue(this ImGuiCol colorVar, Vector3 value) => new ScopedStyleColor(colorVar, value);
    public static IDisposable PushValue(this ImGuiCol colorVar, Color value) => new ScopedStyleColor(colorVar, value.AsVec4());

    public static IDisposable PushValue(this ImGuiCol colorVar, System.Drawing.Color value) =>
        new ScopedStyleColor(colorVar, value.AsVec4());

    public static IDisposable PushValue(this ImGuiCol colorVar, uint value) => new ScopedStyleColor(colorVar, value);
}

public struct ScopedStyleVar : IDisposable
{
    internal ScopedStyleVar(ImGuiStyleVar styleVar, Vector2 value) 
        => ImGui.PushStyleVar(styleVar, value);

    internal ScopedStyleVar(ImGuiStyleVar styleVar, float value) 
        => ImGui.PushStyleVar(styleVar, value);

    void IDisposable.Dispose() => ImGui.PopStyleVar();
}

public struct ScopedStyleColor : IDisposable
{
    internal ScopedStyleColor(ImGuiCol colorVar, Vector4 value) 
        => ImGui.PushStyleColor(colorVar, value);

    internal ScopedStyleColor(ImGuiCol colorVar, Vector3 value) 
        => ImGui.PushStyleColor(colorVar, new Vector4(value, 1f));

    internal ScopedStyleColor(ImGuiCol colorVar, uint value) => 
        ImGui.PushStyleColor(colorVar, value);

    void IDisposable.Dispose() => ImGui.PopStyleColor();
}