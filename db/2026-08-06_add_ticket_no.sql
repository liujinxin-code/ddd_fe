-- ============================================================
-- tk_ticket 新增 ticket_no（工单编号）列
-- 执行环境：MySQL 8.0
-- 说明：你的线上库已手动加了该列（DEFAULT NULL）。此脚本仅用于全新/未加列的环境补齐，
--       若列已存在，直接执行会报 "Duplicate column name" 错误，可忽略或先 DROP 再执行。
--       后端在提交工单时用 Utils.GenerateSerialNo(serialNoPre:"T") 生成单号写入该列。
-- ============================================================

ALTER TABLE `tk_ticket`
  ADD COLUMN `ticket_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL COMMENT '工单编号'
  AFTER `ticket_id`;
