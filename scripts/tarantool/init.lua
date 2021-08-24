function find_profiles(prefix, limit)
  local ret = {}
  local iterated_count = 0

  for _, tuple in box.space.user_profiles.index.name:pairs(prefix, {iterator = 'ge'}) do
    if string.startswith(tuple[2], prefix) and string.startswith(tuple[3], prefix) then
      table.insert(ret, tuple)

      iterated_count = iterated_count + 1
      if iterated_count >= limit then break end
    end
  end
  return ret
end
