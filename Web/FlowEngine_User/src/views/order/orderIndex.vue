<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import { Button, Card, Empty, Input, Pagination, Select, Spin, Tag, message } from 'ant-design-vue'
import { orderApi } from '../../api'
import ExecutionModal from '../../components/order/ExecutionModal.vue'

const orders = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ keyword: '', state: undefined, page: 1, pageSize: 10 })
const selectedOrder = ref(null)
const executionModalVisible = ref(false)
const stateMeta = {
  1: { text: '执行中', color: 'orange' },
  2: { text: '已完成', color: 'green' },
  4: { text: '排队中', color: 'blue' },
}

const loadOrders = async () => {
  loading.value = true
  try {
    const result = await orderApi.list(query)
    orders.value = result.data || []
    total.value = result.count
  } catch (error) { message.error(error.message) }
  finally { loading.value = false }
}

const search = () => { query.page = 1; loadOrders() }
const openExecution = (order) => {
  selectedOrder.value = { ...order, quantity: order.beginQuantity, successCount: order.successQuantity }
  executionModalVisible.value = true
}
watch(() => [query.state, query.page, query.pageSize], loadOrders)
onMounted(loadOrders)
</script>

<template>
  <div class="order-page">
    <header><h2>订单列表</h2><p>查看订单状态、执行数量与渠道推送结果</p></header>
    <Card class="section">
      <div class="toolbar"><h3>我的订单</h3><div class="filters"><Select v-model:value="query.state" allow-clear placeholder="全部状态" style="width:140px"><Select.Option :value="4">排队中</Select.Option><Select.Option :value="1">执行中</Select.Option><Select.Option :value="2">已完成</Select.Option></Select><Input.Search v-model:value="query.keyword" placeholder="订单号或链接" @search="search" /></div></div>
      <Spin :spinning="loading"><div v-if="orders.length" class="grid">
        <article v-for="order in orders" :key="order.orderId" class="item">
          <div class="title"><strong>{{ order.orderNo }}</strong><Tag :color="stateMeta[order.orderState]?.color || 'default'">{{ stateMeta[order.orderState]?.text || `状态 ${order.orderState}` }}</Tag></div>
          <dl><dt>订单 ID</dt><dd>{{ order.orderId }}</dd><dt>配置 ID</dt><dd>{{ order.configId }}</dd><dt>订单数量</dt><dd>{{ order.quantity }}</dd><dt>成功数量</dt><dd>{{ order.successQuantity }}</dd><dt>订单金额</dt><dd>¥{{ Number(order.orderAmount).toFixed(2) }}</dd><dt>推送状态</dt><dd>{{ order.pushState === 0 ? '未推送' : order.pushState === 1 ? '已推送' : '推送异常' }}</dd><dt>渠道单号</dt><dd>{{ order.serialNo || '-' }}</dd><dt>创建时间</dt><dd>{{ order.createTime ? new Date(order.createTime).toLocaleString('zh-CN') : '-' }}</dd></dl>
          <a :href="order.orderLink" target="_blank" rel="noopener noreferrer">{{ order.orderLink }}</a>
          <div class="actions"><Button size="small" type="primary" @click="openExecution(order)">执行情况</Button></div>
        </article>
      </div><Empty v-else-if="!loading" description="暂无订单" /></Spin>
      <Pagination v-model:current="query.page" v-model:page-size="query.pageSize" :total="total" show-size-changer />
    </Card>
    <ExecutionModal v-model:open="executionModalVisible" :initial-values="selectedOrder" />
  </div>
</template>

<style scoped lang="scss">
.order-page{padding:1.5rem;color:#f3f4f6;display:grid;gap:1.25rem}h2,h3,p{margin:0}header p,dt{color:#9ca3af}.section{background:#111827;border:1px solid rgba(255,255,255,.08);border-radius:8px}.section :deep(.ant-card-body){display:grid;gap:1rem}.toolbar,.filters,.title,.actions{display:flex;align-items:center;justify-content:space-between;gap:.75rem;flex-wrap:wrap}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(390px,1fr));gap:1rem}.item{background:rgba(255,255,255,.03);border:1px solid rgba(255,255,255,.08);border-radius:8px;padding:1rem;display:grid;gap:.75rem}.title strong,a{word-break:break-all}dl{display:grid;grid-template-columns:auto 1fr;gap:.4rem 1rem;margin:0}dd{margin:0;text-align:right}.actions{justify-content:flex-end}a{color:#f59e0b}@media(max-width:768px){.order-page{padding:1rem}.filters,.filters :deep(.ant-input-search){width:100%}.grid{grid-template-columns:1fr}}
</style>
