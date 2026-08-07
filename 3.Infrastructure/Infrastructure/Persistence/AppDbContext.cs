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
        public DbSet<TkConfig> TkConfigs => Set<TkConfig>();
        public DbSet<TkPriceUserCustom> TkPriceUserCustoms => Set<TkPriceUserCustom>();
        public DbSet<TkPriceOverall> TkPriceOveralls => Set<TkPriceOverall>();
        public DbSet<TkPriceAgentMarkup> TkPriceAgentMarkups => Set<TkPriceAgentMarkup>();
        public DbSet<TkNotice> TkNotices => Set<TkNotice>();
        public DbSet<TkOrder> TkOrders => Set<TkOrder>();
        public DbSet<TkComment> TkComments => Set<TkComment>();
        public DbSet<TkTicket> TkTickets => Set<TkTicket>();
        public DbSet<TkServiceImage> TkServiceImages => Set<TkServiceImage>();
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
                entity.Property(x => x.DeleteTime).HasColumnName("delete_time");
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

            modelBuilder.Entity<TkConfig>(entity =>
            {
                entity.ToTable("tk_config");
                entity.HasKey(x => x.ConfigId);
                entity.Property(x => x.ConfigId).HasColumnName("config_id");
                entity.Property(x => x.ConfigName).HasColumnName("config_name").HasMaxLength(255);
                entity.Property(x => x.ConfigPrice).HasColumnName("config_price").HasPrecision(10, 6);
                entity.Property(x => x.ShowPriceUnit).HasColumnName("show_price_unit");
                entity.Property(x => x.OrderUnit).HasColumnName("order_unit");
                entity.Property(x => x.ConfigNotice).HasColumnName("config_notice").HasMaxLength(1000);
                entity.Property(x => x.PlatformId).HasColumnName("platform_id");
                entity.Property(x => x.SubPlatformId).HasColumnName("sub_platform_id");
                entity.Property(x => x.ChannelId).HasColumnName("channel_id");
                entity.Property(x => x.ChannelServerId).HasColumnName("channel_server_id");
                entity.Property(x => x.MinQuantity).HasColumnName("min_quantity");
                entity.Property(x => x.MaxQuantity).HasColumnName("max_quantity");
                entity.Property(x => x.ConfigSort).HasColumnName("config_sort");
                entity.Property(x => x.ConfigStatus).HasColumnName("config_status").HasConversion<int>();
                entity.Property(x => x.JsonTemplate).HasColumnName("json_template").HasConversion<int>();
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkPriceUserCustom>(entity =>
            {
                entity.ToTable("tk_price_user_custom");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.CustomPrice).HasColumnName("custom_price").HasPrecision(10, 6);
                entity.Property(x => x.UserId).HasColumnName("userid");
                entity.Property(x => x.ConfigId).HasColumnName("config_id");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkPriceOverall>(entity =>
            {
                entity.ToTable("tk_price_overall");
                entity.HasKey(x => x.OverallId);
                entity.Property(x => x.OverallId).HasColumnName("overall_id");
                entity.Property(x => x.OverallPercent).HasColumnName("overall_percent");
                entity.Property(x => x.UserId).HasColumnName("userid");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("un_userid");
            });

            modelBuilder.Entity<TkPriceAgentMarkup>(entity =>
            {
                entity.ToTable("tk_price_agent_markup");
                entity.HasKey(x => x.MarkupId);
                entity.Property(x => x.MarkupId).HasColumnName("markup_id");
                entity.Property(x => x.MarkupAddPrice).HasColumnName("markup_add_price").HasPrecision(10, 6);
                entity.Property(x => x.ConfigId).HasColumnName("config_id");
                entity.Property(x => x.AgentUserId).HasColumnName("agent_userid");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkNotice>(entity =>
            {
                entity.ToTable("tk_notice");
                entity.HasKey(x => x.NoticeId);
                entity.Property(x => x.NoticeId).HasColumnName("notice_id").ValueGeneratedNever();
                entity.Property(x => x.NoticeContent).HasColumnName("notice_content").HasMaxLength(2000);
                entity.Property(x => x.NoticeType).HasColumnName("notice_type").HasConversion<int>();
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
            });

            modelBuilder.Entity<TkOrder>(entity =>
            {
                entity.ToTable("tk_order");
                entity.HasKey(x => x.OrderId);
                entity.Property(x => x.OrderId).HasColumnName("order_id").ValueGeneratedOnAdd();
                entity.Property(x => x.OrderNo).HasColumnName("order_no").HasMaxLength(50).IsRequired();
                entity.Property(x => x.OrderState).HasColumnName("order_state").HasConversion<int>();
                entity.Property(x => x.OrderLink).HasColumnName("order_link").HasMaxLength(500).IsRequired();
                entity.Property(x => x.ConfigId).HasColumnName("config_id");
                entity.Property(x => x.Userid).HasColumnName("userid");
                entity.Property(x => x.OrderAmount).HasColumnName("order_amount").HasPrecision(11, 6);
                entity.Property(x => x.Quantity).HasColumnName("quantity");
                entity.Property(x => x.SuccessQuantity).HasColumnName("success_quantity");
                entity.Property(x => x.BeginQuantity).HasColumnName("begin_quantity");
                entity.Property(x => x.EndQuantity).HasColumnName("end_quantity");
                entity.Property(x => x.PushState).HasColumnName("push_state");
                entity.Property(x => x.SerialNo).HasColumnName("serial_no").HasMaxLength(50).IsRequired();
                entity.Property(x => x.ChannelId).HasColumnName("channel_id");
                entity.Property(x => x.ChannelServerId).HasColumnName("channel_server_id");
                entity.Property(x => x.AgentUserid).HasColumnName("agent_userid");
                entity.Property(x => x.AgentSingleAddPrice).HasColumnName("agent_single_add_price").HasPrecision(10, 6);
                entity.Property(x => x.IsDifference).HasColumnName("is_difference");
                entity.Property(x => x.AgentOrderAmount).HasColumnName("agent_order_amount").HasPrecision(10, 6);
                entity.Property(x => x.RefundAmount).HasColumnName("refund_amount").HasPrecision(11, 6);
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.HasIndex(x => x.OrderNo).IsUnique().HasDatabaseName("ux_order_no");
                entity.HasIndex(x => x.Userid).HasDatabaseName("ix_order_userid");

                // 评论作为订单聚合的子实体：一次 SaveChanges 内自动回填 tk_comment.order_id
                entity.HasMany(x => x.Comments)
                      .WithOne()
                      .HasForeignKey(c => c.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Navigation(x => x.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<TkTicket>(entity =>
            {
                entity.ToTable("tk_ticket");
                entity.HasKey(x => x.TicketId);
                entity.Property(x => x.TicketId).HasColumnName("ticket_id").ValueGeneratedOnAdd();
                entity.Property(x => x.TicketNo).HasColumnName("ticket_no").HasMaxLength(50);
                entity.Property(x => x.TicketContent).HasColumnName("ticket_content").HasMaxLength(3000).IsRequired();
                entity.Property(x => x.TicketImages).HasColumnName("ticket_images").HasMaxLength(2000).IsRequired();
                entity.Property(x => x.TicketResult).HasColumnName("ticket_result").HasMaxLength(2000).IsRequired();
                // 注意：数据库列名 tikcket_status 是建表时的拼写（少一个 c），此处保持列名映射不变。
                entity.Property(x => x.TicketStatus).HasColumnName("tikcket_status");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.Property(x => x.TicketType).HasColumnName("ticket_type");
                entity.Property(x => x.Userid).HasColumnName("userid");
                entity.HasIndex(x => x.Userid).HasDatabaseName("ix_ticket_userid");
            });

            modelBuilder.Entity<TkComment>(entity =>
            {
                entity.ToTable("tk_comment");
                entity.HasKey(x => x.CommentId);
                entity.Property(x => x.CommentId).HasColumnName("comment_id").ValueGeneratedOnAdd();
                entity.Property(x => x.OrderId).HasColumnName("order_id");
                entity.Property(x => x.CommentContent).HasColumnName("comment_content").HasMaxLength(500).IsRequired();
                entity.Property(x => x.CommentState).HasColumnName("comment_state");
                entity.Property(x => x.Userid).HasColumnName("userid");
                // 软删除统一走 DeleteAuditor：is_delete 标记 + delete_time 删除时间（预留换评论场景）
                // 库里 is_delete 是 int（非 tinyint(1)），显式声明 bool <-> int 转换，避免依赖驱动的隐式转型
                entity.Property(x => x.IsDelete).HasColumnName("is_delete").HasConversion<int>();
                entity.Property(x => x.DeleteTime).HasColumnName("delete_time");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.HasIndex(x => x.OrderId).HasDatabaseName("ix_comment_order_id");
            });

            modelBuilder.Entity<TkServiceImage>(entity =>
            {
                entity.ToTable("tk_service_image");
                entity.HasKey(x => x.ImageId);
                entity.Property(x => x.ImageId).HasColumnName("image_id").ValueGeneratedOnAdd();
                entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
                entity.Property(x => x.AgentUserid).HasColumnName("agent_userid");
                entity.Property(x => x.CreateTime).HasColumnName("create_time");
                entity.HasIndex(x => x.AgentUserid).IsUnique().HasDatabaseName("ux_agent_userid");
            });
        }
    }
}
