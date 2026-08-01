<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import { Card, Empty, Input, Pagination, Select, Spin, Tag, message } from 'ant-design-vue'
import { authApi } from '../../api'

const records = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ keyword: '', status: undefined, page: 1, pageSize: 10 })
const types = {
  0: { text: '订单消费', color: 'red' },
  1: { text: '充值', color: 'green' },
  2: { text: '代理余额提取', color: 'green' },
  3: { text: '转赠支出', color: 'red' },
  4: { text: '转赠收入', color: 'green' },
  5: { text: '订单退款', color: 'green' },
  6: { text: '后台调整', color: 'orange' },
}

const loadRecords = async () => {
  loading.value = true
  try {
    const result = await authApi.consumptions(query)
    records.value = result.data || []
    total.value = result.count
  } catch (error) { message.error(error.message) }
  finally { loading.value = false }
}
const search = () => { query.page = 1; loadRecords() }
watch(() => [query.status, query.page, query.pageSize], loadRecords)
onMounted(loadRecords)
</script>

<template>
  <div class="consume-page">
    <header><h2>消费流水</h2><p>查看用户余额每一次变化前后的完整记录</p></header>
    <Card class="section">
      <div class="toolbar"><h3>余额明细</h3><div class="filters"><Select v-model:value="query.status" allow-clear placeholder="全部类型" style="width:170px"><Select.Option v-for="(meta,key) in types" :key="key" :value="Number(key)">{{ meta.text }}</Select.Option></Select><Input.Search v-model:value="query.keyword" placeholder="搜索流水号或订单号" @search="search" /></div></div>
      <Spin :spinning="loading"><div v-if="records.length" class="grid">
        <article v-for="record in records" :key="record.consumeId" class="item">
          <div class="title"><strong>#{{ record.consumeId }}</strong><Tag :color="types[record.consumeStatus]?.color || 'default'">{{ types[record.consumeStatus]?.text || `类型 ${record.consumeStatus}` }}</Tag></div>
          <div :class="['change', record.changeAmount >= 0 ? 'income' : 'expense']">{{ record.changeAmount >= 0 ? '+' : '' }}¥{{ Number(record.changeAmount).toFixed(2) }}</div>
          <dl><dt>变动前余额</dt><dd>¥{{ Number(record.agoAmount).toFixed(2) }}</dd><dt>变动后余额</dt><dd>¥{{ Number(record.afterAmount).toFixed(2) }}</dd><dt>流水号</dt><dd>{{ record.consumeNo }}</dd><dt>创建时间</dt><dd>{{ record.createTime ? new Date(record.createTime).toLocaleString('zh-CN') : '-' }}</dd></dl>
        </article>
      </div><Empty v-else-if="!loading" description="暂无余额流水" /></Spin>
      <Pagination v-model:current="query.page" v-model:page-size="query.pageSize" :total="total" show-size-changer />
    </Card>
  </div>
</template>

<style scoped lang="scss">
.consume-page{padding:1.5rem;color:#f3f4f6;display:grid;gap:1.25rem}h2,h3,p{margin:0}header p,dt{color:#9ca3af}.section{background:#111827;border:1px solid rgba(255,255,255,.08);border-radius:8px}.section :deep(.ant-card-body){display:grid;gap:1rem}.toolbar,.filters,.title{display:flex;align-items:center;justify-content:space-between;gap:.75rem;flex-wrap:wrap}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:1rem}.item{background:rgba(255,255,255,.03);border:1px solid rgba(255,255,255,.08);border-radius:8px;padding:1rem;display:grid;gap:.75rem}.change{font-size:1.45rem;font-weight:700}.income{color:#22c55e}.expense{color:#f87171}dl{display:grid;grid-template-columns:auto 1fr;gap:.4rem .8rem;margin:0}dd{margin:0;text-align:right;word-break:break-all}@media(max-width:768px){.consume-page{padding:1rem}.filters,.filters :deep(.ant-input-search){width:100%}.grid{grid-template-columns:1fr}}
</style>
