﻿using FluentAssertions;
using Lib.extension;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCloud.Framework.Database.Abstractions.Entity;
using WCloud.Framework.Database.Abstractions.Extension;
using WCloud.Framework.Database.EntityFrameworkCore;
using WCloud.Member.DataAccess.EF;
using WCloud.Member.Domain.Admin;

namespace WCloud.Member.Application.Service.impl
{
    public class AdminService : IAdminService
    {
        private readonly IMSRepository<AdminEntity> _adminRepo;
        private readonly IMSRepository<AdminRoleEntity> _adminRoleRepo;
        private readonly IMSRepository<RoleEntity> _roleRepo;

        public AdminService(
            IMSRepository<AdminEntity> _adminRepo,
            IMSRepository<AdminRoleEntity> _adminRoleRepo,
            IMSRepository<RoleEntity> _roleRepo)
        {
            this._adminRepo = _adminRepo;
            this._adminRoleRepo = _adminRoleRepo;
            this._roleRepo = _roleRepo;
        }

        public virtual async Task UpdateUser(AdminEntity model)
        {
            model.Should().NotBeNull("admin service model");
            model.UID.Should().NotBeNullOrEmpty("admin service model uid");

            var data = new _<AdminEntity>();

            var user = await this._adminRepo.GetFirstAsync(x => x.UID == model.UID);
            user.Should().NotBeNull($"admin不存在:{model.UID}");

            user.SetField(new
            {
                model.UserImg,
                model.NickName,
                model.Sex,
                model.ContactPhone,
                model.ContactEmail
            });

            user.Update();

            await this._adminRepo.UpdateAsync(user);
        }

        public async Task<AdminEntity> GetUserByUID(string uid)
        {
            uid.Should().NotBeNullOrEmpty("admin service getuserbyuid");

            var res = await this._adminRepo.GetFirstAsync(x => x.UID == uid);
            return res;
        }

        public async Task<PagerData<AdminEntity>> QueryUserList(string name = null, string email = null, string keyword = null, int? isremove = null, int page = 1, int pagesize = 20)
        {
            page.Should().BeGreaterOrEqualTo(1, "admin service page");
            pagesize.Should().BeInRange(1, 100, "admin service pagesize");

            var query = this._adminRepo.Database.Set<AdminEntity>().IgnoreQueryFilters().AsNoTrackingQueryable();

            query = query.WhereIf(isremove != null, x => x.IsDeleted == isremove);
            query = query.WhereIf(ValidateHelper.IsNotEmpty(name), x => x.NickName == name);
            query = query.WhereIf(ValidateHelper.IsNotEmpty(keyword), x => x.NickName.StartsWith(keyword));

            var data = await query.ToPagedListAsync(page, pagesize, x => x.Id, desc: false);

            return data;
        }

        public async Task<AdminEntity> GetUserByUserName(string name)
        {
            name.Should().NotBeNullOrEmpty("adminservice getuserbyusername");

            var res = await this._adminRepo.GetFirstAsync(x => x.UserName == name);
            return res;
        }

        public async Task<IEnumerable<AdminEntity>> LoadRoles(IEnumerable<AdminEntity> list)
        {
            list.Should().NotBeNull("load roles list");

            if (list.Any())
            {
                var uids = list.Select(x => x.UID).ToList();
                var maps = await this._adminRoleRepo.Database.Set<AdminRoleEntity>().AsNoTrackingQueryable()
                    .Where(x => uids.Contains(x.AdminUID)).Select(x => new { x.AdminUID, x.RoleUID }).ToArrayAsync();

                if (maps.Any())
                {
                    var role_uids = maps.Select(x => x.RoleUID).ToList();
                    var roles = await this._roleRepo.GetListAsync(x => role_uids.Contains(x.UID));

                    foreach (var m in list)
                    {
                        var user_uids = maps.Where(x => x.AdminUID == m.UID).Select(x => x.RoleUID).ToList();
                        m.Roles = roles.Where(x => user_uids.Contains(x.UID)).ToArray();
                    }
                }
            }
            foreach (var m in list)
            {
                m.Roles ??= new RoleEntity[] { };
            }
            return list;
        }

        public async Task<_<AdminEntity>> AddAdmin(AdminEntity model)
        {
            model.Should().NotBeNull("adminservice add model");
            model.UserName.Should().NotBeNullOrEmpty("adminservice add model username");

            var res = new _<AdminEntity>();

            model.InitSelf();
            if (await this._adminRepo.ExistAsync(x => x.UserName == model.UserName))
            {
                return res.SetErrorMsg("用户名已存在");
            }

            await this._adminRepo.AddAsync(model);

            return res.SetSuccessData(model);
        }

        public async Task<List<AdminEntity>> QueryTopUser(string q = null, string[] role_uid = null, int size = 20)
        {
            size.Should().BeInRange(1, 100, "query top user size");

            var db = this._adminRepo.Database;
            var user_query = db.Set<AdminEntity>().AsNoTrackingQueryable();

            user_query = user_query.WhereIf(ValidateHelper.IsNotEmpty(q),
                x => x.UserName.StartsWith(q) || x.NickName.StartsWith(q));

            if (ValidateHelper.IsNotEmpty(role_uid))
            {
                var role_map_query = db.Set<AdminRoleEntity>().AsNoTracking();

                var query = from user in user_query
                            join map in role_map_query.Where(x => role_uid.Contains(x.RoleUID))
                            on user.UID equals map.AdminUID
                            select user;

                user_query = query;
            }


            var data = await user_query.Take(size).ToListAsync();

            return data;
        }
    }
}
