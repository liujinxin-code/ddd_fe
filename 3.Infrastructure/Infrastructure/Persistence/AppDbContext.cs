using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// EF Core 数据库上下文。
    /// 目前映射 tk_user / tk_consumelog / tk_platform / tk_platform_sub 表，后续新增表时继续在这里添加 DbSet 和 Fluent API 配置。
    /// </summary>
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        //数据库表集合
        public DbSet<TkUser> TkUsers => Set<TkUser>();
        public DbSet<ConsumeLog> ConsumeLogs => Set<ConsumeLog>();
        public DbSet<TkPlatform> TkPlatforms => Set<TkPlatform>();
        public DbSet<TkPlatformSub> TkPlatformSubs => Set<TkPlatformSub>();
        /// <summary>
        /// 使用 Fluent API 显式映射数据库字段。
        /// 这样领域实体可以使用 C# 风格命名，不必被数据库下划线字段名污染。
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TkUser>(entity =>
            {
                entity.ToTable("tk_user");
                entity.HasKey(x => x.Userid);

                entity.Property(x => x.Userid).HasColumnName("userid");
                entity.Property(x => x.Email).HasColumnName("email");
                entity.Property(x => x.Username).HasColumnName("username");
                entity.Property(x => x.Password).HasColumnName("password");
                entity.Property(x => x.UserStatus).HasColumnName("user_status");
                entity.Property(x => x.UserAmount).HasColumnName("user_amount").HasPrecision(10, 6);
                entity.Property(x => x.IsAgent).HasColumnName("is_agent");
                entity.Property(x => x.AgentAmount).HasColumnName("agent_amount").HasPrecision(10, 6);
                entity.Property(x => x.AgentUserid).HasColumnName("agent_userid");
                entity.Property(x => x.AgentDomain).HasColumnName("agent_domain");
                entity.Property(x => x.Createby).HasColumnName("createby");
                entity.Property(x => x.ApiKey).HasColumnName("api_key");
                entity.Property(x => x.UserVersion).HasColumnName("user_version").IsConcurrencyToken();
                entity.Property(x => x.IsDelete).HasColumnName("is_delete");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.Property(x => x.SignleClient).HasColumnName("signle_client");

                entity.HasIndex(x => x.Username).IsUnique().HasDatabaseName("ux_username");
                entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("ux_email");
            });

            modelBuilder.Entity<ConsumeLog>(entity =>
            {
                entity.ToTable("tk_consumelog");
                entity.HasKey(x => x.ConsumeId);
                entity.Property(x => x.ConsumeId).HasColumnName("consume_id");
                entity.Property(x => x.BeforeAmount).HasColumnName("ago_amount").HasPrecision(10, 6);
                entity.Property(x => x.AfterAmount).HasColumnName("after_amount").HasPrecision(10, 6);
                entity.Property(x => x.ConsumeStatus).HasColumnName("consume_status").HasConversion<int>();
                entity.Property(x => x.ConsumeNo).HasColumnName("consume_no").HasMaxLength(255);
                entity.Property(x => x.UserId).HasColumnName("userid");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkPlatform>(entity =>
            {
                entity.ToTable("tk_platform");
                entity.HasKey(x => x.PlatformId);
                entity.Property(x => x.PlatformId).HasColumnName("platform_id");
                entity.Property(x => x.PlatformImg).HasColumnName("platform_img").HasMaxLength(255);
                entity.Property(x => x.PlatformName).HasColumnName("platform_name").HasMaxLength(255);
                entity.Property(x => x.PlatformStatus).HasColumnName("platform_status");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkPlatformSub>(entity =>
            {
                entity.ToTable("tk_platform_sub");
                entity.HasKey(x => x.SubPlatformId);
                entity.Property(x => x.SubPlatformId).HasColumnName("sub_platform_id");
                entity.Property(x => x.SubPlatformName).HasColumnName("sub_platform_name").HasMaxLength(255);
                entity.Property(x => x.PlatformId).HasColumnName("platform_id");
                entity.Property(x => x.SubPlatformStatus).HasColumnName("sub_platform_status").HasConversion<int>();
                entity.Property(x => x.SubPlatformNotice).HasColumnName("sub_platform_notice").HasMaxLength(2000);
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });
        }
    }
}
