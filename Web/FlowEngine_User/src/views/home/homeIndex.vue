<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { Empty, Input, InputNumber, Modal, Pagination, Select, Spin, message } from 'ant-design-vue'
import { homeApi, orderApi } from '../../api'
import { Icon } from '../../assets/js/iconUtils.js'

const platforms = ref([])
const services = ref([])
const loading = ref(false)
const ordering = ref(false)
const keyword = ref('')
const platformId = ref('')
const page = ref(1)
const pageSize = ref(12)
const total = ref(0)
const orderVisible = ref(false)
const selectedService = ref(null)
const orderForm = reactive({ linksText: '', quantity: null, commentsText: '' })

const isCommentTemplate = (service) => service?.jsonTemplate === 2
const isAccountTemplate = (service) => service?.jsonTemplate === 3

const platformMap = computed(() =>
  Object.fromEntries(platforms.value.map((item) => [item.platformId, item])),
)
const orderCount = computed(() => {
  if (!selectedService.value) return 0
  if (isAccountTemplate(selectedService.value)) return 1
  return orderForm.linksText.split('\n').map((x) => x.trim()).filter(Boolean).length
})
const estimatedAmount = computed(() => {
  if (!selectedService.value) return 0
  const quantity = isCommentTemplate(selectedService.value)
    ? orderForm.commentsText.split('\n').filter((x) => x.trim()).length
    : orderForm.quantity
  return (selectedService.value.price / selectedService.value.priceUnit) * (quantity || 0) * orderCount.value
})

const loadPlatforms = async () => {
  const result = await homeApi.platforms()
  platforms.value = result.data || []
}

const loadServices = async () => {
  loading.value = true
  try {
    const result = await homeApi.configs({
      platformIds: platformId.value || undefined,
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

const search = () => {
  page.value = 1
  loadServices()
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
  const links = isAccountTemplate(service)
    ? ['']
    : orderForm.linksText.split('\n').map((x) => x.trim()).filter(Boolean)
  if (!isAccountTemplate(service) && !links.length) return message.warning('请至少输入一个链接，每行一个')
  if (links.length > 100) return message.warning('一次最多提交 100 个链接')
  const comments = isCommentTemplate(service)
    ? orderForm.commentsText.split('\n').map((x) => x.trim()).filter(Boolean)
    : []
  const quantity = isCommentTemplate(service) ? comments.length : Number(orderForm.quantity)
  if (quantity <= 0) return message.warning(isCommentTemplate(service) ? '请输入评论内容' : '请输入下单数量')
  if (service.minQuantity && quantity < service.minQuantity)
    return message.warning(`下单数量不能少于 ${service.minQuantity}`)
  if (service.maxQuantity && quantity > service.maxQuantity)
    return message.warning(`下单数量不能超过 ${service.maxQuantity}`)
  if (service.orderUnit && quantity % service.orderUnit !== 0)
    return message.warning(`下单数量必须是 ${service.orderUnit} 的整数倍`)
  ordering.value = true
  try {
    const result = await orderApi.createBatch({
      configId: service.configId,
      orders: links.map((link) => ({ link, quantity, comments })),
    })
    message.success(`已成功提交 ${result.data.items.length} 条订单`)
    orderVisible.value = false
  } catch (error) {
    message.error(error.message)
  } finally {
    ordering.value = false
  }
}

onMounted(async () => {
  try {
    await Promise.all([loadPlatforms(), loadServices()])
  } catch (error) {
    message.error(error.message)
  }
})
</script>

<template>
  <div class="homeIndex-container">
    <div class="header-container">
      <h1 class="tips1">提升你的 <span class="tip">社交媒体</span> 影响力</h1>
      <p class="tips2">
        专业、安全、高效的社交媒体增长服务，覆盖 TikTok、Facebook、Instagram 等主流平台。
      </p>
    </div>

    <div class="search-container">
      <Select
        v-model:value="platformId"
        class="select-search"
        allow-clear
        placeholder="全部平台"
        :options="[
          { value: '', label: '全部平台' },
          ...platforms.map((item) => ({ value: item.platformId, label: item.platformName || `平台 ${item.platformId}` })),
        ]"
        @change="search"
      />
      <span class="search-icon" aria-hidden="true"><Icon icon="SearchOutlined" /></span>
      <input v-model="keyword" placeholder="请输入业务名称，关键词..." class="input-search" spellcheck="false" @keyup.enter="search" />
      <button class="button-search" type="button" @click="search">查询</button>
    </div>

    <Spin :spinning="loading">
    <div v-if="services.length" class="config-container">
      <div v-for="item in services" :key="item.configId" class="config-item">
        <div class="header">
          <img
            :src="platformMap[item.platformId]?.platformImg || '/favicon.ico'"
            class="service-image"
            alt=""
          />
          <div class="product-name">{{ platformMap[item.platformId]?.platformName || `平台 ${item.platformId}` }}</div>
        </div>
        <strong>{{ item.configName || `服务 ${item.configId}` }}</strong>
        <div class="config-name">{{ item.configTips || '暂无服务说明' }}</div>
        <div class="operate">
          <div>
            <div class="config-num">{{ item.priceUnit }} 数量 / 包 · 范围 {{ item.minQuantity || 1 }}-{{ item.maxQuantity || '不限' }}</div>
            <div class="config-amount">¥ {{ Number(item.price).toFixed(2) }}</div>
          </div>
          <button class="button-search" type="button" @click="openOrder(item)">购买</button>
        </div>
      </div>
    </div>
    <Empty v-else-if="!loading" description="暂无匹配的业务" />
    </Spin>

    <Pagination v-if="total > pageSize" v-model:current="page" v-model:page-size="pageSize" :total="total" show-size-changer @change="loadServices" />

    <Modal v-model:open="orderVisible" title="提交订单" :confirm-loading="ordering" ok-text="确认下单" cancel-text="取消" @ok="submitOrder">
      <div v-if="selectedService" class="order-form">
        <div class="order-summary">
          <strong>{{ selectedService.configName }}</strong>
          <span>单价 ¥{{ Number(selectedService.price).toFixed(2) }} / {{ selectedService.priceUnit }} 个</span>
        </div>
        <div v-if="selectedService.configTips" class="order-tips">
          <label>业务说明</label>
          <p>{{ selectedService.configTips }}</p>
        </div>
        <template v-if="!isAccountTemplate(selectedService)">
          <label>目标链接（每行一个，可批量提交）</label>
          <Input.TextArea v-model:value="orderForm.linksText" :rows="5" placeholder="请输入目标链接，每行一个；一行会生成一条订单" />
        </template>
        <template v-if="isCommentTemplate(selectedService)">
          <label>评论内容（每行一条，将应用到每个链接）</label>
          <Input.TextArea v-model:value="orderForm.commentsText" :rows="6" placeholder="每行输入一条评论，评论行数即每条订单的数量" />
        </template>
        <template v-else>
          <label>{{ isAccountTemplate(selectedService) ? '购买账户数量' : '下单数量' }}</label>
          <InputNumber v-model:value="orderForm.quantity" :min="selectedService.minQuantity || 1" :max="selectedService.maxQuantity || undefined" :step="selectedService.orderUnit || 1" style="width: 100%" />
        </template>
        <div class="estimated">{{ orderCount }} 条订单，预计金额：¥{{ estimatedAmount.toFixed(2) }}</div>
      </div>
    </Modal>
  </div>
</template>

<style lang="scss" scoped>
.homeIndex-container {
  padding: 1.25rem 0 1.5rem;

  .header-container {
    text-align: center;
    margin: 0 0 1.5rem;
    padding: 1.25rem 0;

    .tips1 {
      font-size: 2.4rem;
      font-weight: 700;
      color: #f9fafb;
      margin-bottom: 0.6rem;

      .tip {
        color: #f59e0b;
      }
    }

    .tips2 {
      color: #9ca3af;
      font-size: 1rem;
      max-width: 760px;
      margin: 0 auto;
      line-height: 1.6;
    }
  }

  .search-container {
    width: min(760px, 100%);
    margin: 0 auto 1.5rem;
    background: #111827;
    border-radius: 16px;
    border: 1px solid rgba(255, 255, 255, 0.16);
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.24);
  }

  .search-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex: 0 0 20px;
    color: #778694;
    font-size: 1rem;
  }

  .select-search {
    width: 142px;
    min-width: 142px;
    height: 40px;
    color: #26313d;

    :deep(.ant-select-selector) {
      height: 40px !important;
      display: flex;
      align-items: center;
      padding: 0 11px !important;
      background: transparent !important;
      border: 0 !important;
      color: #26313d !important;
      box-shadow: none !important;
    }

    :deep(.ant-select-selection-placeholder),
    :deep(.ant-select-arrow) {
      color: #65727f !important;
    }
  }

  .input-search {
    flex: 1;
    min-width: 0;
    background: transparent;
    border: 0;
    color: #26313d;
    font-size: 1rem;
  }

  .input-search:focus {
    outline: none;
  }

  .button-search {
    height: 40px;
    padding: 0 1rem;
    border-radius: 10px;
    background: linear-gradient(120deg, #f59e0b 0%, #d97706 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    color: #111827;
    font-weight: 600;
    white-space: nowrap;
    border: 0;
  }

  .config-container {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1rem;
    padding: 0.5rem 0 1.25rem;
  }

  .config-item {
    cursor: pointer;
    display: grid;
    gap: 0.8rem;
    border-radius: 16px;
    background: #111827;
    border: 1px solid rgba(255, 255, 255, 0.08);
    padding: 1rem;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.24);
  }

  .config-item:hover {
    border-color: #f59e0b;
    transform: translateY(-2px);
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .service-image {
    width: 48px;
    height: 48px;
    border-radius: 999px;
  }

  .product-name {
    padding: 0.35rem 0.8rem;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.06);
    color: #f9fafb;
  }

  .config-name {
    color: #cbd5e1;
    line-height: 1.6;
    min-height: 4.8rem;
    max-height: 4.8rem;
    overflow-y: auto;
    padding-right: 0.25rem;
    word-break: break-word;
  }

  .operate {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 0.5rem;
  }

  .config-num {
    color: #9ca3af;
    margin-bottom: 0.2rem;
  }

  .config-amount {
    color: #fbbf24;
    font-size: 1.1rem;
    font-weight: 700;
  }

  .loading-more {
    width: 140px;
    margin: 0 auto;
    height: 44px;
    border-radius: 12px;
    background: #111827;
    border: 1px solid rgba(255, 255, 255, 0.16);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    color: #cbd5e1;
  }

  .order-form {
    display: grid;
    gap: 0.75rem;
  }

  .order-summary {
    display: flex;
    justify-content: space-between;
    gap: 1rem;
  }

  .order-tips {
    display: grid;
    gap: 0.4rem;
    padding: 0.75rem;
    border-radius: 8px;
    background: rgba(245, 158, 11, 0.08);
    border: 1px solid rgba(245, 158, 11, 0.18);
  }

  .order-tips p {
    margin: 0;
    color: #cbd5e1;
    line-height: 1.6;
    white-space: pre-wrap;
    word-break: break-word;
  }

  .estimated {
    color: #fbbf24;
    font-weight: 700;
    text-align: right;
  }
}

@media (max-width: 768px) {
  .homeIndex-container {
    padding: 1rem 0 1.25rem;

    .header-container {
      margin-bottom: 1rem;

      .tips1 {
        font-size: 1.8rem;
      }

      .tips2 {
        font-size: 0.95rem;
      }
    }

    .search-container {
      flex-wrap: wrap;
      padding: 0.7rem;

      width: min(380px, 100%);
    }

    .select-search {
      width: 100%;
      min-width: 0;
    }

    .button-search {
      width: 100%;
    }

    .config-container {
      grid-template-columns: 1fr;
    }

    .config-item {
      padding: 0.9rem;
    }
  }
}
</style>
