﻿using System;
using System.Collections.Generic;

namespace splitzy_backend.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
}
