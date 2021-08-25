function find_profiles(prefix, offset, limit)
  local ret = {}
  local iterated_count = 0
  
  prefix = prefix:lower()

  for _, tuple in box.space.user_profiles.index.name:pairs(prefix, {iterator = 'ge'}) do
    if string.startswith(tuple[2]:lower(), prefix) and string.startswith(tuple[3]:lower(), prefix) then
      
      if iterated_count >= offset + limit then break end

      if iterated_count >= offset then
        table.insert(ret, tuple)
      end

      iterated_count = iterated_count + 1
    end
  end
  return ret
end
