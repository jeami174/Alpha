﻿using Data.Context;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class RoleRepository(DataContext context) : BaseRepository<RoleEntity>(context), IRoleRepository
{
}
