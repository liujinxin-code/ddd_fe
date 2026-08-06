-- ============================================================
-- 新增客服工单表 tk_ticket
-- 执行环境：MySQL 8.0（与 Pomelo 驱动一致）
-- 说明：需手动在数据库执行；EF 实体 TkTicket 已映射下列名（tikcket_status 为建表拼写，保持原样）。
-- ============================================================

CREATE TABLE `tk_ticket` (
  `ticket_id` int NOT NULL AUTO_INCREMENT,
  `ticket_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT '工单编号',
  `ticket_content` varchar(3000) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT '工单内容',
  `ticket_images` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT '工单图片 ["",""] 最多五张图片',
  `ticket_result` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL COMMENT '处理结果（后台系统填写）',
  `tikcket_status` int NOT NULL COMMENT '0/待处理  1已处理',
  `create_time` datetime NOT NULL,
  `ticket_type` int DEFAULT NULL COMMENT '问题类型 0/订单问题 1/下单问题 2/网站问题 3/网站建议',
  `userid` int DEFAULT NULL COMMENT '用户id',
  PRIMARY KEY (`ticket_id`),
  KEY `ix_ticket_userid` (`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='客服工单';
