<script setup>
import { onMounted, onUnmounted, reactive, ref, computed } from 'vue'
import { Button, Empty, Input, Modal, Pagination, Select, Spin, Table, Tag, message } from 'ant-design-vue'
import { enumApi, orderApi } from '../../api'
import ExecutionModal from '../../components/order/ExecutionModal.vue'
import { formatMoney } from '../../utils/format'

const orders = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ keyword: '', state: undefined, page: 1, pageSize: 10 })
const selectedOrder = ref(null)
const executionModalVisible = ref(false)

// ── 评论详情弹窗（评论业务展示下单时提交的评论内容） ──
const commentModalVisible = ref(false)
const commentOrder = ref(null)
const openComment = (order) => {
  commentOrder.value = order
  commentModalVisible.value = true
}
const closeComment = () => { commentModalVisible.value = false }

// ── 响应式断点 ──────────────────────────────────────────────
const isMobile = ref(false)
const MOBILE_BREAKPOINT = 768

const checkMobile = () => {
  isMobile.value = window.innerWidth < MOBILE_BREAKPOINT
}

let resizeObserver = null
onMounted(async () => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  try {
    const result = await enumApi.orderState()
    enumOptions.value = result.data || []
  } catch (error) {
    message.error(error.message || '加载枚举失败')
  }
  loadOrders()
})
onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

// 订单状态：从后端枚举同步，避免前后端文案不一致
const enumOptions = ref([])
const stateOptions = computed(() => [
  { value: 0, label: '全部状态' },
  ...(enumOptions.value || []).map((item) => ({ value: item.value, label: item.label })),
])
const stateMeta = computed(() => {
  const map = {}
  const colorMap = {
    1: 'processing', // 正在执行
    2: 'success',    // 已完单
    3: 'warning',    // 部分完成
    4: 'default',    // 已取消
  }
  for (const item of enumOptions.value || []) {
    map[item.value] = { text: item.label, color: colorMap[item.value] || 'default' }
  }
  return map
})

// 增量业务（粉丝 1 / 评论 2）才有执行详情；账户业务（3）一次性交付，无执行过程
const isIncremental = (order) => [1, 2].includes(Number(order?.jsonTemplate))

// 金额展示：对齐后端 decimal(11,6)，最多 6 位小数并去尾零
const fmtMoney = formatMoney

const fmtTime = (value) => {
  if (!value) return '-'
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString('zh-CN', { hour12: false })
}

// 移动端：截断订单号（保留前12+后4）
const shortOrderNo = (no) => {
  if (!no || no.length <= 18) return no
  return no.slice(0, 12) + '...' + no.slice(-4)
}

// ── 桌面端表格列定义（仅桌面端使用） ────────────────────────
const columns = [
  { title: '订单号', dataIndex: 'orderNo', key: 'orderNo', width: 200, fixed: 'left' },
  { title: '状态', dataIndex: 'orderState', key: 'orderState', width: 108, align: 'center' },
  { title: '平台类型', dataIndex: 'platformName', key: 'platformName', width: 104 },
  { title: '业务类型', dataIndex: 'subPlatformName', key: 'subPlatformName', width: 110 },
  { title: '下单链接', dataIndex: 'orderLink', key: 'orderLink', width: 220, ellipsis: true },
  { title: '订单金额', dataIndex: 'orderAmount', key: 'orderAmount', width: 110, align: 'right' },
  { title: '下单数量', dataIndex: 'quantity', key: 'quantity', width: 96, align: 'right' },
  { title: '成功数量', dataIndex: 'successQuantity', key: 'successQuantity', width: 96, align: 'right' },
  { title: '退费金额', dataIndex: 'refundAmount', key: 'refundAmount', width: 104, align: 'right' },
  { title: '下单时间', dataIndex: 'createTime', key: 'createTime', width: 172 },
  { title: '操作', key: 'action', width: 108, align: 'center', fixed: 'right' },
]

const loadOrders = async () => {
  loading.value = true
  try {
    const result = await orderApi.list(query)
    orders.value = result.data || []
    total.value = result.count || 0
  } catch (error) {
    message.error(error.message)
  } finally {
    loading.value = false
  }
}

const search = () => {
  query.page = 1
  loadOrders()
}

const onPageChange = (page, pageSize) => {
  query.page = page
  query.pageSize = pageSize
  loadOrders()
}

const openExecution = (order) => {
  selectedOrder.value = {
    ...order,
    quantity: order.beginQuantity ?? order.quantity,
    successCount: order.successQuantity,
  }
  executionModalVisible.value = true
}
</script>

<template>
  <div class="order-page">
    <header class="page-header">
      <h2>订单列表</h2>
      <p>查看订单状态、执行数量与退费情况</p>
    </header>

    <section class="panel">
      <!-- 筛选工具栏（双端共用） -->
      <div class="toolbar">
        <h3>我的订单</h3>
        <div class="filters">
          <Select
            v-model:value="query.state"
            :options="stateOptions"
            placeholder="全部状态"
            allow-clear
            :style="{ width: isMobile ? '100%' : '168px' }"
            @change="search"
          />
          <Input.Search
            v-model:value="query.keyword"
            placeholder="订单号或链接"
            allow-clear
            :style="{ width: isMobile ? '100%' : '240px' }"
            @search="search"
          />
        </div>
      </div>

      <!-- ═══════ 桌面端：表格 ═══════ -->
      <Spin v-if="!isMobile" :spinning="loading">
        <Table
          v-if="orders.length"
          :columns="columns"
          :data-source="orders"
          :pagination="false"
          row-key="orderNo"
          size="middle"
          :scroll="{ x: 1528 }"
          class="order-table"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'orderNo'">
              <div class="cell-order">
                <span class="order-no">{{ record.orderNo }}</span>
                <span v-if="record.configName" class="config-name">{{ record.configName }}</span>
              </div>
            </template>

            <template v-else-if="column.key === 'orderState'">
              <Tag :color="stateMeta[record.orderState]?.color || 'default'">
                {{ stateMeta[record.orderState]?.text || `状态 ${record.orderState}` }}
              </Tag>
            </template>

            <template v-else-if="column.key === 'orderLink'">
              <a
                v-if="record.orderLink"
                :href="record.orderLink"
                target="_blank"
                rel="noopener noreferrer"
                :title="record.orderLink"
                class="order-link"
              >{{ record.orderLink }}</a>
              <span v-else class="muted">—</span>
            </template>

            <template v-else-if="column.key === 'orderAmount'">
              <span class="amount">¥{{ fmtMoney(record.orderAmount) }}</span>
            </template>

            <template v-else-if="column.key === 'successQuantity'">
              <span :class="{ 'qty-partial': record.successQuantity < record.quantity }">
                {{ record.successQuantity }}
              </span>
            </template>

            <template v-else-if="column.key === 'refundAmount'">
              <span :class="Number(record.refundAmount) > 0 ? 'amount-refund' : 'muted'">
                ¥{{ fmtMoney(record.refundAmount) }}
              </span>
            </template>

            <template v-else-if="column.key === 'createTime'">
              {{ fmtTime(record.createTime) }}
            </template>

            <template v-else-if="column.key === 'action'">
              <Button v-if="record.jsonTemplate === 2" type="link" size="small" @click="openComment(record)">
                评论详情
              </Button>
              <Button v-else-if="record.jsonTemplate === 1" type="link" size="small" @click="openExecution(record)">
                执行详情
              </Button>
              <span v-else class="muted">—</span>
            </template>
          </template>
        </Table>

        <Empty v-else-if="!loading" description="暂无订单" />
      </Spin>

      <!-- ═══════ 移动端：卡片列表 ═══════ -->
      <div v-else class="mobile-list">
        <Spin :spinning="loading">
          <div v-if="orders.length" class="mobile-cards">
            <div
              v-for="order in orders"
              :key="order.orderNo"
              class="order-card"
            >
              <!-- 卡片头部：订单号 + 状态 -->
              <div class="card-head">
                <div class="card-order-no" :title="order.orderNo">
                  📋 {{ shortOrderNo(order.orderNo) }}
                </div>
                <Tag
                  :color="stateMeta[order.orderState]?.color || 'default'"
                  class="card-tag"
                >
                  {{ stateMeta[order.orderState]?.text || `状态${order.orderState}` }}
                </Tag>
              </div>

              <!-- 平台/业务类型 -->
              <div class="card-meta-row">
                <span class="meta-badge platform">{{ order.platformName || '-' }}</span>
                <span class="meta-badge sub">{{ order.subPlatformName || '-' }}</span>
              </div>

              <!-- 核心数据网格 -->
              <div class="card-stats">
                <div class="stat-item">
                  <span class="stat-label">订单金额</span>
                  <span class="stat-val amount">¥{{ fmtMoney(order.orderAmount) }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">下单数量</span>
                  <span class="stat-val">{{ order.quantity }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">成功数量</span>
                  <span
                    class="stat-val"
                    :class="{ 'qty-partial': order.successQuantity < order.quantity }"
                  >{{ order.successQuantity }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">退费金额</span>
                  <span
                    class="stat-val"
                    :class="Number(order.refundAmount) > 0 ? 'amount-refund' : 'muted'"
                  >¥{{ fmtMoney(order.refundAmount) }}</span>
                </div>
              </div>

              <!-- 下单链接 -->
              <div v-if="order.orderLink" class="card-link-row">
                <a
                  :href="order.orderLink"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="card-link"
                >🔗 {{ order.orderLink }}</a>
              </div>

              <!-- 底部：时间 + 操作 -->
              <div class="card-foot">
                <span class="card-time">{{ fmtTime(order.createTime) }}</span>
                <Button
                  v-if="order.jsonTemplate === 2"
                  type="primary"
                  size="small"
                  ghost
                  @click="openComment(order)"
                >评论详情</Button>
                <Button
                  v-else-if="order.jsonTemplate === 1"
                  type="primary"
                  size="small"
                  ghost
                  @click="openExecution(order)"
                >执行详情</Button>
                <span v-else class="muted">—</span>
              </div>
            </div>
          </div>

          <Empty v-else-if="!loading" description="暂无订单" />
        </Spin>
      </div>

      <!-- 分页（双端共用） -->
      <div v-if="total > 0" class="pager">
        <Pagination
          :current="query.page"
          :page-size="query.pageSize"
          :total="total"
          :simple="isMobile"
          show-size-changer
          :show-total="(t) => `共 ${t} 条`"
          @change="onPageChange"
          @show-size-change="onPageChange"
        />
      </div>
    </section>

    <ExecutionModal v-model:open="executionModalVisible" :initial-values="selectedOrder" />

    <!-- ═══════ 评论详情弹窗（评论业务下单内容） ═══════ -->
    <Modal
      v-model:open="commentModalVisible"
      :title="commentOrder ? `评论详情 · ${commentOrder.orderNo}` : '评论详情'"
      :width="isMobile ? '95vw' : '520px'"
      :footer="null"
      @cancel="closeComment"
    >
      <div v-if="commentOrder" class="comment-detail">
        <div v-if="commentOrder.comments && commentOrder.comments.length" class="comment-list">
          <div v-for="(c, i) in commentOrder.comments" :key="i" class="comment-item">
            <span class="comment-index">{{ i + 1 }}</span>
            <span class="comment-text">{{ c }}</span>
          </div>
        </div>
        <Empty v-else description="该订单暂无评论内容" />
      </div>
    </Modal>
  </div>
</template>

<style scoped lang="scss">
$bg-card: #ffffff;
$border-base: #e5e9ef;
$text-primary: #1e293b;
$text-secondary: #64748b;
$text-muted: #94a3b8;
$color-primary: #586ee1;
$color-accent: #f59e0b;
$r-md: 12px;

.order-page {
  display: grid;
  gap: 1rem;
  padding: 0.75rem 0 1.25rem;
}

.page-header {
  h2 { margin: 0; font-size: 1.15rem; font-weight: 600; color: $text-primary; }
  p { margin: 0.25rem 0 0; font-size: 0.82rem; color: $text-muted; }
}

.panel {
  background: $bg-card;
  border: 1px solid $border-base;
  border-radius: $r-md;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
  padding: 1rem;
  display: grid;
  gap: 0.9rem;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  flex-wrap: wrap;

  h3 { margin: 0; font-size: 0.95rem; font-weight: 600; color: $text-primary; }
}

.filters {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  flex-wrap: wrap;
}

/* ═══════ 桌面端表格样式 ═══════ */
.order-table {
  :deep(.ant-table) { font-size: 0.82rem; }
  :deep(.ant-table-thead > tr > th) {
    background: #f8fafb;
    color: $text-secondary;
    font-weight: 600;
    white-space: nowrap;
  }
  :deep(.ant-table-tbody > tr:hover > td) { background: #f6f8ff; }
}

.cell-order {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}
.order-no { font-weight: 600; color: $text-primary; word-break: break-all; }
.config-name { font-size: 0.74rem; color: $text-muted; }

.order-link {
  color: $color-primary;
  &:hover { text-decoration: underline; }
}

.amount { font-weight: 600; color: $text-primary; }
.amount-refund { font-weight: 600; color: $color-accent; }
.qty-partial { color: $color-accent; font-weight: 600; }
.muted { color: $text-muted; }

.pager {
  display: flex;
  justify-content: flex-end;
}

/* ═══════ 移动端卡片样式 ═══════ */
.mobile-list {
  min-height: 120px;
}

.mobile-cards {
  display: grid;
  gap: 0.75rem;
}

.order-card {
  background: #fafbfc;
  border: 1px solid $border-base;
  border-radius: 10px;
  padding: 0.85rem 0.75rem;
  display: grid;
  gap: 0.6rem;
  transition: box-shadow 0.2s;

  &:active {
    box-shadow: 0 2px 8px rgba(88, 110, 225, 0.12);
  }
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.card-order-no {
  font-size: 0.78rem;
  font-weight: 600;
  color: $text-primary;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 0;
  flex: 1;
}

.card-tag {
  flex-shrink: 0;
  font-size: 0.72rem;
}

.card-meta-row {
  display: flex;
  gap: 0.45rem;
  flex-wrap: wrap;
}

.meta-badge {
  display: inline-block;
  font-size: 0.72rem;
  padding: 0.15rem 0.5rem;
  border-radius: 6px;
  background: #eff0f3;
  color: $text-secondary;
  line-height: 1.4;

  &.platform {
    background: #eef2ff;
    color: $color-primary;
  }
  &.sub {
    background: #fff8ec;
    color: #c27b00;
  }
}

.card-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.4rem;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.15rem;
  background: #fff;
  border: 1px solid #eceef3;
  border-radius: 8px;
  padding: 0.45rem 0.25rem;
}

.stat-label {
  font-size: 0.66rem;
  color: $text-muted;
  white-space: nowrap;
}

.stat-val {
  font-size: 0.85rem;
  font-weight: 600;
  color: $text-primary;
}

.card-link-row {
  overflow: hidden;
}

.card-link {
  display: block;
  font-size: 0.74rem;
  color: $color-primary;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 1.5;

  &:hover { text-decoration: underline; opacity: 0.85; }
}

.card-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 0.2rem;
  border-top: 1px dashed #eaecef;
}

.card-time {
  font-size: 0.72rem;
  color: $text-muted;
}

/* ═══════ 评论详情弹窗 ═══════ */
.comment-detail { display: grid; gap: 0.6rem; padding-top: 0.25rem; }
.comment-list { display: grid; gap: 0.55rem; }
.comment-item {
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
  background: #f8fafb;
  border: 1px solid #eef1f5;
  border-radius: 8px;
  padding: 0.6rem 0.7rem;
  line-height: 1.6;
}
.comment-index {
  flex-shrink: 0;
  width: 20px;
  height: 20px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: $color-primary;
  color: #fff;
  font-size: 0.72rem;
  margin-top: 0.1rem;
}
.comment-text {
  font-size: 0.85rem;
  color: $text-primary;
  white-space: pre-wrap;
  word-break: break-word;
}

/* ═══════ 响应式微调 ═══════ */
@media (max-width: 768px) {
  .order-page {
    padding: 0.5rem 0 1rem;
  }

  .panel {
    padding: 0.75rem;
    border-radius: 10px;
  }

  .toolbar {
    flex-direction: column;
    align-items: stretch;

    h3 { font-size: 0.9rem; }
  }

  .filters {
    flex-direction: column;
    width: 100%;

    .ant-select,
    .ant-input-search { width: 100% !important; }
  }

  /* 手机上 4 列统计改为 2×2 */
  .card-stats {
    grid-template-columns: repeat(2, 1fr);
  }

  .pager {
    justify-content: center;
  }
}

/* 超小屏（<380px）适配 */
@media (max-width: 380px) {
  .card-stats {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.35rem;
  }

  .stat-item {
    padding: 0.4rem 0.2rem;
  }

  .stat-label { font-size: 0.62rem; }
  .stat-val { font-size: 0.8rem; }

  .order-card {
    padding: 0.7rem 0.6rem;
  }
}
</style>
