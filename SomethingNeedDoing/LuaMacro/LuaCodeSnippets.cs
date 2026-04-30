namespace SomethingNeedDoing.LuaMacro;

public static class LuaCodeSnippets
{
    public const string EntrypointTemplate = @"
yield = coroutine.yield
--
function entrypoint()
{0}
end
--
return coroutine.wrap(entrypoint)";

    public const string FStringSnippet = @"
function f(str)
   local outer_env = _ENV
   return (str:gsub(""%b{}"", function(block)
      local code = block:match(""{(.*)}"")
      local exp_env = {}
      setmetatable(exp_env, { __index = function(_, k)
         local stack_level = 5
         while debug.getinfo(stack_level, """") ~= nil do
            local i = 1
            repeat
               local name, value = debug.getlocal(stack_level, i)
               if name == k then
                  return value
               end
               i = i + 1
            until name == nil
            stack_level = stack_level + 1
         end
         return rawget(outer_env, k)
      end })
      local fn, err = load(""return ""..code, ""expression `""..code..""`"", ""t"", exp_env)
      if fn then
         return tostring(fn())
      else
         error(err, 0)
      end
   end))
end";

    public const string PackageSearchersSnippet = @"
_G.snd = {
  require = {
    paths = {},
    add_paths = function(...)
      for k, v in pairs({ ... }) do
        table.insert(snd.require.paths, v)
      end
    end
  }
}

package.original_searchers = package.searchers
package.searchers = { package.original_searchers[1] } -- keep the preload searcher
table.insert(package.searchers, function(name) -- find files
  if name:match("".macro$"") then return end
  local chunkname = 'file[""' .. name .. '""]'

  local abs_file = package.searchpath("""", name, '/') -- check absolute path
  if abs_file ~= nil then
    local loaded, err = loadfile(abs_file)
    return assert(loaded, err), chunkname
  end

  for _, v in ipairs(snd.require.paths) do -- check in paths from snd.require.paths
    local path = v:gsub(""[/\\]*$"", """")
    local rel_file = package.searchpath("""", name, '/')
        or package.searchpath(name, path .. ""\\?;"" .. path .. ""\\?.lua"", '/')
    if rel_file ~= nil then
      local loaded, err = loadfile(rel_file)
      return assert(loaded, err), chunkname
    end
  end

  if #snd.require.paths > 0 then
    return 'no matching file: ' .. chunkname .. ' in searched paths:\n  ' .. table.concat(snd.require.paths, '\n  ')
  else
    return 'no matching file: ' .. chunkname .. ' (and snd.require.paths was empty)'
  end
end)
table.insert(package.searchers, function(name) -- find macros
  local macro = string.gsub(name, "".macro$"", """")
  local chunkname = 'macro[""' .. macro .. '""]'
  local macro_text = InternalGetMacroText(macro)
  if macro_text ~= nil then
    local loaded, err = load(macro_text)
    return assert(loaded, err), chunkname
  end
  return 'no matching macro: ' .. chunkname
end)
";

    /// <summary>
    /// Lua code snippet to enhance error reporting with stack traces.
    /// </summary>
    public const string ErrorHandlerSnippet = @"
-- Enhanced error handler for better debugging
function enhanced_error_handler(err)
  local error_info = debug.getinfo(2, ""Sln"")
  local error_source = error_info and error_info.source or ""unknown""
  local error_line = error_info and error_info.currentline or 0
  local error_name = error_info and error_info.name or ""unknown""

  local stack_trace = """"
  local level = 2
  while true do
    local info = debug.getinfo(level, ""Sln"")
    if not info then break end

    local source = info.source
    if source:sub(1,1) == ""@"" then
      source = source:sub(2)
    elseif source:sub(1,1) == ""="" then
      source = ""[string]"" .. source:sub(2)
    end

    stack_trace = stack_trace .. string.format(""  %s:%d in function '%s'\n"",
      source, info.currentline, info.name or ""(anonymous)"")

    level = level + 1
  end

  return string.format(""Error: %s\nSource: %s:%d in function '%s'\nStack trace:\n%s"",
    err, error_source, error_line, error_name, stack_trace)
end

-- Set the error handler
debug.sethook(function(event, line)
  if event == ""error"" then
    error(enhanced_error_handler(debug.traceback()), 0)
  end
end, ""l"")
";
}
