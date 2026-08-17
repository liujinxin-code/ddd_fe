<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { Empty, Input, Pagination, Select, Spin, Table, Tag, message } from 'ant-design-vue'
import { authApi, enumApi } from '../../api'
import { formatMoney } from '../../utils/format'

const records = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ keyword: '', status: undefined, page: 1, pageSize: 10 })

// ── 响应式断点（与订单列表一致） ──────────────────────────
const isMobile = ref(false)
const MOBILE_BREAKPOINT = 768
const checkMobile = () => { isMobile.value = window.innerWidth < MOBILE_BREAKPOINT }
onMounted(async () => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  try {
    const result = await enumApi.consumeStatus()
    enumOptions.value = result.data || []
  } catch (error) {
    message.error(error.message || '加载枚举失败')
  }
  loadRecords()
})
onUnmounted(() => window.removeEventListener('resize', checkMobile))

// 变动类型：从后端枚举同步，避免前后端文案不一致
const enumOptions = ref([])
const typeOptions = computed(() => [
  { value: -1, label: '全部类型' },
  ...(enumOptions.value || []).map((item) => ({ value: item.value, label: item.label })),
])
const typeMeta = computed(() => {
  const map = {}
  const colorMap = {
    0: 'volcano',   // 订单消费
    1: 'green',     // 充值
    2: 'green',     // 代理提现(到账)
    3: 'red',       // 转赠支出
    4: 'green',     // 转赠收入
    5: 'cyan',      // 订单退款
    6: 'orange',    // 代理收益扣减
  }
  for (const item of enumOptions.value || []) {
    map[item.value] = { text: item.label, color: colorMap[item.value] || 'default' }
  }
  return map
})

// 金额展示：对齐后端 decimal(11,6)，最多 6 位小数并去尾零
const fmtMoney = formatMoney
const fmtTime = (value) => {
  if (!value) return '-'
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString('zh-CN', { hour12: false })
}
const isIncome = (r) => Number(r.changeAmount) >= 0

// ── 桌面端表格列定义 ──────────────────────────────────────
const columns = [
  { title: '流水号', dataIndex: 'consumeNo', key: 'consumeNo', width: 220, fixed: 'left' },
  { title: '类型', dataIndex: 'consumeStatus', key: 'consumeStatus', width: 150, align: 'center' },
  { title: '变动额', dataIndex: 'changeAmount', key: 'changeAmount', width: 130, align: 'right' },
  { title: '时间', dataIndex: 'createTime', key: 'createTime', width: 172 },
]

const loadRecords = async () => {
  loading.value = true
  try {
    const result = await authApi.consumptions(query)
    records.value = result.data || []
    total.value = result.count || 0
  } catch (error) {
    message.error(error.message)
  } finally {
    loading.value = false
  }
}
const search = () => { query.page = 1; loadRecords() }
const onPageChange = (page, pageSize) => {
  query.page = page
  query.pageSize = pageSize
  loadRecords()
}
</script>

<template>
  <div class="consume-page">
    <header class="page-header">
      <h2>消费流水</h2>
      <p>查看余额每一次变化的来源与去向（订单消费、充值、提现、转赠、退款）</p>
    </header>

    <section class="panel">
      <!-- 筛选工具栏（双端共用） -->
      <div class="toolbar">
        <h3>余额明细</h3>
        <div class="filters">
          <Select
            v-model:value="query.status"
            :options="typeOptions"
            placeholder="全部类型"
            allow-clear
            :style="{ width: isMobile ? '100%' : '168px' }"
            @change="search"
          />
          <Input.Search
            v-model:value="query.keyword"
            placeholder="搜索流水号"
            allow-clear
            :style="{ width: isMobile ? '100%' : '240px' }"
            @search="search"
          />
        </div>
      </div>

      <!-- ═══════ 桌面端：表格 ═══════ -->
      <Spin v-if="!isMobile" :spinning="loading">
        <Table
          v-if="records.length"
          :columns="columns"
          :data-source="records"
          :pagination="false"
          row-key="consumeId"
          size="middle"
          :scroll="{ x: 700 }"
          class="consume-table"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'consumeNo'">
              <span class="order-no" :title="record.consumeNo">{{ record.consumeNo }}</span>
            </template>

            <template v-else-if="column.key === 'consumeStatus'">
              <Tag :color="typeMeta[record.consumeStatus]?.color || 'default'">
                {{ typeMeta[record.consumeStatus]?.text || `类型 ${record.consumeStatus}` }}
              </Tag>
            </template>

            <template v-else-if="column.key === 'changeAmount'">
              <span :class="isIncome(record) ? 'amount-income' : 'amount-expense'">
                {{ isIncome(record) ? '+' : '-' }}¥{{ fmtMoney(Math.abs(Number(record.changeAmount))) }}
              </span>
            </template>

            <template v-else-if="column.key === 'createTime'">
              {{ fmtTime(record.createTime) }}
            </template>
          </template>
        </Table>

        <Empty v-else-if="!loading" description="暂无余额流水" />
      </Spin>

      <!-- ═══════ 移动端：卡片列表 ═══════ -->
      <div v-else class="mobile-list">
        <Spin :spinning="loading">
          <div v-if="records.length" class="mobile-cards">
            <div v-for="record in records" :key="record.consumeId" class="consume-card">
              <!-- 卡片头部：流水号 + 类型 -->
              <div class="card-head">
                <div class="card-no" :title="record.consumeNo">🔁 {{ record.consumeNo }}</div>
                <Tag :color="typeMeta[record.consumeStatus]?.color || 'default'" class="card-tag">
                  {{ typeMeta[record.consumeStatus]?.text || `类型${record.consumeStatus}` }}
                </Tag>
              </div>

              <!-- 核心数据网格 -->
              <div class="card-stats">
                <div class="stat-item stat-full">
                  <span class="stat-label">变动额</span>
                  <span :class="isIncome(record) ? 'stat-val amount-income' : 'stat-val amount-expense'">
                    {{ isIncome(record) ? '+' : '-' }}¥{{ fmtMoney(Math.abs(Number(record.changeAmount))) }}
                  </span>
                </div>
              </div>

              <!-- 底部：时间 -->
              <div class="card-foot">
                <span class="card-time">{{ fmtTime(record.createTime) }}</span>
              </div>
            </div>
          </div>

          <Empty v-else-if="!loading" description="暂无余额流水" />
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
  </div>
</template>

<style scoped lang="scss">
$bg-card: #ffffff;
$border-base: #e5e9ef;
$text-primary: #1e293b;
$text-secondary: #64748b;
$text-muted: #94a3b8;
$color-primary: #586ee1;
$color-income: #16a34a;
$color-expense: #ef4444;
$color-accent: #f59e0b;
$r-md: 12px;

.consume-page {
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
.consume-table {
  :deep(.ant-table) { font-size: 0.82rem; }
  :deep(.ant-table-thead > tr > th) {
    background: #f8fafb;
    color: $text-secondary;
    font-weight: 600;
    white-space: nowrap;
  }
  :deep(.ant-table-tbody > tr:hover > td) { background: #f6f8ff; }
}

.order-no { font-weight: 600; color: $text-primary; word-break: break-all; }
.muted { color: $text-muted; }
.amount-income { font-weight: 600; color: $color-income; }
.amount-expense { font-weight: 600; color: $color-expense; }

.pager { display: flex; justify-content: flex-end; }

/* ═══════ 移动端卡片样式 ═══════ */
.mobile-list { min-height: 120px; }
.mobile-cards { display: grid; gap: 0.75rem; }

.consume-card {
  background: #fafbfc;
  border: 1px solid $border-base;
  border-radius: 10px;
  padding: 0.85rem 0.75rem;
  display: grid;
  gap: 0.6rem;
  transition: box-shadow 0.2s;

  &:active { box-shadow: 0 2px 8px rgba(88, 110, 225, 0.12); }
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}
.card-no {
  font-size: 0.78rem;
  font-weight: 600;
  color: $text-primary;
  word-break: break-all;
  overflow-wrap: anywhere;
  white-space: normal;
  min-width: 0;
  flex: 1;
}
.card-tag { flex-shrink: 0; font-size: 0.72rem; }

.card-stats {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.4rem;
}
.stat-item {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  background: #fff;
  border: 1px solid #eceef3;
  border-radius: 8px;
  padding: 0.45rem 0.5rem;
}
.stat-full { grid-column: 1 / -1; }
.stat-label { font-size: 0.68rem; color: $text-muted; }
.stat-val { font-size: 0.85rem; font-weight: 600; color: $text-primary; }

.card-foot {
  padding-top: 0.2rem;
  border-top: 1px dashed #eaecef;
}
.card-time { font-size: 0.72rem; color: $text-muted; }

/* ═══════ 响应式微调 ═══════ */
@media (max-width: 768px) {
  .consume-page { padding: 0.5rem 0 1rem; }
  .panel { padding: 0.75rem; border-radius: 10px; }
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
  .pager { justify-content: center; }
}

@media (max-width: 380px) {
  .stat-item { padding: 0.4rem 0.35rem; }
  .stat-label { font-size: 0.64rem; }
  .stat-val { font-size: 0.8rem; }
  .consume-card { padding: 0.7rem 0.6rem; }
}
</style>
