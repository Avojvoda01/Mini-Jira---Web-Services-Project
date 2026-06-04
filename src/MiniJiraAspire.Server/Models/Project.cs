using System;
using System.Collections.Generic;

namespace MiniJiraAspire.Server.Models;

public class Project : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public List<ProjectMember> Members { get; set; } = [];
}