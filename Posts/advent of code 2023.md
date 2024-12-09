
    ((s.Split "\n").Select(x => int.Parse(x.FirstWhere char.IsDigit + x.LastWhere char.IsDigit))).Sum