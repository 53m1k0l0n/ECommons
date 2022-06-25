﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using ECommons.DalamudServices;
using ImGuiNET;

namespace ECommons.ImGuiMethods
{
    public static class ImGuiEx
    {
        public static void SetNextItemFullWidth()
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        }

        static Dictionary<string, float> InputWithRightButtonsAreaValues = new();
        public static void InputWithRightButtonsArea(string id, Action inputAction, Action rightAction)
        {
            if (InputWithRightButtonsAreaValues.ContainsKey(id))
            {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - InputWithRightButtonsAreaValues[id]); 
            }
            inputAction();
            ImGui.SameLine();
            var cur1 = ImGui.GetCursorPosX();
            rightAction();
            ImGui.SameLine(0, 0);
            InputWithRightButtonsAreaValues[id] = ImGui.GetCursorPosX() - cur1 + ImGui.GetStyle().ItemSpacing.X;
            ImGui.Dummy(Vector2.Zero);
        }

        static Dictionary<string, Box<uint>> InputListValues = new();
        public static void InputListUint(string name, List<uint> list, Dictionary<uint, string> overrideValues = null)
        {
            if (!InputListValues.ContainsKey(name)) InputListValues[name] = new(0);
            InputList(name, list, overrideValues, delegate
            {
                var buttonSize = ImGuiHelpers.GetButtonSize("Add");
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
                ImGuiEx.InputUint($"##{name.Replace("#", "_")}", ref InputListValues[name].Value);
                ImGui.SameLine();
                if (ImGui.Button("Add"))
                {
                    list.Add(InputListValues[name].Value);
                    InputListValues[name].Value = 0;
                }
            });
        }

        public static void InputList<T>(string name, List<T> list, Dictionary<T, string> overrideValues, Action addFunction)
        {
            var text = list.Count == 0 ? "- No values -" : (list.Count == 1 ? $"{(overrideValues != null && overrideValues.ContainsKey(list[0]) ? overrideValues[list[0]] : list[0])}" : $"- {list.Count} elements -");
            if(ImGui.BeginCombo(name, text))
            {
                addFunction();
                var rem = -1;
                for (var i = 0;i<list.Count;i++)
                {
                    var id = $"{name}ECommonsDeleItem{i}";
                    var x = list[i];
                    ImGui.Selectable($"{(overrideValues != null && overrideValues.ContainsKey(x) ? overrideValues[x]:x)}");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.OpenPopup(id);
                    }
                    if (ImGui.BeginPopup(id))
                    {
                        if (ImGui.Selectable("Delete##ECommonsDeleItem"))
                        {
                            rem = i;
                        }
                        if (ImGui.Selectable("Clear (hold shift+ctrl)##ECommonsDeleItem")
                            && ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl)
                        {
                            rem = -2;
                        }
                        ImGui.EndPopup();
                    }
                }
                if(rem > -1)
                {
                    list.RemoveAt(rem);
                }
                if(rem == -2)
                {
                    list.Clear();
                }
                ImGui.EndCombo();
            }
        }

        public static void WithTextColor(Vector4 col, Action func)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            GenericHelpers.Safe(func);
            ImGui.PopStyleColor();
        }

        public static void Tooltip(string s)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(s);
            }
        }

        public static void Text(string s)
        {
            ImGui.TextUnformatted(s);
        }

        public static void Text(Vector4 col, string s)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            ImGui.TextUnformatted(s);
            ImGui.PopStyleColor();
        }

        public static void Text(uint col, string s)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            ImGui.TextUnformatted(s);
            ImGui.PopStyleColor();
        }

        public static void TextWrapped(string s)
        {
            ImGui.PushTextWrapPos();
            ImGui.TextUnformatted(s);
            ImGui.PopTextWrapPos();
        }

        public static void TextWrapped(Vector4 col, string s)
        {
            ImGui.PushTextWrapPos(0);
            ImGuiEx.Text(col, s);
            ImGui.PopTextWrapPos();
        }

        public static void TextWrapped(uint col, string s)
        {
            ImGui.PushTextWrapPos();
            ImGuiEx.Text(col, s);
            ImGui.PopTextWrapPos();
        }

        public static Vector4 GetParsedColor(int percent)
        {
            if (percent < 25)
            {
                return ImGuiColors.ParsedGrey;
            }
            else if (percent < 50)
            {
                return ImGuiColors.ParsedGreen;
            }
            else if (percent < 75)
            {
                return ImGuiColors.ParsedBlue;
            }
            else if (percent < 95)
            {
                return ImGuiColors.ParsedPurple;
            }
            else if (percent < 99)
            {
                return ImGuiColors.ParsedOrange;
            }
            else if (percent == 99)
            {
                return ImGuiColors.ParsedPink;
            }
            else if (percent == 100)
            {
                return ImGuiColors.ParsedGold;
            }
            else
            {
                return ImGuiColors.DalamudRed;
            }
        }

        public static void EzTabBar(string id, params (string name, Action function, Vector4? color, bool child)[] tabs)
        {
            ImGui.BeginTabBar(id);
            foreach(var x in tabs)
            {
                if (x.name == null) continue;
                if(x.color != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, x.color.Value);
                }
                if (ImGui.BeginTabItem(x.name))
                {
                    if (x.color != null)
                    {
                        ImGui.PopStyleColor();
                    }
                    if(x.child) ImGui.BeginChild(x.name + "child");
                    x.function();
                    if(x.child) ImGui.EndChild();
                    ImGui.EndTabItem();
                }
                else
                {
                    if (x.color != null)
                    {
                        ImGui.PopStyleColor();
                    }
                }
            }
            ImGui.EndTabBar();
        }
        
        public static void InvisibleButton(int width = 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.Button(" ");
            ImGui.PopStyleColor(3);
        }

        public static void EnumCombo<T>(string name, ref T refConfigField) where T : IConvertible
        {
            if(ImGui.BeginCombo(name, refConfigField.ToString().Replace("_", " ")))
            {
                foreach(var x in Enum.GetValues(typeof(T)))
                {
                    if(ImGui.Selectable(x.ToString().Replace("_", " ")))
                    {
                        refConfigField = (T)x;
                    }
                }
                ImGui.EndCombo();
            }
        }

        public static bool IconButton(FontAwesomeIcon icon, string id = "ECommonsButton")
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{id}");
            ImGui.PopFont();
            return result;
        }

        public static float Measure(Action func, bool includeSpacing = true)
        {
            var pos = ImGui.GetCursorPosX();
            func();
            ImGui.SameLine(0, 0);
            var diff = ImGui.GetCursorPosX() - pos;
            ImGui.Dummy(Vector2.Zero);
            return diff + (includeSpacing?ImGui.GetStyle().ItemSpacing.X:0);
        }

        public static void InputHex(string name, ref uint hexInt)
        {
            var text = $"{hexInt:X}";
            if (ImGui.InputText(name, ref text, 8))
            {
                if (uint.TryParse(text, NumberStyles.HexNumber, null, out var num))
                {
                    hexInt = num;
                }
            }
        }

        public static void InputUint(string name, ref uint uInt)
        {
            var text = $"{uInt}";
            if (ImGui.InputText(name, ref text, 16))
            {
                if (uint.TryParse(text, out var num))
                {
                    uInt = num;
                }
            }
        }

        public static void TextCopy(string text)
        {
            ImGui.TextUnformatted(text);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetClipboardText(text);
                Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", null, NotificationType.Success);
            }
        }

        public static void ButtonCopy(string buttonText, string copy)
        {
            if(ImGui.Button(buttonText.Replace("$COPY", copy)))
            {
                ImGui.SetClipboardText(copy);
                Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", null, NotificationType.Success);
            }
        }
    }
}
