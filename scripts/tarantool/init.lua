function find_profiles(prefix, limit)
  local ret = {}
  local iterated_count = 0
  
  prefix = prefix:lower()

  for _, tuple in box.space.user_profiles.index.name:pairs(prefix, {iterator = 'ge'}) do
    if string.startswith(tuple[2]:lower(), prefix) and string.startswith(tuple[3]:lower(), prefix) then
      table.insert(ret, tuple)

      iterated_count = iterated_count + 1
      if iterated_count >= limit then break end
    end
  end
  return ret
end
