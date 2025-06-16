using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertFunction
{
    public BatchConvertFunctionType Type { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public Control View { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public BatchConvertFunction(BatchConvertFunctionType type, string name, bool isSelected, Control view)
    {
        Type = type;
        Name = name;
        IsSelected = isSelected;
        View = view;
    }

    public static BatchConvertFunction[] List(BatchConvertViewModel vm)
    {
        var activeFunctions = Se.Settings.Tools.BatchConvert.ActiveFunctions;
        return new List<BatchConvertFunction>()
        {
            MakeFunction(BatchConvertFunctionType.RemoveFormatting, "Remove formatting", ViewRemoveFormatting.Make(vm) , activeFunctions),
            MakeFunction(BatchConvertFunctionType.OffsetTimeCodes, "Offset time codes", ViewOffsetTimeCodes.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.AdjustDisplayDuration, "Adjust display duration", ViewAdjustDuration.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.DeleteLines, "Delete lines", ViewDeleteLines.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeFrameRate, "Change frame rate", ViewChangeFrameRate.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeSpeed, "Change speed", ViewChangeSpeed.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeCasing, "Change casing", ViewChangeCasing.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.FixCommonErrors, "Fix common errors", ViewFixCommonErrors.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.MultipleReplace, "Multiple replace", ViewMultipleReplace.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.AutoTranslate, "Auto-translate", ViewAutoTranslate.Make(vm), activeFunctions),
        }.ToArray();
    }

    private static BatchConvertFunction MakeFunction(BatchConvertFunctionType functionType, string name, Control view, string[] activeFunctions)
    {
        var isActive = activeFunctions.Contains(functionType.ToString());
        return new BatchConvertFunction(functionType, name, isActive, view);
    }
}