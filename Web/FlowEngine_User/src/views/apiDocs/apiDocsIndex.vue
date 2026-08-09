<script setup>
import { computed, onMounted, ref } from 'vue'
import { Button, Card, Empty, Input, Modal, Select, Spin, Tag, message } from 'ant-design-vue'
import { apiKeyApi, homeApi } from '../../api'
import { Icon } from '../../assets/js/iconUtils.js'

const loading = ref(false)
const apiKey = ref('')
const revealed = ref(false)
const passwordModalVisible = ref(false)
const password = ref('')
const passwordLoading = ref(false)
const passwordMode = ref('view') // 'view' | 'generate'，当前只用于查看，生成不再弹窗

const platforms = ref([])
const subPlatforms = ref([])
const selectedPlatform = ref(undefined)
const selectedSubPlatform = ref(undefined)
const configs = ref([])
const configsLoading = ref(false)

const baseUrl = computed(() => {
  const apiBase = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')
  return apiBase.replace(/\/api$/, '')
})

const displayKey = computed(() => {
  if (!apiKey.value) return '*****'
  return revealed.value ? apiKey.value : '****' + apiKey.value.slice(-4)
})

const loadPlatforms = async () => {
  try {
    const result = await homeApi.platforms()
    platforms.value = result.data || []
  } catch (error) {
    message.error(error.message || '加载平台失败')
  }
}

const onPlatformChange = async (pid) => {
  selectedPlatform.value = pid
  selectedSubPlatform.value = undefined
  configs.value = []
  if (pid == null) {
    subPlatforms.value = []
    return
  }
  try {
    const result = await homeApi.subs(pid)
    subPlatforms.value = result.data || []
  } catch (error) {
    message.error(error.message || '加载业务类型失败')
  }
}

const onSubPlatformChange = async (sid) => {
  selectedSubPlatform.value = sid
  configs.value = []
  if (sid == null) return
  configsLoading.value = true
  try {
    const result = await homeApi.apiConfigs({
      platformId: selectedPlatform.value,
      subPlatformId: sid,
      page: 1,
      pageSize: 1000,
    })
    configs.value = result.data || []
  } catch (error) {
    message.error(error.message || '加载业务配置失败')
  } finally {
    configsLoading.value = false
  }
}

const copyToClipboard = (text) => {
  navigator.clipboard.writeText(text).then(() => message.success('已复制'))
}

const handleGenerate = async () => {
  loading.value = true
  try {
    const result = await apiKeyApi.generate()
    apiKey.value = result.data?.apiKey || ''
    revealed.value = true
    copyToClipboard(apiKey.value)
    message.success('新的 API Key 已生成并复制')
  } catch (error) {
    message.error(error.message || '生成失败')
  } finally {
    loading.value = false
  }
}

const openViewModal = () => {
  passwordMode.value = 'view'
  password.value = ''
  passwordModalVisible.value = true
}

const handlePasswordSubmit = async () => {
  if (!password.value) {
    message.warning('请输入登录密码')
    return
  }
  passwordLoading.value = true
  try {
    const result = await apiKeyApi.view(password.value)
    apiKey.value = result.data?.apiKey || ''
    revealed.value = true
    passwordModalVisible.value = false
    password.value = ''
    message.success(apiKey.value ? 'API Key 已展示' : '当前暂无 API Key，请先生成')
  } catch (error) {
    message.error(error.message || '验证失败')
  } finally {
    passwordLoading.value = false
  }
}

const authHeader = computed(() => `Authorization: Bearer ${apiKey.value || 'YOUR_API_KEY'}`)

const exampleOrderCreate = computed(() => ({
  method: 'POST',
  url: `${baseUrl.value}/api/Order/create`,
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${apiKey.value || 'YOUR_API_KEY'}`,
  },
  body: {
    items: [
      { configId: 1, orderLink: 'https://example.com/target', quantity: 100, comments: [] },
    ],
  },
}))

const exampleOrderList = computed(() => ({
  method: 'POST',
  url: `${baseUrl.value}/api/Order/list`,
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${apiKey.value || 'YOUR_API_KEY'}`,
  },
  body: { orderState: 0, keyword: '', pageIndex: 1, pageSize: 10, sorting: 'createtime desc' },
}))

const exampleOrderDetail = computed(() => ({
  method: 'POST',
  url: `${baseUrl.value}/api/Order/list`,
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${apiKey.value || 'YOUR_API_KEY'}`,
  },
  body: { orderState: 0, keyword: 'O20260808123456', pageIndex: 1, pageSize: 10, sorting: 'createtime desc' },
  note: '当前订单详情通过「订单列表」接口按订单号 keyword 查询。',
}))

const exampleBalance = computed(() => ({
  method: 'GET',
  url: `${baseUrl.value}/api/User/balance`,
  headers: { 'Authorization': `Bearer ${apiKey.value || 'YOUR_API_KEY'}` },
}))

onMounted(() => {
  loadPlatforms()
})
</script>

<template>
  <div class="api-docs-page">
    <header class="page-header">
      <h2>API 文档</h2>
      <p>通过 API Key 调用平台接口，实现自动化下单与查询。</p>
    </header>

    <section class="panel">
      <h3 class="panel-title"><Icon icon="KeyOutlined" /> 我的 API Key</h3>
      <div class="token-row">
        <div class="token-value" :title="apiKey">{{ displayKey }}</div>
        <Button type="primary" size="small" :loading="loading" @click="handleGenerate">
          <Icon icon="ThunderboltOutlined" /> 生成 API Key
        </Button>
        <Button size="small" @click="openViewModal">
          <Icon icon="EyeOutlined" /> 查看 API Key
        </Button>
        <Button v-if="apiKey" size="small" @click="copyToClipboard(apiKey)">
          <Icon icon="CopyOutlined" /> 复制
        </Button>
      </div>
      <ul class="token-tip">
        <li><strong>生成</strong>：直接生成新的长期 API Key 并覆盖旧 Key，新 Key 有效期 <strong>20 年</strong>，生成后自动复制。</li>
        <li><strong>查看</strong>：需要输入登录密码，校验后展示当前 Key，不会生成新 Key。</li>
        <li>调用接口时请在请求头中携带：<code>{{ authHeader }}</code></li>
      </ul>
    </section>

    <section class="panel">
      <h3 class="panel-title"><Icon icon="AppstoreOutlined" /> 业务配置查询</h3>
      <div class="selector-row">
        <Select
          v-model:value="selectedPlatform"
          :options="platforms.map((p) => ({ value: p.platformId, label: p.platformName }))"
          placeholder="请选择平台"
          allow-clear
          :style="{ width: '220px' }"
          @change="onPlatformChange"
        />
        <Select
          v-model:value="selectedSubPlatform"
          :options="subPlatforms.map((s) => ({ value: s.subPlatformId, label: s.subPlatformName }))"
          placeholder="请选择业务类型"
          allow-clear
          :disabled="!selectedPlatform"
          :style="{ width: '220px' }"
          @change="onSubPlatformChange"
        />
      </div>

      <Spin v-if="configsLoading" />
      <div v-else-if="configs.length" class="json-block-wrapper">
        <div class="json-toolbar">
          <span>共 {{ configs.length }} 项业务配置</span>
          <Button size="small" @click="copyToClipboard(JSON.stringify(configs, null, 2))">
            <Icon icon="CopyOutlined" /> 复制 JSON
          </Button>
        </div>
        <pre class="json-block">{{ JSON.stringify(configs, null, 2) }}</pre>

        <h4 class="field-title">业务配置字段说明</h4>
        <table class="field-table">
          <thead>
            <tr><th>字段</th><th>类型</th><th>说明</th></tr>
          </thead>
          <tbody>
            <tr><td>configId</td><td>int</td><td>业务配置唯一标识，下单时传入 Items.ConfigId</td></tr>
            <tr><td>configName</td><td>string</td><td>业务名称，如「抖音粉丝」「微博评论」</td></tr>
            <tr><td>unitPrice</td><td>decimal</td><td>当前用户最终单价（单个数量）</td></tr>
            <tr><td>minQuantity</td><td>int</td><td>最小下单数量，0 表示无下限</td></tr>
            <tr><td>maxQuantity</td><td>int</td><td>最大下单数量，0 表示无上限</td></tr>
            <tr><td>orderUnit</td><td>int</td><td>下单数量必须被此单位整除，0 表示无约束</td></tr>
            <tr><td>jsonTemplate</td><td>int</td><td>模板类型：1 粉丝 / 2 评论 / 3 购买账户</td></tr>
          </tbody>
        </table>
      </div>
      <Empty v-else-if="selectedSubPlatform" description="该业务类型下暂无配置" />
      <div v-else class="placeholder-text">请先选择平台和业务类型查看 JSON 数据。</div>
    </section>

    <section class="panel">
      <h3 class="panel-title"><Icon icon="FileTextOutlined" /> 接口调用说明</h3>

      <div class="api-section">
        <h4>1. 创建订单</h4>
        <p class="api-desc">批量创建订单，系统会即时扣减余额。整批原子：任一明细失败则全部不创建并回滚。</p>
        <div class="code-toolbar">
          <Tag color="blue">{{ exampleOrderCreate.method }}</Tag>
          <span class="code-url">{{ exampleOrderCreate.url }}</span>
        </div>
        <pre class="code-block">{{ JSON.stringify(exampleOrderCreate, null, 2) }}</pre>

        <h4 class="field-title">请求参数</h4>
        <table class="field-table">
          <thead><tr><th>字段</th><th>类型</th><th>必填</th><th>说明</th></tr></thead>
          <tbody>
            <tr><td>items</td><td>array</td><td>是</td><td>订单明细数组，一批最多 100 条</td></tr>
            <tr><td>items[].configId</td><td>int</td><td>是</td><td>业务配置 id，对应上面 JSON 中的 configId</td></tr>
            <tr><td>items[].orderLink</td><td>string</td><td>视模板</td><td>粉丝/评论模板必填；购买账户模板可空，最长 500 字符</td></tr>
            <tr><td>items[].quantity</td><td>int</td><td>视模板</td><td>粉丝/账户模板必填；评论模板忽略，以 comments 条数为准</td></tr>
            <tr><td>items[].comments</td><td>string[]</td><td>视模板</td><td>仅评论模板必填，一条评论 = 一个数量，每条最长 500 字符</td></tr>
          </tbody>
        </table>

        <h4 class="field-title">响应字段</h4>
        <table class="field-table">
          <thead><tr><th>字段</th><th>类型</th><th>说明</th></tr></thead>
          <tbody>
            <tr><td>code</td><td>int</td><td>业务码，200 表示成功</td></tr>
            <tr><td>message</td><td>string</td><td>提示信息</td></tr>
            <tr><td>data.orderNos</td><td>string[]</td><td>本次创建成功的订单号列表，顺序与请求明细一致</td></tr>
            <tr><td>data.totalAmount</td><td>decimal</td><td>总扣款金额</td></tr>
          </tbody>
        </table>

        <p class="api-desc">成功响应示例：</p>
        <pre class="code-block">{{ JSON.stringify({ code: 200, message: 'success', data: { orderNos: ['O20260808123456'], totalAmount: 10.5 } }, null, 2) }}</pre>
      </div>

      <div class="api-section">
        <h4>2. 查询订单列表 / 订单详情</h4>
        <p class="api-desc">通过订单号关键字查询订单详情，或通过状态分页查询订单列表。</p>
        <div class="code-toolbar">
          <Tag color="blue">{{ exampleOrderList.method }}</Tag>
          <span class="code-url">{{ exampleOrderList.url }}</span>
        </div>
        <pre class="code-block">{{ JSON.stringify(exampleOrderList, null, 2) }}</pre>

        <h4 class="field-title">请求参数</h4>
        <table class="field-table">
          <thead><tr><th>字段</th><th>类型</th><th>必填</th><th>说明</th></tr></thead>
          <tbody>
            <tr><td>orderState</td><td>int</td><td>否</td><td>订单状态筛选：0 不筛选 / 1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消</td></tr>
            <tr><td>keyword</td><td>string</td><td>否</td><td>匹配订单号或下单链接；传订单号可定位到具体订单</td></tr>
            <tr><td>pageIndex</td><td>int</td><td>否</td><td>页码，从 1 开始，默认 1</td></tr>
            <tr><td>pageSize</td><td>int</td><td>否</td><td>每页条数，默认 20，最大 100</td></tr>
            <tr><td>sorting</td><td>string</td><td>否</td><td>排序表达式，如 createtime desc；白名单 createtime/orderamount/quantity/orderstate</td></tr>
          </tbody>
        </table>

        <h4 class="field-title">响应字段</h4>
        <table class="field-table">
          <thead><tr><th>字段</th><th>类型</th><th>说明</th></tr></thead>
          <tbody>
            <tr><td>code</td><td>int</td><td>业务码</td></tr>
            <tr><td>message</td><td>string</td><td>提示信息</td></tr>
            <tr><td>data</td><td>array</td><td>订单列表</td></tr>
            <tr><td>data[].configId</td><td>int</td><td>业务配置 id，对应业务配置列表中的 configId</td></tr>
            <tr><td>data[].orderNo</td><td>string</td><td>业务订单号</td></tr>
            <tr><td>data[].orderState</td><td>int</td><td>订单状态：1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消</td></tr>
            <tr><td>data[].platformName</td><td>string</td><td>平台名称，如抖音</td></tr>
            <tr><td>data[].subPlatformName</td><td>string</td><td>业务类型名称，如涨粉</td></tr>
            <tr><td>data[].configName</td><td>string</td><td>业务配置名称</td></tr>
            <tr><td>data[].orderLink</td><td>string</td><td>下单链接</td></tr>
            <tr><td>data[].quantity</td><td>int</td><td>下单数量</td></tr>
            <tr><td>data[].successQuantity</td><td>int</td><td>成功数量</td></tr>
            <tr><td>data[].orderAmount</td><td>decimal</td><td>订单金额</td></tr>
            <tr><td>data[].refundAmount</td><td>decimal</td><td>退费金额</td></tr>
            <tr><td>data[].createTime</td><td>string</td><td>下单时间</td></tr>
            <tr><td>dataTotal</td><td>int</td><td>符合条件的总记录数</td></tr>
          </tbody>
        </table>

        <p class="api-desc">按订单号查详情示例：</p>
        <pre class="code-block">{{ JSON.stringify(exampleOrderDetail, null, 2) }}</pre>
        <p class="api-desc">成功响应示例：</p>
        <pre class="code-block">{{ JSON.stringify({ code: 200, message: 'success', data: [{ configId: 1, orderNo: 'O20260808123456', orderState: 1, platformName: '抖音', subPlatformName: '涨粉', configName: '抖音粉丝', orderLink: 'https://example.com', orderAmount: 10.5, quantity: 100, successQuantity: 0, refundAmount: 0, createTime: '2026-08-08 12:34:56' }], dataTotal: 1 }, null, 2) }}</pre>
      </div>

      <div class="api-section">
        <h4>3. 查询用户余额</h4>
        <p class="api-desc">通过 API Key 查询当前账户余额。</p>
        <div class="code-toolbar">
          <Tag color="green">{{ exampleBalance.method }}</Tag>
          <span class="code-url">{{ exampleBalance.url }}</span>
        </div>
        <pre class="code-block">{{ JSON.stringify(exampleBalance, null, 2) }}</pre>

        <h4 class="field-title">响应字段</h4>
        <table class="field-table">
          <thead><tr><th>字段</th><th>类型</th><th>说明</th></tr></thead>
          <tbody>
            <tr><td>code</td><td>int</td><td>业务码</td></tr>
            <tr><td>message</td><td>string</td><td>提示信息</td></tr>
            <tr><td>data.userAmount</td><td>decimal</td><td>用户余额（可用于下单）</td></tr>
          </tbody>
        </table>

        <p class="api-desc">成功响应示例：</p>
        <pre class="code-block">{{ JSON.stringify({ code: 200, message: 'success', data: { userAmount: 1250.5 } }, null, 2) }}</pre>
      </div>
    </section>

    <Modal
      v-model:open="passwordModalVisible"
      title="验证身份"
      :footer="null"
      width="400px"
      centered
      @cancel="password.value = ''"
    >
      <div class="password-form">
        <p>请输入登录密码以查看 API Key</p>
        <Input.Password v-model:value="password" placeholder="登录密码" @press-enter="handlePasswordSubmit" />
        <Button type="primary" block :loading="passwordLoading" @click="handlePasswordSubmit">
          确认
        </Button>
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
$r-md: 12px;

.api-docs-page {
  display: grid;
  gap: 1rem;
  padding: 0.75rem 0 1.25rem;
  min-width: 0;
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
  min-width: 0;
  overflow-x: hidden;
}

.panel-title {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: $text-primary;
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.token-row {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  flex-wrap: wrap;
}

.token-value {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.95rem;
  color: $text-primary;
  background: #f6f8fa;
  border: 1px solid #e5e9ef;
  border-radius: 8px;
  padding: 0.45rem 0.75rem;
  flex: 1 1 280px;
  min-width: 0;
  max-width: 100%;
  word-break: break-all;
  overflow-wrap: anywhere;
  white-space: normal;
}

.token-tip {
  margin: 0;
  padding-left: 1.1rem;
  font-size: 0.75rem;
  color: $text-muted;
  line-height: 1.7;
  code {
    background: #f1f5f9;
    padding: 0.1rem 0.35rem;
    border-radius: 4px;
    color: $text-secondary;
    font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
    word-break: break-all;
    overflow-wrap: anywhere;
    white-space: normal;
  }
}

.selector-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.placeholder-text {
  color: $text-muted;
  font-size: 0.82rem;
  padding: 1rem 0;
}

.json-block-wrapper {
  display: grid;
  gap: 0.75rem;
}

.json-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 0.78rem;
  color: $text-secondary;
}

.json-block,
.code-block {
  margin: 0;
  background: #0f172a;
  color: #e2e8f0;
  padding: 0.9rem;
  border-radius: 8px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.78rem;
  line-height: 1.55;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
  overflow-wrap: anywhere;
  max-width: 100%;
}

.json-block {
  max-height: 420px;
  overflow-y: auto;
}

@media (max-width: 768px) {
  .json-block {
    max-height: 320px;
  }
}

.api-section {
  display: grid;
  gap: 0.55rem;

  h4 {
    margin: 0;
    font-size: 0.88rem;
    font-weight: 600;
    color: $text-primary;
  }
}

.field-title {
  margin: 0.5rem 0 0;
  font-size: 0.8rem;
  font-weight: 600;
  color: $text-primary;
}

.field-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.76rem;
  color: $text-secondary;
  th, td {
    border: 1px solid $border-base;
    padding: 0.4rem 0.55rem;
    text-align: left;
  }
  th {
    background: #f8fafc;
    font-weight: 600;
    color: $text-primary;
  }
  td:first-child {
    font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
    color: $text-primary;
  }
}

.api-desc {
  margin: 0;
  font-size: 0.8rem;
  color: $text-secondary;
}

.api-warning {
  margin: 0;
  font-size: 0.76rem;
  color: #b45309;
  background: #fff7ed;
  padding: 0.5rem 0.65rem;
  border-radius: 6px;
}

.code-toolbar {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.78rem;
}

.code-url {
  color: $text-secondary;
  word-break: break-all;
}

.password-form {
  display: grid;
  gap: 0.85rem;
  padding-top: 0.25rem;

  p { margin: 0; font-size: 0.82rem; color: $text-secondary; }
}

@media (max-width: 768px) {
  .api-docs-page { padding: 0.5rem 0 1rem; }
  .panel { padding: 0.85rem; border-radius: 10px; }
  .selector-row { flex-direction: column; align-items: stretch; }
  .selector-row .ant-select { width: 100% !important; }
  .token-value { min-width: 0; width: 100%; }
  .token-row { flex-direction: column; align-items: stretch; }
  .field-table { font-size: 0.7rem; }
  .field-table th, .field-table td { padding: 0.3rem; }
}
</style>
