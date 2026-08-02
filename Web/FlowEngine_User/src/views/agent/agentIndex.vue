<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { Button, Card, Col, Empty, Input, InputNumber, Modal, Pagination, Row, Select, Spin, Tag, message } from 'ant-design-vue'
import { agentApi } from '../../api'
import { useAuth } from '../../stores/auth'

const auth = useAuth()
const user = computed(() => auth.user.value || {})
const children = ref([])
const childTotal = ref(0)
const childLoading = ref(false)
const childQuery = reactive({ keyword: '', enabled: undefined, page: 1, pageSize: 6 })
const markups = ref([])
const markupTotal = ref(0)
const markupLoading = ref(false)
const markupQuery = reactive({ keyword: '', page: 1, pageSize: 6 })
const childModal = ref(false)
const transferModal = ref(false)
const statusModal = ref(false)
const markupModal = ref(false)
const withdrawModal = ref(false)
const selectedChild = ref(null)
const selectedMarkup = ref(null)
const saving = ref(false)
const childForm = reactive({ email: '', username: '', password: '' })
const amountForm = reactive({ amount: null })
const statusEnabled = ref(true)
const markupAmount = ref(0)

const activeCount = computed(() => children.value.filter((x) => x.enabled).length)

const loadChildren = async () => {
  childLoading.value = true
  try {
    const result = await agentApi.children(childQuery)
    children.value = result.data || []
    childTotal.value = result.count
  } catch (error) { message.error(error.message) }
  finally { childLoading.value = false }
}

const loadMarkups = async () => {
  markupLoading.value = true
  try {
    const result = await agentApi.markups(markupQuery)
    markups.value = result.data || []
    markupTotal.value = result.count
  } catch (error) { message.error(error.message) }
  finally { markupLoading.value = false }
}

const saveChild = async () => {
  if (!childForm.email || !childForm.username || !childForm.password) return message.warning('请完整填写用户信息')
  saving.value = true
  try {
    await agentApi.createChild(childForm)
    message.success('下级用户创建成功')
    childModal.value = false
    Object.assign(childForm, { email: '', username: '', password: '' })
    await loadChildren()
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const openTransfer = (child) => {
  selectedChild.value = child
  amountForm.amount = null
  transferModal.value = true
}

const transfer = async () => {
  if (!amountForm.amount || amountForm.amount <= 0) return message.warning('请输入有效金额')
  saving.value = true
  try {
    await agentApi.transfer({ childUserId: selectedChild.value.userId, amount: amountForm.amount })
    message.success('余额赠送成功')
    transferModal.value = false
    await Promise.all([loadChildren(), auth.loadUser()])
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const openStatus = (child) => {
  selectedChild.value = child
  statusEnabled.value = child.enabled
  statusModal.value = true
}

const saveStatus = async () => {
  saving.value = true
  try {
    await agentApi.changeStatus(selectedChild.value.userId, { enabled: statusEnabled.value })
    message.success('状态修改成功')
    statusModal.value = false
    await loadChildren()
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const resetPassword = (child) => Modal.confirm({
  title: `重置 ${child.username} 的密码？`,
  content: '旧登录会话将立即失效，新密码只显示一次。',
  okText: '确认重置', cancelText: '取消', centered: true,
  async onOk() {
    try {
      const result = await agentApi.resetPassword(child.userId)
      Modal.info({ title: '新密码', content: result.data.password, okText: '我已记录', centered: true })
    } catch (error) { message.error(error.message) }
  },
})

const openMarkup = (item) => {
  selectedMarkup.value = item
  markupAmount.value = item.markupAddPrice
  markupModal.value = true
}

const saveMarkup = async () => {
  saving.value = true
  try {
    await agentApi.setMarkup({ configId: selectedMarkup.value.configId, markupAddPrice: markupAmount.value })
    message.success('加价配置已保存')
    markupModal.value = false
    // 加价列表接口后端尚未提供，保存后不刷新列表
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const withdraw = async () => {
  if (!amountForm.amount || amountForm.amount <= 0) return message.warning('请输入有效金额')
  saving.value = true
  try {
    await agentApi.withdraw({ amount: amountForm.amount })
    message.success('代理余额已提取到用户余额')
    withdrawModal.value = false
    await auth.loadUser()
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

watch(() => [childQuery.enabled, childQuery.page, childQuery.pageSize], loadChildren)
onMounted(() => Promise.all([loadChildren(), auth.loadUser()]))
</script>

<template>
  <div class="agent-page">
    <header class="page-header">
      <div><h2>代理管理</h2><p>管理直属用户、余额转赠与业务加价</p></div>
      <div class="header-actions"><Button disabled title="功能暂未开放：后端未提供该接口">提取代理余额（暂未开放）</Button><Button type="primary" @click="childModal = true">新增用户</Button></div>
    </header>

    <Row :gutter="[16, 16]" class="stats">
      <Col :xs="24" :md="8"><Card><span>用户余额</span><strong>¥{{ Number(user.userAmount || 0).toFixed(2) }}</strong></Card></Col>
      <Col :xs="24" :md="8"><Card><span>代理余额</span><strong>¥{{ Number(user.agentAmount || 0).toFixed(2) }}</strong></Card></Col>
      <Col :xs="24" :md="8"><Card><span>本页启用用户</span><strong>{{ activeCount }} / {{ children.length }}</strong></Card></Col>
    </Row>

    <Card class="section">
      <div class="toolbar"><h3>直属用户</h3><div class="filters"><Select v-model:value="childQuery.enabled" allow-clear placeholder="全部状态" style="width: 130px"><Select.Option :value="true">已启用</Select.Option><Select.Option :value="false">已停用</Select.Option></Select><Input.Search v-model:value="childQuery.keyword" placeholder="用户名或邮箱" @search="childQuery.page = 1; loadChildren()" /></div></div>
      <Spin :spinning="childLoading"><div v-if="children.length" class="grid">
        <article v-for="child in children" :key="child.userId" class="item">
          <div class="item-title"><strong>{{ child.username }}</strong><Tag :color="child.enabled ? 'green' : 'red'">{{ child.enabled ? '已启用' : '已停用' }}</Tag></div>
          <dl><dt>用户 ID</dt><dd>{{ child.userId }}</dd><dt>邮箱</dt><dd>{{ child.email }}</dd><dt>余额</dt><dd>¥{{ Number(child.userAmount).toFixed(2) }}</dd><dt>创建时间</dt><dd>{{ child.createTime ? new Date(child.createTime).toLocaleString('zh-CN') : '-' }}</dd></dl>
          <div class="actions"><Button size="small" @click="openStatus(child)">状态</Button><Button size="small" type="primary" @click="openTransfer(child)">赠送余额</Button><Button size="small" danger @click="resetPassword(child)">重置密码</Button></div>
        </article>
      </div><Empty v-else-if="!childLoading" description="暂无直属用户" /></Spin>
      <Pagination v-model:current="childQuery.page" v-model:page-size="childQuery.pageSize" :total="childTotal" show-size-changer />
    </Card>

    <Card class="section">
      <div class="toolbar"><h3>业务加价</h3><Input.Search v-model:value="markupQuery.keyword" placeholder="搜索业务名称" @search="markupQuery.page = 1; loadMarkups()" /></div>
      <Spin :spinning="markupLoading"><div v-if="markups.length" class="grid">
        <article v-for="item in markups" :key="item.configId" class="item">
          <div class="item-title"><strong>{{ item.configName || `服务 ${item.configId}` }}</strong><span>#{{ item.configId }}</span></div>
          <p class="tips">{{ item.configTips || '暂无说明' }}</p>
          <dl><dt>官方价格</dt><dd>¥{{ Number(item.configPrice).toFixed(2) }}</dd><dt>基础价格</dt><dd>¥{{ Number(item.basePrice).toFixed(2) }}</dd><dt>整包加价</dt><dd>¥{{ Number(item.markupAddPrice).toFixed(2) }}</dd><dt>下级展示价</dt><dd>¥{{ Number(item.childDisplayPrice).toFixed(2) }}</dd><dt>价格单位</dt><dd>{{ item.priceUnit }}</dd><dt>单个加价</dt><dd>¥{{ Number(item.agentSingleAddPrice).toFixed(2) }}</dd></dl>
          <div class="actions"><Button type="primary" size="small" @click="openMarkup(item)">设置加价</Button></div>
        </article>
      </div><Empty v-else-if="!markupLoading" description="暂无业务配置" /></Spin>
      <Pagination v-model:current="markupQuery.page" v-model:page-size="markupQuery.pageSize" :total="markupTotal" show-size-changer />
    </Card>

    <Modal v-model:open="childModal" title="新增直属用户" :confirm-loading="saving" @ok="saveChild"><div class="form"><label>邮箱</label><Input v-model:value="childForm.email"/><label>用户名</label><Input v-model:value="childForm.username"/><label>密码</label><Input.Password v-model:value="childForm.password"/></div></Modal>
    <Modal v-model:open="transferModal" :title="`向 ${selectedChild?.username || ''} 赠送余额`" :confirm-loading="saving" @ok="transfer"><InputNumber v-model:value="amountForm.amount" :min="0.01" :precision="2" style="width:100%"/></Modal>
    <Modal v-model:open="statusModal" :title="`修改 ${selectedChild?.username || ''} 状态`" :confirm-loading="saving" @ok="saveStatus"><Select v-model:value="statusEnabled" style="width:100%"><Select.Option :value="true">已启用</Select.Option><Select.Option :value="false">已停用</Select.Option></Select></Modal>
    <Modal v-model:open="markupModal" :title="`设置 ${selectedMarkup?.configName || ''} 加价`" :confirm-loading="saving" @ok="saveMarkup"><div class="form"><span>基础价格：¥{{ Number(selectedMarkup?.basePrice || 0).toFixed(2) }} / {{ selectedMarkup?.priceUnit || 1 }}</span><label>整包加价</label><InputNumber v-model:value="markupAmount" :min="0.01" :precision="2" style="width:100%"/></div></Modal>
    <Modal v-model:open="withdrawModal" title="提取代理余额" :confirm-loading="saving" @ok="withdraw"><div class="form"><span>可提取：¥{{ Number(user.agentAmount || 0).toFixed(2) }}</span><InputNumber v-model:value="amountForm.amount" :min="0.01" :max="Number(user.agentAmount || 0)" :precision="2" style="width:100%"/></div></Modal>
  </div>
</template>

<style scoped lang="scss">
.agent-page { padding: 1.5rem; color: #f3f4f6; display: grid; gap: 1.25rem; }
.page-header,.toolbar,.header-actions,.filters,.item-title,.actions { display:flex; align-items:center; justify-content:space-between; gap:.75rem; flex-wrap:wrap; }
h2,h3,p { margin:0; } .page-header p,.tips,dt,.stats span { color:#9ca3af; }
.tips { line-height:1.6; min-height:4.8rem; max-height:4.8rem; overflow-y:auto; padding-right:.25rem; word-break:break-word; }
.stats :deep(.ant-card),.section { background:#111827; border:1px solid rgba(255,255,255,.08); border-radius:8px; }
.stats :deep(.ant-card-body) { display:grid; gap:.35rem; } .stats strong { color:#f59e0b; font-size:1.5rem; }
.section :deep(.ant-card-body) { display:grid; gap:1rem; }
.grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(310px,1fr)); gap:1rem; }
.item { background:rgba(255,255,255,.03); border:1px solid rgba(255,255,255,.08); border-radius:8px; padding:1rem; display:grid; gap:.75rem; }
dl { display:grid; grid-template-columns:auto 1fr; gap:.45rem .8rem; margin:0; } dd { margin:0; text-align:right; word-break:break-word; }
.actions { justify-content:flex-end; } .form { display:grid; gap:.65rem; }
@media(max-width:768px){.agent-page{padding:1rem}.filters,.filters :deep(.ant-input-search){width:100%}.grid{grid-template-columns:1fr}.actions>*{flex:1}}
</style>
