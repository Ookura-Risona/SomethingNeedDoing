﻿namespace SomethingNeedDoing.Documentation;
/// <summary>
/// Represents documentation for a Lua function.
/// </summary>
public class LuaFunctionDoc(string moduleName, string functionName, string? description, LuaTypeInfo returnType, List<(string Name, LuaTypeInfo Type, string? Description, object? DefaultValue)> parameters, string[]? examples, bool isMethod)
{
    public string ModuleName { get; } = moduleName;
    public string FunctionName { get; } = functionName;
    public string? Description { get; } = description;
    public LuaTypeInfo ReturnType { get; } = returnType;
    public List<(string Name, LuaTypeInfo Type, string? Description, object? DefaultValue)> Parameters { get; } = parameters;
    public string[]? Examples { get; } = examples;
    public bool IsMethod { get; } = isMethod;
}
