﻿using FluentAssertions;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCloud.Framework.Database.Abstractions.Extension;
using WCloud.Member.DataAccess.EF;
using WCloud.Member.Domain.Admin;
using WCloud.Member.Domain.User;

namespace WCloud.Member.Application.Service.impl
{
    public class DeptService : IDeptService
    {
        private readonly IMSRepository<DepartmentEntity> _deptRepo;

        public DeptService(IMSRepository<DepartmentEntity> _deptRepo)
        {
            this._deptRepo = _deptRepo;
        }

        public async Task<_<DepartmentEntity>> Add(DepartmentEntity model)
        {
            model.Should().NotBeNull("dept service add model");
            model.NodeName.Should().NotBeNullOrEmpty("dept service add dept name");

            model.Level.Should().BeGreaterOrEqualTo(1);

            var res = new _<DepartmentEntity>();

            if (await this._deptRepo.ExistAsync(x => x.NodeName == model.NodeName))
                return res.SetErrorMsg("部门名称已经存在");

            model.InitSelf();

            await this._deptRepo.AddAsync(model);

            return res.SetSuccessData(model);
        }

        public async Task<bool> DeleteWhenNoChildren(string uid)
        {
            uid.Should().NotBeNullOrEmpty("dept delete uid");

            var res = await this._deptRepo.DeleteSingleNodeWhenNoChildren_(uid);
            return res;
        }

        public Task<List<UserEntity>> LoadDept(List<UserEntity> list)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DepartmentEntity>> Query(string group = null)
        {
            var query = this._deptRepo.NoTrackingQueryable;

            query = query.WhereIf(ValidateHelper.IsNotEmpty(group), x => x.GroupKey == group);

            var res = await query.Take(5000).ToListAsync();

            return res;
        }

        public async Task<_<DepartmentEntity>> Update(DepartmentEntity model)
        {
            model.Should().NotBeNull("dept update model");
            model.UID.Should().NotBeNullOrEmpty("dept update uid");
            model.NodeName.Should().NotBeNullOrEmpty("dept update dept name");

            var res = new _<DepartmentEntity>();

            var data = await this._deptRepo.GetFirstAsync(x => x.UID == model.UID);
            data.Should().NotBeNull();

            if (await this._deptRepo.ExistAsync(x => x.NodeName == model.NodeName && x.UID != model.UID))
                return res.SetErrorMsg("部门名称已经存在");

            data.NodeName = model.NodeName;
            data.Description = model.Description;

            await this._deptRepo.UpdateAsync(data);

            return res.SetSuccessData(data);
        }
    }
}
