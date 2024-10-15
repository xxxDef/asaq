using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Asaq.Core;

public static class InvalidOperation
{
    [DebuggerHidden]
    public static void IfFalse(
        [DoesNotReturnIf(false)] bool condition, 
        string? message = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] long callerLineNumber = 0,
        [CallerMemberName] string callerMember = "")
    {
        if (condition)
            return;
        Throw(message ?? $"Unexpected {condition} condition",
            callerFilePath,
            callerLineNumber,
            callerMember);
    }

    [DebuggerHidden]
    public static void IfNullOrEmpty(
        [NotNull] string? value, 
        string? message = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] long callerLineNumber = 0,
        [CallerMemberName] string callerMember = "")
    {
        var isNull = value is null ? "null" : "empty";
        IfFalse(!string.IsNullOrEmpty(value), 
            message ?? $"Unexpected {isNull} string",
            callerFilePath,
            callerLineNumber,
            callerMember);
    }

    [DebuggerHidden]
    public static void IfNull<T>(
        [NotNull] T? obj, 
        string? message = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] long callerLineNumber = 0,
        [CallerMemberName] string callerMember = "")
    {
        IfFalse(obj is not null, 
            message ?? $"Unexpected null value", 
            callerFilePath, 
            callerLineNumber, 
            callerMember);
    }

    [DebuggerHidden]
    public static void IfNotNull<T>(
        T? obj, 
        string? message = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] long callerLineNumber = 0,
        [CallerMemberName] string callerMember = "")
    {
        IfFalse(obj is null,
            message ?? $"Unexpected not-null value",
            callerFilePath,
            callerLineNumber,
            callerMember);
    }

    [DebuggerHidden]
    [DoesNotReturn]
    public static void Throw(
       string message,
       [CallerFilePath] string callerFilePath = "",
       [CallerLineNumber] long callerLineNumber = 0,
       [CallerMemberName] string callerMember = "")

    {
        throw new InvalidOperationException(
            message ?? $"{message}{Environment.NewLine}Method {callerMember} in {callerFilePath}({callerLineNumber})");
    }
}
