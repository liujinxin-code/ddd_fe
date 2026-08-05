<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { Empty, Input, InputNumber, Modal, Pagination, Select, Spin, message } from 'ant-design-vue'
import { homeApi, orderApi } from '../../api'
import { useAuth } from '../../stores/auth'
import { Icon } from '../../assets/js/iconUtils.js'

const auth = useAuth()

// ── 数据状态 ──
const platforms = ref([])
const subPlatforms = ref([])
const services = ref([])
const loading = ref(false)
const ordering = ref(false)
const keyword = ref('')
const platformId = ref(null)
const subPlatformId = ref(null)
const page = ref(1)
const pageSize = ref(12)
const total = ref(0)
const orderVisible = ref(false)
const selectedService = ref(null)
const orderForm = reactive({ linksText: '', quantity: null, commentsText: '' })

// 当前选中业务类型的公告内容（来自 subPlatformNotice 字段）
const currentSubNotice = computed(() => {
  if (!subPlatformId.value) return ''
  const found = subPlatforms.value.find((s) => s.subPlatformId === subPlatformId.value)
  return found?.subPlatformNotice || ''
})

// 选中的平台/业务类型名称（用于右侧面板标题）
const currentSubName = computed(() => {
  if (!subPlatformId.value) return ''
  return subPlatforms.value.find((s) => s.subPlatformId === subPlatformId.value)?.subPlatformName || ''
})

// ── 业务逻辑 ──
const isCommentTemplate = (service) => service?.jsonTemplate === 2
const isAccountTemplate = (service) => service?.jsonTemplate === 3

// 金额格式化：保留原始精度，不去尾零（decimal(10,6) 对齐）
// 不指定 maximumFractionDigits，让 toLocaleString 自动按数值决定小数位数
const fmtMoney = (value) => {
  const n = Number(value) || 0
  // 整数返回 "0" 而非 "0.00"；小数保留全部有效位
  return n.toLocaleString('en-US', {
    minimumFractionDigits: n % 1 === 0 ? 0 : 2,
    useGrouping: false,
  })
}

// 展示价：优先取后端已算好的 displayPrice（避免 JS 浮点误差），fallback 到 unitPrice × showPriceUnit
const displayPriceOf = (item) => {
  if (item?.displayPrice != null && item.displayPrice !== '') return Number(item.displayPrice)
  if (item?.unitPrice != null) return Number(item.unitPrice) * (Number(item?.showPriceUnit) || 1)
  return 0
}

// 按行拆分文本域，去空白与空行
const splitLines = (text) => (text || '').split('\n').map((x) => x.trim()).filter(Boolean)

// 订单条数：账户业务恒为 1 条；其余「一个链接 = 一条订单」
const orderCount = computed(() => {
  if (!selectedService.value) return 0
  if (isAccountTemplate(selectedService.value)) return 1
  return splitLines(orderForm.linksText).length
})

// 每条订单的数量：评论业务以评论条数为准，其余取输入数量
const perOrderQuantity = computed(() => {
  if (!selectedService.value) return 0
  if (isCommentTemplate(selectedService.value)) return splitLines(orderForm.commentsText).length
  return Number(orderForm.quantity) || 0
})

const estimatedAmount = computed(() => {
  const service = selectedService.value
  if (!service) return 0
  return (service.price / service.priceUnit) * perOrderQuantity.value * orderCount.value
})

// 评论业务：条数即订单数量。边输边提示当前条数，并即时校验业务的数量约束
const commentCountHint = computed(() => {
  const service = selectedService.value
  if (!service || !isCommentTemplate(service)) return { text: '', invalid: false }
  const count = perOrderQuantity.value
  if (count === 0) return { text: '评论条数 = 订单数量', invalid: false }
  if (service.minQuantity && count < service.minQuantity)
    return { text: `${count} 条，至少需 ${service.minQuantity} 条`, invalid: true }
  if (service.maxQuantity && count > service.maxQuantity)
    return { text: `${count} 条，最多 ${service.maxQuantity} 条`, invalid: true }
  if (service.orderUnit && count % service.orderUnit !== 0)
    return { text: `${count} 条，需为 ${service.orderUnit} 的整数倍`, invalid: true }
  return { text: `${count} 条 = 订单数量 ${count}`, invalid: false }
})

// 提交前的轻量可用性判断（详细提示仍在 submitOrder 内逐条给出）
const canSubmit = computed(() =>
  Boolean(selectedService.value) &&
  orderCount.value > 0 &&
  perOrderQuantity.value > 0 &&
  !commentCountHint.value.invalid,
)

const loadPlatforms = async () => {
  const result = await homeApi.platforms()
  platforms.value = result.data || []
}

const loadSubs = async () => {
  if (!platformId.value) {
    subPlatforms.value = []
    subPlatformId.value = null
    return
  }
  try {
    const result = await homeApi.subs(platformId.value)
    subPlatforms.value = result.data || []
    if (subPlatforms.value.length > 0) {
      subPlatformId.value = subPlatforms.value[0].subPlatformId
    } else {
      subPlatformId.value = null
    }
  } catch (error) {
    message.error(error.message)
  }
}

const loadServices = async () => {
  loading.value = true
  try {
    const result = await homeApi.configs({
      platformId: platformId.value || undefined,
      subPlatformId: subPlatformId.value || undefined,
      keyword: keyword.value.trim(),
      page: page.value,
      pageSize: pageSize.value,
    })
    services.value = result.data || []
    total.value = result.count
  } catch (error) {
    message.error(error.message)
  } finally {
    loading.value = false
  }
}

const platformOptions = computed(() =>
  platforms.value.map((p) => ({
    value: p.platformId,
    label: p.platformName || `平台 ${p.platformId}`,
  })),
)
const subPlatformOptions = computed(() =>
  subPlatforms.value.map((s) => ({
    value: s.subPlatformId,
    label: s.subPlatformName || `业务类型 ${s.subPlatformId}`,
  })),
)

// 下拉框输入检索：按 label 模糊匹配
const filterOption = (input, option) => {
  const label = typeof option?.label === 'string' ? option.label : String(option?.label || '')
  return label.toLowerCase().includes(input.toLowerCase())
}

const onPlatformChange = async () => {
  subPlatformId.value = null
  await loadSubs()
  search()
}

const onSubChange = () => {
  search()
}

const search = () => {
  page.value = 1
  loadServices()
}

// ── 防抖实时搜索：输入停止 300ms 后自动触发 ──
let debounceTimer = null
const onKeywordInput = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => search(), 300)
}
const clearKeyword = () => {
  keyword.value = ''
  search()
}

const openOrder = (service) => {
  selectedService.value = service
  Object.assign(orderForm, {
    linksText: '',
    quantity: service.minQuantity || service.orderUnit || 1,
    commentsText: '',
  })
  orderVisible.value = true
}

const submitOrder = async () => {
  const service = selectedService.value
  if (!service) return
  const isComment = isCommentTemplate(service)
  const isAccount = isAccountTemplate(service)

  // 账户业务无链接：占位一条空链接，服务端允许留空
  const links = isAccount ? [''] : splitLines(orderForm.linksText)
  if (!isAccount && !links.length) return message.warning('请至少输入一个链接，每行一个')
  if (links.length > 100) return message.warning('单次最多提交 100 条订单')
  const longLink = links.find((l) => l.length > 500)
  if (longLink) return message.warning('单个链接不能超过 500 字符')

  // 评论业务：一条评论 = 一个数量，评论会应用到每一个链接
  const comments = isComment ? splitLines(orderForm.commentsText) : []
  if (isComment) {
    if (!comments.length) return message.warning('请输入评论内容，每行一条')
    if (comments.some((c) => c.length > 500)) return message.warning('单条评论不能超过 500 字符')
  }

  const quantity = isComment ? comments.length : Number(orderForm.quantity)
  if (!quantity || quantity <= 0) return message.warning('请输入下单数量')
  if (service.minQuantity && quantity < service.minQuantity)
    return message.warning(`下单数量不能少于 ${service.minQuantity}`)
  if (service.maxQuantity && quantity > service.maxQuantity)
    return message.warning(`下单数量不能超过 ${service.maxQuantity}`)
  if (service.orderUnit && quantity % service.orderUnit !== 0)
    return message.warning(`下单数量必须是 ${service.orderUnit} 的整数倍`)

  ordering.value = true
  try {
    // 与后端 CreateOrderItem 对齐：一个链接一条明细；金额由服务端计算，前端不参与
    // 评论业务的数量恒等于评论条数，故不传 quantity，避免出现两个数量来源
    const items = links.map((link) =>
      isComment
        ? { configId: service.configId, orderLink: link, comments }
        : { configId: service.configId, orderLink: link, quantity },
    )
    const result = await orderApi.createBatch(items)
    const { orderNos = [], totalAmount = 0 } = result.data || {}
    message.success(`下单成功：${orderNos.length} 条订单，共扣款 ¥${Number(totalAmount).toFixed(2)}`)
    orderVisible.value = false
    // 静默刷新余额，失败不打断下单成功的反馈
    auth.loadUser().catch(() => {})
  } catch (error) {
    message.error(error.message)
  } finally {
    ordering.value = false
  }
}

onMounted(async () => {
  try {
    await loadPlatforms()
    if (platforms.value.length > 0 && !platformId.value) {
      platformId.value = platforms.value[0].platformId
      await loadSubs()
      await loadServices()
    } else {
      await loadServices()
    }
  } catch (error) {
    message.error(error.message)
  }
})
</script>

<template>
  <div class="homeIndex-container">
    <!-- ═══ 左侧栏：筛选 ═══ -->
    <aside class="sidebar-left">
      <div class="filter-card">
        <label class="filter-label">选择平台</label>
        <Select v-model:value="platformId" :options="platformOptions" placeholder="全部平台" size="large" show-search :filter-option="filterOption" :popup-match-select-width="false" popup-class-name="filter-select-dropdown" @change="onPlatformChange" />
      </div>
      <div v-if="subPlatforms.length" class="filter-card">
        <label class="filter-label">业务类型</label>
        <Select v-model:value="subPlatformId" :options="subPlatformOptions" placeholder="请选择" size="large" show-search :filter-option="filterOption" :popup-match-select-width="false" popup-class-name="filter-select-dropdown" @change="onSubChange" />
      </div>
      <div class="filter-summary">
        <span class="filter-count">{{ total }} 项服务</span>
        <span v-if="keyword" class="filter-keyword">搜索: "{{ keyword }}"</span>
      </div>
    </aside>

    <!-- ═══ 中间区：服务市场（一体化模块） ═══ -->
    <main class="main-content">
      <section class="marketplace">
        <!-- 移动端公告：桌面隐藏，≤1024px 显示在业务区顶部 -->
        <div class="notice-mobile">
          <div v-if="currentSubNotice" class="notice-card">
            <div class="notice-header"><Icon icon="NotificationOutlined" /><strong>{{ currentSubName || '公告' }}</strong></div>
            <div class="notice-content">{{ currentSubNotice }}</div>
          </div>
          <div v-else class="notice-card notice-card--empty">
            <Icon icon="NotificationOutlined" class="notice-empty-icon" />
            <p>选择业务类型后<br />显示公告内容</p>
          </div>
        </div>

        <header class="marketplace-header">
          <div class="header-text">
            <h1 class="header-title">提升你的<span class="highlight">社交媒体</span>影响力</h1>
            <p class="header-desc">专业、安全、高效的社交媒体增长服务，覆盖 TikTok、Facebook、Instagram 等主流平台。</p>
          </div>
          <div class="header-search">
            <input v-model="keyword" placeholder="搜索业务名称或ID..." class="search-input" spellcheck="false" @input="onKeywordInput" @keyup.enter="search" />
            <button v-if="keyword" class="search-clear" type="button" title="清空搜索" @click="clearKeyword">×</button>
            <button class="search-btn" type="button" @click="search"><Icon icon="SearchOutlined" /></button>
          </div>
        </header>

        <div class="marketplace-body">
          <Spin :spinning="loading">
            <div v-if="services.length" class="service-grid">
              <article v-for="item in services" :key="item.configId" class="service-card" @click="openOrder(item)">
                <h3 class="card-title">[{{ item.configId }}] {{ item.configName || `服务 ${item.configId}` }}</h3>
                <p class="card-desc">{{ item.configNotice || item.configTips || '暂无服务说明' }}</p>
                <div class="card-footer">
                  <div class="card-meta">
                    <span class="meta-spec">
                      <b>{{ item.showPriceUnit || 1 }}</b> / ¥{{ fmtMoney(displayPriceOf(item)) }}
                      <span class="meta-range">区间 {{ item.minQuantity || 1 }}-{{ item.maxQuantity || '不限' }}</span>
                    </span>
                    <span class="meta-price">¥{{ fmtMoney(item.unitPrice) }}<em>/个</em></span>
                  </div>
                  <button class="card-action" type="button" @click.stop="openOrder(item)">购买</button>
                </div>
              </article>
            </div>
            <Empty v-else-if="!loading" description="暂无匹配的业务" />
          </Spin>
        </div>

        <div v-if="total > pageSize" class="marketplace-pagination">
          <Pagination v-model:current="page" v-model:page-size="pageSize" :total="total" show-size-changer size="small" @change="loadServices" />
        </div>
      </section>
    </main>

    <!-- ═══ 右侧栏：公告 ═══ -->
    <aside class="sidebar-right">
      <div v-if="currentSubNotice" class="notice-card">
        <div class="notice-header"><Icon icon="NotificationOutlined" /><strong>{{ currentSubName || '公告' }}</strong></div>
        <div class="notice-content">{{ currentSubNotice }}</div>
      </div>
      <div v-else class="notice-card notice-card--empty">
        <Icon icon="NotificationOutlined" class="notice-empty-icon" />
        <p>选择业务类型后<br />显示公告内容</p>
      </div>
    </aside>

    <!-- ═══ 下单弹窗 ═══ -->
    <Modal
      v-model:open="orderVisible"
      title="提交订单"
      :confirm-loading="ordering"
      :ok-button-props="{ disabled: !canSubmit }"
      ok-text="确认下单"
      cancel-text="取消"
      @ok="submitOrder"
    >
      <div v-if="selectedService" class="order-form">
        <div class="order-summary"><strong>[{{ selectedService.configId }}] {{ selectedService.configName }}</strong><span>单价 ¥{{ Number(selectedService.price).toFixed(2) }} / {{ selectedService.priceUnit }} 个</span></div>
        <div v-if="selectedService.configNotice || selectedService.configTips" class="order-tips"><label>业务说明</label><p>{{ selectedService.configNotice || selectedService.configTips }}</p></div>
        <template v-if="!isAccountTemplate(selectedService)">
          <label>目标链接（每行一个，可批量提交）</label>
          <Input.TextArea v-model:value="orderForm.linksText" :rows="5" placeholder="请输入目标链接，每行一个；一行会生成一条订单" />
        </template>
        <template v-if="isCommentTemplate(selectedService)">
          <label class="label-row">
            <span>评论内容（每行一条，将应用到每个链接）</span>
            <span class="label-count" :class="{ 'is-invalid': commentCountHint.invalid }">{{ commentCountHint.text }}</span>
          </label>
          <Input.TextArea v-model:value="orderForm.commentsText" :rows="6" placeholder="每行输入一条评论，评论条数即该订单的数量（单条不超过 500 字）" />
        </template>
        <template v-else>
          <label>{{ isAccountTemplate(selectedService) ? '购买账户数量（同次多个账户算一个订单）' : '下单数量' }}</label>
          <InputNumber v-model:value="orderForm.quantity" :min="selectedService.minQuantity || 1" :max="selectedService.maxQuantity || undefined" :step="selectedService.orderUnit || 1" style="width: 100%" />
        </template>
        <div class="estimated">
          <span class="estimated-breakdown">
            {{ orderCount }} 条订单 × {{ perOrderQuantity }}{{ isCommentTemplate(selectedService) ? ' 条评论' : ' 个' }}
          </span>
          <span class="estimated-amount">预计 ¥{{ estimatedAmount.toFixed(2) }}</span>
        </div>
      </div>
    </Modal>
  </div>
</template>

<style lang="scss" scoped>
$bg-card: #ffffff;
$border-base: #e5e9ef;
$border-hover: #aeb9ee;
$text-primary: #1e293b;
$text-secondary: #64748b;
$text-muted: #94a3b8;
$color-primary: #586ee1;
$color-accent: #f59e0b;
$r-sm: 8px;
$r-md: 12px;

.homeIndex-container {
  display: grid;
  grid-template-columns: 220px 1fr 260px;
  gap: 1rem;
  padding: 0.75rem 0 1.25rem;
  min-height: 0;
}

/* ─── 左侧筛选栏 ─── */
.sidebar-left {
  display: flex; flex-direction: column; gap: 0.65rem;
  height: fit-content; position: sticky; top: 1rem;
}
.filter-card {
  background: $bg-card; border: 1px solid $border-base; border-radius: $r-md;
  padding: 0.85rem; box-shadow: 0 1px 2px rgba(0,0,0,0.04);
}
.filter-label {
  display: block; font-size: 0.75rem; font-weight: 600; color: $text-muted;
  text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.4rem;
}
.filter-summary {
  background: $bg-card; border: 1px solid $border-base; border-radius: $r-md;
  padding: 0.7rem 0.85rem; box-shadow: 0 1px 2px rgba(0,0,0,0.04);
  display: flex; flex-direction: column; gap: 0.2rem;
}
.filter-count { font-size: 0.82rem; color: $text-primary; font-weight: 600; }
.filter-keyword { font-size: 0.75rem; color: $color-primary; }

/* ─── antd Select 深度样式修复（show-search 模式） ─── */
.filter-card {
  :deep(.ant-select) { width: 100%; }
  :deep(.ant-select-selector) {
    min-height: 40px !important;
    border-color: $border-base !important;
    border-radius: $r-sm !important;
    transition: border-color 0.2s, box-shadow 0.2s;
  }
  :deep(.ant-select-focused .ant-select-selector) {
    border-color: $color-primary !important;
    box-shadow: 0 0 0 3px rgba(88,110,225,0.08) !important;
  }
  // show-search 内部输入框：确保文字可见
  :deep(.ant-select .ant-select-selection-search-input) {
    color: $text-primary !important;
    &::placeholder { color: $text-muted !important; }
  }
  // 下拉选项文字颜色
  :deep(.ant-select-item) {
    color: $text-primary;
    font-size: 0.88rem;
  }
}

/* ─── 主内容：服务市场模块 ─── */
.main-content { min-width: 0; }

.marketplace {
  background: $bg-card; border: 1px solid $border-base; border-radius: $r-md;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06); overflow: hidden;
  display: flex; flex-direction: column;
}

.marketplace-header {
  display: flex; align-items: center; justify-content: space-between;
  gap: 1.25rem; padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  background: linear-gradient(135deg, #fafbff 0%, #f8faff 100%);
}
.header-text { min-width: 0; }
.header-title {
  margin: 0; font-size: 1.35rem; font-weight: 700; color: $text-primary; line-height: 1.3;
  .highlight {
    color: $color-accent; position: relative;
    &::after { content:''; position:absolute; left:0; bottom:2px; width:100%; height:6px;
      background: rgba(245,158,11,0.18); border-radius:3px; z-index:-1; }
  }
}
.header-desc { margin: 0.3rem 0 0; color: $text-secondary; font-size: 0.82rem; line-height: 1.5; max-width: 420px; }

.header-search {
  display: flex; align-items: center; flex-shrink: 0;
  width: 280px; height: 40px; background: $bg-card;
  border: 1px solid $border-base; border-radius: $r-sm;
  padding: 0 0.35rem 0 0.75rem;
  transition: border-color 0.2s, box-shadow 0.2s;
  &:focus-within { border-color: $color-primary; box-shadow: 0 0 0 3px rgba(88,110,225,0.08); }
}
.search-input {
  flex: 1; min-width: 0; background: transparent; border: 0; color: $text-primary; font-size: 0.88rem;
  &::placeholder{color:$text-muted;} &:focus{outline:none;}
}
.search-clear {
  flex-shrink: 0; width: 22px; height: 22px;
  display: flex; align-items: center; justify-content: center;
  background: #f1f5f9; border: none; border-radius: 50%;
  color: $text-muted; font-size: 0.85rem; cursor: pointer;
  transition: background 0.15s, color 0.15s;
  line-height: 1; padding: 0;
  &:hover{background:#e2e8f0;color:$text-primary;}
}
.search-btn {
  flex-shrink: 0; width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg,$color-primary 0%,#4f46e5 100%);
  border: none; border-radius: 6px; color: #fff; cursor: pointer; font-size: 0.88rem;
  transition: opacity 0.15s;
  &:hover{opacity:0.88;} &:active{transform:scale(0.95);}
}

.marketplace-body { padding: 1.25rem; min-height: 300px; }
.service-grid {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 0.85rem;
}

.service-card {
  cursor: pointer; background: $bg-card; border: 1px solid $border-base; border-radius: $r-md;
  padding: 1.1rem; display: flex; flex-direction: column; gap: 0.55rem;
  transition: border-color 0.2s, box-shadow 0.2s, transform 0.15s;
  &:hover { border-color: $border-hover; box-shadow: 0 4px 16px rgba(88,110,225,0.12); transform: translateY(-3px); }
}
.card-title { margin: 0; font-size: 0.98rem; font-weight: 600; color: $text-primary; line-height: 1.35; }
.card-desc {
  margin: 0; color: $text-secondary; font-size: 0.83rem; line-height: 1.55; flex: 1;
  min-height: 2.7em; max-height: 4.2em; overflow: hidden;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
}
.card-footer {
  display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
  padding-top: 0.6rem; border-top: 1px solid #f1f5f9; margin-top: auto;
}
.card-meta { display: flex; flex-direction: column; gap: 0.15rem; min-width: 0; }
.meta-spec {
  font-size: 0.74rem; color: $text-muted; line-height: 1.4;
  b { color: $text-secondary; font-weight: 600; }
}
.meta-range { margin-left: 0.35rem; color: $text-muted; opacity: 0.85; }
.meta-price {
  font-size: 1.1rem; font-weight: 700; color: $color-accent; line-height: 1.2;
  em { font-style: normal; font-size: 0.72rem; font-weight: 500; color: $text-muted; margin-left: 0.1rem; }
}
.card-action {
  height: 32px; padding: 0 0.85rem; border-radius: 6px;
  background: linear-gradient(135deg,$color-primary 0%,#4f46e5 100%);
  color: #fff; font-weight: 600; font-size: 0.8rem; border: 0;
  cursor: pointer; white-space: nowrap; flex-shrink: 0;
  transition: opacity 0.15s, transform 0.1s, box-shadow 0.2s;
  &:hover{opacity:0.88;box-shadow:0 2px 8px rgba(88,110,225,0.25);}
  &:active{transform:scale(0.95);}
}

.marketplace-pagination { padding: 0.85rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: center; }

/* ─── 右侧公告栏 ─── */
.sidebar-right { position: sticky; top: 1rem; height: fit-content; }
.notice-card {
  background: $bg-card; border: 1px solid $border-base; border-radius: $r-md;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04); overflow: hidden;
}
.notice-header {
  display: flex; align-items: center; gap: 0.4rem;
  padding: 0.85rem 1rem; border-bottom: 1px solid #f1f5f9; background: #fafbff;
  font-size: 0.86rem; color: $text-primary;
  :deep(.anticon){color:$color-accent;font-size:0.95rem;}
}
.notice-content {
  padding: 1rem; color: $text-secondary; font-size: 0.84rem; line-height: 1.75;
  white-space: pre-wrap; word-break: break-word;
  max-height: calc(100vh - 260px); overflow-y: auto;
  &::-webkit-scrollbar{width:4px;} &::-webkit-scrollbar-thumb{background:#cbd5e1;border-radius:4px;}
}
/* 移动端公告内容限高 5 行，超出可上下滑动 */
.notice-mobile .notice-content {
  max-height: 9.5rem;
  overflow-y: auto;
}
.notice-card--empty {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 0.5rem; min-height: 180px; padding: 1.5rem 1rem; color: $text-muted; text-align: center;
}
.notice-empty-icon { font-size: 1.6rem; color: #cbd5e1; }
.notice-card--empty p { margin: 0; font-size: 0.82rem; line-height: 1.5; }

/* ─── 下单弹窗 ─── */
.order-form { display: grid; gap: 0.65rem; }
.order-summary { display: flex; justify-content: space-between; gap: 1rem; padding-bottom: 0.5rem; border-bottom: 1px solid #f1f5f9; }
.order-tips { display: grid; gap: 0.3rem; padding: 0.65rem; border-radius: $r-sm; background: rgba(245,158,11,0.06); border: 1px solid rgba(245,158,11,0.14); }
.order-tips p { margin: 0; color: $text-secondary; line-height: 1.55; white-space: pre-wrap; word-break: break-word; font-size: 0.85rem; }
.estimated {
  display: flex; align-items: baseline; justify-content: space-between; gap: 0.75rem;
  padding: 0.6rem 0.75rem; border-radius: $r-sm;
  background: rgba(245,158,11,0.06); border: 1px solid rgba(245,158,11,0.14);
}
.label-row { display: flex; align-items: baseline; justify-content: space-between; gap: 0.5rem; }
.label-count {
  font-size: 0.76rem; font-weight: 600; color: $color-primary; white-space: nowrap;
  transition: color 0.15s;
  &.is-invalid { color: #dc2626; }
}
.estimated-breakdown { color: $text-secondary; font-size: 0.82rem; }
.estimated-amount { color: $color-accent; font-weight: 700; font-size: 0.98rem; white-space: nowrap; }

/* 移动端公告：默认隐藏 */
.notice-mobile { display: none; }

/* ─── 响应式 ─── */
@media (max-width: 1024px) {
  .homeIndex-container { grid-template-columns: 190px 1fr; }
  .sidebar-right { display: none; }
  .notice-mobile { display: block; padding: 0.85rem 1.5rem 0; }
  .header-search { width: 220px; }
  .service-grid { grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); }
}
@media (max-width: 768px) {
  .homeIndex-container { grid-template-columns: 1fr; padding: 0.5rem 0 1rem; gap: 0.7rem; }
  .sidebar-left { position: static; flex-direction: row; overflow-x: auto; gap: 0.5rem; }
  .filter-card { min-width: 150px; flex-shrink: 0; }
  .filter-summary { display: none; }
  .marketplace-header { flex-direction: column; align-items: stretch; gap: 0.75rem; padding: 1rem; }
  .notice-mobile { padding: 0.75rem 1rem 0; }
  .header-search { width: 100%; }
  .header-desc { max-width: none; }
  .marketplace-body { padding: 1rem; }
  .service-grid { grid-template-columns: 1fr; }
  .marketplace-pagination { padding: 0.7rem 1rem; }
}
</style>

<!-- 非 scoped：antd Select 下拉弹出层通过 portal 挂到 body，:deep() 够不到 -->
<style lang="scss">
.filter-select-dropdown {
  min-width: 200px !important;
  max-width: 320px !important;
  .ant-select-item { font-size: 0.88rem; color: #1e293b; padding: 8px 12px; }
  .ant-select-item-option-active { background: #f1f5f9; }
}
</style>
