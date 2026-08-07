<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { Button, Card, Col, Empty, Image, Input, InputNumber, Modal, Pagination, Row, Select, Spin, Tag, Upload, message } from 'ant-design-vue'
import { agentApi, homeApi, resolveAssetUrl, serviceImageApi, ticketApi } from '../../api'
import { useAuth } from '../../stores/auth'
import { formatMoney } from '../../utils/format'

const auth = useAuth()
const user = computed(() => auth.user.value || {})

// 金额/价格统一格式化：最多 6 位小数并去尾零
const formatPrice6 = formatMoney

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
const dashboard = reactive({ userAmount: 0, agentAmount: 0, enabledChildrenCount: 0, totalChildrenCount: 0 })
const overallPercent = ref(0)
const overallLoading = ref(false)

// 业务加价新增模态框状态
const addMarkupModal = ref(false)
const addMarkupLoading = ref(false)
const platforms = ref([])
const subPlatforms = ref([])
const markupConfigOptions = ref([])
const addMarkupForm = reactive({ platformId: null, subPlatformId: null, configId: null, markupAddPrice: null })
const selectedMarkupConfig = ref(null)

// 代理客服微信图片上传
const wechatImageUrl = ref('')
const wechatUploadLoading = ref(false)

const loadDashboard = async () => {
  try {
    const result = await agentApi.dashboard()
    const data = result.data || {}
    dashboard.userAmount = data.userAmount || 0
    dashboard.agentAmount = data.agentAmount || 0
    dashboard.enabledChildrenCount = data.enabledChildrenCount || 0
    dashboard.totalChildrenCount = data.totalChildrenCount || 0
  } catch (error) { message.error(error.message) }
}

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
    markups.value = (result.data || []).map((m) => ({
      markupId: m.markupId,
      configId: m.configId,
      configName: m.configName,
      configTips: m.configNotice,
      configPrice: m.configPrice,
      basePrice: m.basePrice,
      markupAddPrice: m.markupAddPrice,
      priceUnit: m.showPriceUnit,
      childDisplayPrice: m.childDisplayPrice,
      createTime: m.createTime,
    }))
    markupTotal.value = result.count
  } catch (error) { message.error(error.message) }
  finally { markupLoading.value = false }
}

// ── 业务加价新增模态框 ──
const loadPlatformsForMarkup = async () => {
  try {
    const result = await homeApi.platforms()
    platforms.value = result.data || []
  } catch (error) { message.error(error.message) }
}

const loadSubsForMarkup = async () => {
  addMarkupForm.subPlatformId = null
  addMarkupForm.configId = null
  selectedMarkupConfig.value = null
  markupConfigOptions.value = []
  if (!addMarkupForm.platformId) {
    subPlatforms.value = []
    return
  }
  try {
    const result = await homeApi.subs(addMarkupForm.platformId)
    subPlatforms.value = result.data || []
  } catch (error) { message.error(error.message) }
}

const loadMarkupConfigs = async () => {
  addMarkupForm.configId = null
  selectedMarkupConfig.value = null
  if (!addMarkupForm.platformId || !addMarkupForm.subPlatformId) {
    markupConfigOptions.value = []
    return
  }
  addMarkupLoading.value = true
  try {
    const result = await agentApi.markupConfigs({
      platformId: addMarkupForm.platformId,
      subPlatformId: addMarkupForm.subPlatformId,
      page: 1,
      pageSize: 100,
    })
    markupConfigOptions.value = result.data || []
  } catch (error) { message.error(error.message) }
  finally { addMarkupLoading.value = false }
}

const onMarkupConfigChange = () => {
  selectedMarkupConfig.value = markupConfigOptions.value.find((c) => c.configId === addMarkupForm.configId) || null
}

const openAddMarkup = async () => {
  Object.assign(addMarkupForm, { platformId: null, subPlatformId: null, configId: null, markupAddPrice: null })
  selectedMarkupConfig.value = null
  subPlatforms.value = []
  markupConfigOptions.value = []
  await loadPlatformsForMarkup()
  addMarkupModal.value = true
}

const saveAddMarkup = async () => {
  if (!addMarkupForm.platformId) return message.warning('请选择平台')
  if (!addMarkupForm.subPlatformId) return message.warning('请选择业务类型')
  if (!addMarkupForm.configId) return message.warning('请选择业务配置')
  if (addMarkupForm.markupAddPrice === null || addMarkupForm.markupAddPrice === undefined || addMarkupForm.markupAddPrice < 0) {
    return message.warning('请输入有效的加价金额')
  }
  saving.value = true
  try {
    await agentApi.setMarkup({ configId: addMarkupForm.configId, markupAddPrice: addMarkupForm.markupAddPrice })
    message.success('单业务加价已保存')
    addMarkupModal.value = false
    await loadMarkups()
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const deleteMarkup = (item) => Modal.confirm({
  title: `删除 [${item.configId}] ${item.configName || `业务 ${item.configId}`} 的加价？`,
  content: '删除后下级用户对该业务将不再享受此加价，恢复为总体加价或基准价。',
  okText: '确认删除', cancelText: '取消', centered: true, okButtonProps: { danger: true },
  async onOk() {
    try {
      await agentApi.deleteMarkup(item.configId)
      message.success('加价已删除')
      await loadMarkups()
    } catch (error) { message.error(error.message) }
  },
})

const platformOptions = computed(() =>
  platforms.value.map((p) => ({ value: p.platformId, label: p.platformName || `平台 ${p.platformId}` })),
)
const subPlatformOptions = computed(() =>
  subPlatforms.value.map((s) => ({ value: s.subPlatformId, label: s.subPlatformName || `业务类型 ${s.subPlatformId}` })),
)
const markupConfigSelectOptions = computed(() =>
  markupConfigOptions.value.map((c) => ({ value: c.configId, label: `[${c.configId}] ${c.configName || `业务 ${c.configId}`}` })),
)

// 下拉框按 label 模糊匹配
const filterOption = (input, option) => {
  const label = typeof option?.label === 'string' ? option.label : String(option?.label || '')
  return label.toLowerCase().includes(input.toLowerCase())
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
  if (markupAmount.value === null || markupAmount.value === undefined || markupAmount.value < 0) {
    return message.warning('请输入有效的加价金额')
  }
  saving.value = true
  try {
    await agentApi.setMarkup({ configId: selectedMarkup.value.configId, markupAddPrice: markupAmount.value })
    message.success('加价配置已保存')
    markupModal.value = false
    await loadMarkups()
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const withdraw = async () => {
  if (!amountForm.amount || amountForm.amount <= 0) return message.warning('请输入有效金额')
  saving.value = true
  try {
    await agentApi.withdraw(amountForm.amount)
    message.success('代理余额已提取到用户余额')
    withdrawModal.value = false
    await Promise.all([loadDashboard(), auth.loadUser()])
  } catch (error) { message.error(error.message) }
  finally { saving.value = false }
}

const loadOverallPrice = async () => {
  overallLoading.value = true
  try {
    const result = await agentApi.getOverallPrice()
    overallPercent.value = result.data?.overallPercent ?? 0
  } catch (error) { message.error(error.message) }
  finally { overallLoading.value = false }
}

const saveOverallPrice = async () => {
  if (overallPercent.value === null || overallPercent.value === undefined || overallPercent.value < 0 || overallPercent.value > 200) {
    return message.warning('请输入 0-200 之间的有效百分比')
  }
  overallLoading.value = true
  try {
    await agentApi.setOverallPrice(Number(overallPercent.value))
    message.success('总体加价百分比已保存')
  } catch (error) { message.error(error.message) }
  finally { overallLoading.value = false }
}

const loadAgentWechatImage = async () => {
  if (!user.value.isAgent) return
  try {
    const result = await serviceImageApi.myOwn()
    wechatImageUrl.value = resolveAssetUrl(result.data?.imageUrl)
  } catch (error) { message.error(error.message) }
}

const uploadWechatImage = async ({ file, onSuccess, onError }) => {
  wechatUploadLoading.value = true
  try {
    const uploadResult = await ticketApi.upload([file])
    const urls = uploadResult.data || []
    if (!urls.length) throw new Error('图片上传失败')
    await serviceImageApi.upload(urls[0])
    wechatImageUrl.value = resolveAssetUrl(urls[0])
    message.success('客服微信图片已保存')
    onSuccess?.()
  } catch (error) {
    message.error(error.message)
    onError?.(error)
  } finally {
    wechatUploadLoading.value = false
  }
}

watch(() => [childQuery.enabled, childQuery.page, childQuery.pageSize], loadChildren)
watch(() => [markupQuery.page, markupQuery.pageSize], loadMarkups)
onMounted(async () => {
  // F2：先确保用户态就绪，再决定是否加载客服图片，避免全新登录首进漏加载
  await auth.loadUser()
  await Promise.all([loadDashboard(), loadChildren(), loadMarkups(), loadOverallPrice(), loadAgentWechatImage()])
})
</script>

<template>
  <div class="agent-page">
    <header class="page-header">
      <div><h2>代理管理</h2><p>管理直属用户、余额转赠与业务加价</p></div>
      <div class="header-actions"><Button @click="withdrawModal = true; amountForm.amount = null">提取代理余额</Button><Button type="primary" @click="childModal = true">新增用户</Button></div>
    </header>

    <Row :gutter="[16, 16]" class="stats">
      <Col :xs="24" :md="8"><Card><span>用户余额</span><strong>¥{{ formatPrice6(dashboard.userAmount) }}</strong></Card></Col>
      <Col :xs="24" :md="8"><Card><span>代理余额</span><strong>¥{{ formatPrice6(dashboard.agentAmount) }}</strong></Card></Col>
      <Col :xs="24" :md="8"><Card><span>启用用户 / 全部用户</span><strong>{{ dashboard.enabledChildrenCount }} / {{ dashboard.totalChildrenCount }}</strong></Card></Col>
    </Row>

    <Card class="section">
      <div class="toolbar"><h3>直属用户</h3><div class="filters"><Select v-model:value="childQuery.enabled" allow-clear placeholder="全部状态" style="width: 130px"><Select.Option :value="true">已启用</Select.Option><Select.Option :value="false">已停用</Select.Option></Select><Input.Search v-model:value="childQuery.keyword" placeholder="用户名或邮箱" @search="childQuery.page = 1; loadChildren()" /></div></div>
      <Spin :spinning="childLoading"><div v-if="children.length" class="grid">
        <article v-for="child in children" :key="child.userId" class="item">
          <div class="item-title"><strong>{{ child.username }}</strong><Tag :color="child.enabled ? 'green' : 'red'">{{ child.enabled ? '已启用' : '已停用' }}</Tag></div>
          <dl><dt>用户 ID</dt><dd>{{ child.userId }}</dd><dt>邮箱</dt><dd>{{ child.email }}</dd><dt>余额</dt><dd>¥{{ formatPrice6(child.userAmount) }}</dd><dt>创建时间</dt><dd>{{ child.createTime ? new Date(child.createTime).toLocaleString('zh-CN') : '-' }}</dd></dl>
          <div class="actions"><Button size="small" @click="openStatus(child)">状态</Button><Button size="small" type="primary" @click="openTransfer(child)">赠送余额</Button><Button size="small" danger @click="resetPassword(child)">重置密码</Button></div>
        </article>
      </div><Empty v-else-if="!childLoading" description="暂无直属用户" /></Spin>
      <Pagination v-model:current="childQuery.page" v-model:page-size="childQuery.pageSize" :total="childTotal" show-size-changer />
    </Card>

    <Row :gutter="[16, 16]" class="equal-height-row">
      <Col :xs="24" :md="12">
        <Card class="section">
          <div class="toolbar"><h3>总体加价设置</h3></div>
          <Spin :spinning="overallLoading">
            <div class="overall-form">
              <p class="tips">为所有业务统一设置加价百分比（0-200%），首次保存为新增，之后为修改。</p>
              <div class="overall-input-row">
                <InputNumber v-model:value="overallPercent" :min="0" :max="200" :precision="0" placeholder="请输入百分比" style="width: 160px">
                  <template #addonAfter>%</template>
                </InputNumber>
                <Button type="primary" :loading="overallLoading" @click="saveOverallPrice">保存</Button>
              </div>
            </div>
          </Spin>
        </Card>
      </Col>

      <Col v-if="user.isAgent" :xs="24" :md="12">
        <Card class="section">
          <div class="toolbar"><h3>客服微信图片</h3></div>
          <Spin :spinning="wechatUploadLoading">
            <div class="wechat-form">
              <p class="tips">上传你的微信二维码，下级用户点击首页「添加微信」时将优先展示此图片。</p>
              <div class="wechat-preview">
                <Image v-if="wechatImageUrl" :src="wechatImageUrl" alt="客服微信" class="wechat-preview-img" preview />
                <div v-else class="wechat-empty">暂未上传</div>
              </div>
              <Upload :show-upload-list="false" :custom-request="uploadWechatImage" accept="image/png,image/jpeg,image/jpg">
                <Button :loading="wechatUploadLoading" type="primary">{{ wechatImageUrl ? '更换图片' : '上传图片' }}</Button>
              </Upload>
            </div>
          </Spin>
        </Card>
      </Col>
    </Row>

    <Card class="section">
      <div class="toolbar"><h3>业务加价</h3><div class="filters"><Input.Search v-model:value="markupQuery.keyword" placeholder="搜索业务名称或ID" @search="markupQuery.page = 1; loadMarkups()" /><Button type="primary" @click="openAddMarkup">新增加价</Button></div></div>
      <Spin :spinning="markupLoading"><div v-if="markups.length" class="grid">
        <article v-for="item in markups" :key="item.configId" class="item">
          <div class="item-title"><strong>[{{ item.configId }}] {{ item.configName || `服务 ${item.configId}` }}</strong></div>
          <p class="tips">{{ item.configTips || '暂无说明' }}</p>
          <dl><dt>基础价格</dt><dd>¥{{ formatPrice6(item.basePrice) }}</dd><dt>加价金额</dt><dd>¥{{ formatPrice6(item.markupAddPrice) }}</dd><dt>下级展示价格</dt><dd>¥{{ formatPrice6(item.childDisplayPrice) }}/个</dd></dl>
          <div class="actions"><Button type="primary" size="small" @click="openMarkup(item)">设置加价</Button><Button size="small" danger @click="deleteMarkup(item)">删除</Button></div>
        </article>
      </div><Empty v-else-if="!markupLoading" description="暂无业务配置" /></Spin>
      <Pagination v-model:current="markupQuery.page" v-model:page-size="markupQuery.pageSize" :total="markupTotal" show-size-changer />
    </Card>

    <Modal v-model:open="childModal" title="新增直属用户" :confirm-loading="saving" @ok="saveChild"><div class="form"><label>邮箱</label><Input v-model:value="childForm.email"/><label>用户名</label><Input v-model:value="childForm.username"/><label>密码</label><Input.Password v-model:value="childForm.password"/></div></Modal>
    <Modal v-model:open="transferModal" :title="`向 ${selectedChild?.username || ''} 赠送余额`" :confirm-loading="saving" @ok="transfer"><InputNumber v-model:value="amountForm.amount" :min="0.01" :precision="6" style="width:100%"/></Modal>
    <Modal v-model:open="statusModal" :title="`修改 ${selectedChild?.username || ''} 状态`" :confirm-loading="saving" @ok="saveStatus"><Select v-model:value="statusEnabled" style="width:100%"><Select.Option :value="true">已启用</Select.Option><Select.Option :value="false">已停用</Select.Option></Select></Modal>
    <Modal v-model:open="markupModal" :title="selectedMarkup ? `设置 [${selectedMarkup.configId}] ${selectedMarkup.configName} 加价` : '设置加价'" :confirm-loading="saving" @ok="saveMarkup"><div class="form"><span>基础价格：¥{{ formatPrice6(selectedMarkup?.basePrice) }}/个</span><label>整包加价</label><InputNumber v-model:value="markupAmount" :min="0" :precision="6" style="width:100%"/></div></Modal>
    <Modal v-model:open="addMarkupModal" title="新增单业务加价" :confirm-loading="saving" @ok="saveAddMarkup">
      <Spin :spinning="addMarkupLoading">
        <div class="form markup-add-form">
          <label>平台</label>
          <Select v-model:value="addMarkupForm.platformId" :options="platformOptions" placeholder="请选择平台" show-search :filter-option="filterOption" style="width:100%" @change="loadSubsForMarkup" />
          <label>业务类型</label>
          <Select v-model:value="addMarkupForm.subPlatformId" :options="subPlatformOptions" placeholder="请选择业务类型" show-search :filter-option="filterOption" style="width:100%" @change="loadMarkupConfigs" />
          <label>业务配置</label>
          <Select v-model:value="addMarkupForm.configId" :options="markupConfigSelectOptions" placeholder="请选择业务配置" show-search :filter-option="filterOption" style="width:100%" @change="onMarkupConfigChange" />
          <div v-if="selectedMarkupConfig" class="base-price-hint">
            <span>基准价格：¥{{ formatPrice6(selectedMarkupConfig.basePrice) }}/个</span>
          </div>
          <label>加价金额</label>
          <InputNumber v-model:value="addMarkupForm.markupAddPrice" :min="0" :precision="6" placeholder="请输入加价金额" style="width:100%" />
        </div>
      </Spin>
    </Modal>
    <Modal v-model:open="withdrawModal" title="提取代理余额" :confirm-loading="saving" @ok="withdraw"><div class="form"><span>可提取：¥{{ formatPrice6(user.agentAmount) }}</span><InputNumber v-model:value="amountForm.amount" :min="0.01" :max="Number(user.agentAmount || 0)" :precision="6" style="width:100%"/></div></Modal>
  </div>
</template>

<style scoped lang="scss">
.agent-page { padding: 1.5rem; color: #253142; display: grid; gap: 1.25rem; }
.page-header,.toolbar,.header-actions,.filters,.item-title,.actions { display:flex; align-items:center; justify-content:space-between; gap:.75rem; flex-wrap:wrap; }
h2,h3,p { margin:0; } .page-header p,.tips,dt,.stats span { color:#68768a; }
.tips { line-height:1.6; min-height:4.8rem; max-height:4.8rem; overflow-y:auto; padding-right:.25rem; word-break:break-word; }
.stats :deep(.ant-card),.section { background:#f8fafb; border:1px solid #d5dde3; border-radius:8px; }
.stats :deep(.ant-card-body) { display:grid; gap:.35rem; } .stats strong { color:#586ee1; font-size:1.5rem; }
.equal-height-row { align-items: stretch; }
.equal-height-row > div { display: flex; }
.equal-height-row .section { flex: 1; }
.section :deep(.ant-card-body) { display:grid; gap:1rem; }
.grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(310px,1fr)); gap:1rem; }
.item { background:#e9eef2; border:1px solid #d5dde3; border-radius:8px; padding:1rem; display:grid; gap:.75rem; }
dl { display:grid; grid-template-columns:auto 1fr; gap:.45rem .8rem; margin:0; } dd { margin:0; text-align:right; word-break:break-word; }
.actions { justify-content:flex-end; } .form { display:grid; gap:.65rem; }
.overall-form { display:grid; gap:.75rem; }
.overall-form .tips { min-height:auto; max-height:none; }
.overall-input-row { display:flex; align-items:center; gap:.75rem; flex-wrap:wrap; }
.markup-add-form label { color:#68768a; font-size:.875rem; }
.base-price-hint { display:flex; gap:1rem; color:#f59e0b; font-size:.875rem; padding:.35rem 0; }
.wechat-form { display:grid; gap:.75rem; }
.wechat-form .tips { min-height:auto; max-height:none; }
.wechat-preview {
  width: 120px; height: 120px; border-radius: 8px; overflow: hidden;
  border: 1px solid #d5dde3; background: #eef2f5;
  display: flex; align-items: center; justify-content: center;
  img, :deep(.ant-image-img) { width: 100%; height: 100%; object-fit: cover; }
  :deep(.ant-image) { display: block; width: 100%; height: 100%; }
}
.wechat-empty { color:#68768a; font-size:.8rem; }
@media(max-width:768px){.agent-page{padding:1rem}.filters,.filters :deep(.ant-input-search){width:100%}.grid{grid-template-columns:1fr}.actions>*{flex:1}.overall-input-row :deep(.ant-input-number){width:100% !important}.markup-add-form :deep(.ant-select),.markup-add-form :deep(.ant-input-number){width:100% !important}}
</style>
