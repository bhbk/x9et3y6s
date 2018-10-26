using AutoMapper;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext-8?view=aspnetcore-2.0
    public partial class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>
    {
        private static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        private static IConfigurationRoot _conf;
        private static IMapper _maps;
        private static IList _fieldsExcluded, _fieldsSensitive, _tablesExcluded;

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public AppDbContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder)
        : base(optionsBuilder.Options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _maps = new MapperConfiguration(maps =>
            {
                maps.AddProfile<IdentityMaps>();
            }).CreateMapper();

            _fieldsExcluded = _conf.GetSection("IdentityActivity:FieldsExcluded").GetChildren().Select(x => x.Value).ToList();
            _fieldsSensitive = _conf.GetSection("IdentityActivity:FieldsSensitive").GetChildren().Select(x => x.Value).ToList();
            _tablesExcluded = _conf.GetSection("IdentityActivity:TablesExcluded").GetChildren().Select(x => x.Value).ToList();

            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entryList = OnSaveActivityBefore();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            OnSaveActivityAfter(entryList);

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entryList = OnSaveActivityBefore();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            await OnSaveActivityAfter(entryList);

            return result;
        }

        //https://www.meziantou.net/2017/08/14/entity-framework-core-history-audit-table    
        private List<ActivityCreate> OnSaveActivityBefore()
        {
            ChangeTracker.DetectChanges();

            var activityList = new List<ActivityCreate>();

            foreach (var entry in ChangeTracker.Entries())
            {
                //entity does not have correct state
                if (entry.State == EntityState.Detached
                    || entry.State == EntityState.Unchanged)
                    continue;

                //entity is excluded from tracking
                if (_tablesExcluded.Contains(entry.Entity.GetType().Name))
                    continue;

                var activity = new ActivityCreate(entry);

                foreach (var prop in entry.Properties)
                {
                    Guid actorId;

                    //entity property required for any tracking...
                    if (!Guid.TryParse(entry.Property("ActorId").CurrentValue.ToString(), out actorId))
                        continue;

                    //entity property is excluded from tracking
                    if (_fieldsExcluded.Contains(prop.Metadata.Name))
                        continue;

                    //entity property generated by database after saving
                    if (prop.IsTemporary)
                    {
                        activity.TemporaryProperties.Add(prop);
                        continue;
                    }

                    if (prop.Metadata.IsPrimaryKey())
                    {
                        activity.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                        continue;
                    }

                    activity.ActorId = actorId;
                    activity.TableName = entry.Metadata.Relational().TableName;
                    activity.Immutable = true;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            {
                                activity.CurrentValues[prop.Metadata.Name] = prop.CurrentValue;
                                activity.ActivityType = ActivityType.Create.ToString();
                            }
                            break;

                        case EntityState.Deleted:
                            {
                                activity.OriginalValues[prop.Metadata.Name] = prop.OriginalValue;
                                activity.ActivityType = ActivityType.Delete.ToString();
                            }
                            break;

                        case EntityState.Modified:
                            if (prop.IsModified)
                            {
                                activity.OriginalValues[prop.Metadata.Name] = prop.OriginalValue;
                                activity.CurrentValues[prop.Metadata.Name] = prop.CurrentValue;
                                activity.ActivityType = ActivityType.Update.ToString();
                            }
                            break;
                    }

                    //entity property is included... but containts sensitive data
                    if (_fieldsSensitive.Contains(prop.Metadata.Name))
                    {
                        activity.CurrentValues[prop.Metadata.Name] = "[********]";
                        activity.OriginalValues[prop.Metadata.Name] = "[********]";
                    }
                }

                var verify = _maps.Map<AppActivity>(activity);

                if(verify.OriginalValues != verify.CurrentValues)
                    activityList.Add(activity);
            }

            //save entities that have all the changes
            foreach (var entry in activityList.Where(x => !x.HasTemporaryProperties))
                AppActivity.Add(_maps.Map<AppActivity>(entry));

            //add to list of entries where some property values are unknown
            return activityList.Where(x => x.HasTemporaryProperties).ToList();
        }

        private Task OnSaveActivityAfter(List<ActivityCreate> entryList)
        {
            if (entryList == null || entryList.Count == 0)
                return Task.CompletedTask;

            foreach (var entry in entryList)
            {
                //get final value of temporary properties
                foreach (var prop in entry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                        entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    else
                        entry.CurrentValues[prop.Metadata.Name] = prop.CurrentValue;
                }

                AppActivity.Add(_maps.Map<AppActivity>(entry));
            }

            return SaveChangesAsync();
        }
    }
}
