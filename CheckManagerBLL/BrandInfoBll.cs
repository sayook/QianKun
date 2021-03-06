﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckManagerDAL;
using CheckManagerModel;
using CheckManagerModel.DbModels;

namespace CheckManagerBLL
{
    public class BrandInfoBll
    {
        public IEnumerable<BrandInfo> GetBrandInfos(string cloudName)
        {
            EzpEdDal ezpEdDal = new EzpEdDal($"{cloudName}.ezp-ed");
            EzpBaseDal ezpBaseDal = new EzpBaseDal($"{cloudName}.ezp-base");
            IEnumerable<Ed_base_brand> dbBrandList = null;
            IEnumerable<Pf_sys_db_shard_nodes> dbShardNodes = null;
            dbBrandList = ezpEdDal.GetEd_base_brand();
            dbShardNodes = ezpBaseDal.GetPf_sys_db_shard_nodes();

            if (dbBrandList == null || dbShardNodes == null) return null;

            return dbBrandList.Select(x => new BrandInfo
            {
                BrandId = x.Id,
                BrandName = x.Name,
                CrmDbConnectiong = dbShardNodes.FirstOrDefault(n => n.DbShardingId == x.CrmDbShardingId && n.IsMaster == "0")?.DbConnString,
                RtlDbConnectiong = dbShardNodes.FirstOrDefault(n => n.DbShardingId == x.RtlDbShardingId && n.IsMaster == "0")?.DbConnString
            }).Where(x => !string.IsNullOrWhiteSpace(x.CrmDbConnectiong) && !string.IsNullOrWhiteSpace(x.RtlDbConnectiong));
        }

        public async Task<IEnumerable<BrandInfo>> GetBrandInfosAsync(string cloudName)
        {
            EzpEdDal ezpEdDal = new EzpEdDal($"{cloudName}.ezp-ed");
            var taskDbBrandList = ezpEdDal.GetEd_base_brandAsync();

            EzpBaseDal ezpBaseDal = new EzpBaseDal($"{cloudName}.ezp-base");
            var taskDbShardNodes = ezpBaseDal.GetPf_sys_db_shard_nodesAsync();


            IEnumerable<Ed_base_brand> dbBrandList = await taskDbBrandList;
            IEnumerable<Pf_sys_db_shard_nodes> dbShardNodes = await taskDbShardNodes;

            if (dbBrandList == null || dbShardNodes == null) return null;

            return dbBrandList.Select(x => new BrandInfo
            {
                BrandId = x.Id,
                BrandName = x.Name,
                CrmDbConnectiong = dbShardNodes.FirstOrDefault(n => n.DbShardingId == x.CrmDbShardingId && n.IsMaster == "0")?.DbConnString,
                RtlDbConnectiong = dbShardNodes.FirstOrDefault(n => n.DbShardingId == x.RtlDbShardingId && n.IsMaster == "0")?.DbConnString
            }).Where(x => !string.IsNullOrWhiteSpace(x.CrmDbConnectiong) && !string.IsNullOrWhiteSpace(x.RtlDbConnectiong));
        }
    }
}
