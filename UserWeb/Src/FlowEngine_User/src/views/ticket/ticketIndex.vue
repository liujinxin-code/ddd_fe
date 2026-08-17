<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { Button, Empty, Image, Input, Modal, Pagination, Select, Spin, Table, Tag, Upload, message } from 'ant-design-vue'
import { enumApi, resolveAssetUrl, ticketApi } from '../../api'
import { Icon } from '../../assets/js/iconUtils.js'

const tickets = ref([])
const total = ref(0)
const loading = ref(false)
const query = reactive({ keyword: '', status: undefined, type: undefined, page: 1, pageSize: 10 })

// ── 响应式断点（与订单/消费列表一致） ─────────────────────
const isMobile = ref(false)
const MOBILE_BREAKPOINT = 768
const checkMobile = () => { isMobile.value = window.innerWidth < MOBILE_BREAKPOINT }
onMounted(async () => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  try {
    const [tStatus, tType] = await Promise.all([enumApi.ticketStatus(), enumApi.ticketType()])
    statusEnum.value = tStatus.data || []
    typeEnum.value = tType.data || []
  } catch (error) {
    message.error(error.message || '加载枚举失败')
  }
  loadTickets()
})
onUnmounted(() => window.removeEventListener('resize', checkMobile))

// 枚举同步（状态 / 问题类型）
const statusEnum = ref([])
const typeEnum = ref([])
const statusOptions = computed(() => [
  { value: -1, label: '全部状态' },
  ...(statusEnum.value || []).map((i) => ({ value: i.value, label: i.label })),
])
const typeOptions = computed(() => [
  { value: -1, label: '全部类型' },
  ...(typeEnum.value || []).map((i) => ({ value: i.value, label: i.label })),
])
const statusMeta = computed(() => {
  const map = { 0: { text: '待处理', color: 'orange' }, 1: { text: '已处理', color: 'green' } }
  for (const i of statusEnum.value || []) {
    if (!map[i.value]) map[i.value] = { text: i.label, color: 'default' }
  }
  return map
})
const typeMeta = computed(() => {
  const colorMap = { 0: 'blue', 1: 'cyan', 2: 'geekblue', 3: 'purple' }
  const map = {}
  for (const i of typeEnum.value || []) {
    map[i.value] = { text: i.label, color: colorMap[i.value] || 'default' }
  }
  return map
})

// 时间格式化
const fmtTime = (value) => {
  if (!value) return '-'
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString('zh-CN', { hour12: false })
}

// ── 桌面端表格列 ─────────────────────────────────────────
const CONTENT_MAX = 24
const contentShort = (text) => {
  if (!text) return ''
  const t = text.replace(/\s+/g, ' ').trim()
  return t.length > CONTENT_MAX ? t.slice(0, CONTENT_MAX) + '…' : t
}

const columns = [
  { title: '工单编号', dataIndex: 'ticketNo', key: 'ticketNo', width: 180, ellipsis: true },
  { title: '问题内容', dataIndex: 'ticketContent', key: 'ticketContent', width: 260 },
  { title: '问题类型', dataIndex: 'ticketType', key: 'ticketType', width: 120, align: 'center' },
  { title: '状态', dataIndex: 'ticketStatus', key: 'ticketStatus', width: 100, align: 'center' },
  { title: '图片', dataIndex: 'ticketImages', key: 'ticketImages', width: 120, align: 'center' },
  { title: '提交时间', dataIndex: 'createTime', key: 'createTime', width: 172 },
]

const loadTickets = async () => {
  loading.value = true
  try {
    const result = await ticketApi.list(query)
    tickets.value = result.data || []
    total.value = result.count || 0
  } catch (error) {
    message.error(error.message)
  } finally {
    loading.value = false
  }
}
const search = () => { query.page = 1; loadTickets() }
const onPageChange = (page, pageSize) => {
  query.page = page
  query.pageSize = pageSize
  loadTickets()
}

// ── 提交工单弹窗 ─────────────────────────────────────────
const modalVisible = ref(false)
const submitting = ref(false)
const form = reactive({ ticketContent: '', ticketType: undefined })
const selectedFiles = ref([]) // { file, url } —— 本地预览用

const openModal = () => {
  form.ticketContent = ''
  form.ticketType = undefined
  clearFiles()
  modalVisible.value = true
}
const closeModal = () => { modalVisible.value = false }

// ── 详情弹窗（工单内容 / 处理结果 弹窗展示） ──
const detailVisible = ref(false)
const detailTicket = ref(null)
const openDetail = (record) => {
  detailTicket.value = record
  detailVisible.value = true
}
const closeDetail = () => { detailVisible.value = false }

const clearFiles = () => {
  selectedFiles.value.forEach((s) => URL.revokeObjectURL(s.url))
  selectedFiles.value = []
}

// 选择图片：仅 png/jpg，单张≤5MB，最多 5 张（返回 false 阻止自动上传，由提交时统一上传）
const beforeUpload = (file) => {
  const isImg = ['image/png', 'image/jpeg'].includes((file.type || '').toLowerCase())
  if (!isImg) {
    message.error('仅支持 PNG / JPG 格式图片')
    return Upload.LIST_IGNORE
  }
  const isLt5M = file.size / 1024 / 1024 < 5
  if (!isLt5M) {
    message.error('单张图片不能超过 5MB')
    return Upload.LIST_IGNORE
  }
  if (selectedFiles.value.length >= 5) {
    message.error('最多上传 5 张图片')
    return Upload.LIST_IGNORE
  }
  selectedFiles.value.push({ file, url: URL.createObjectURL(file) })
  return false
}
const removeImage = (idx) => {
  const item = selectedFiles.value[idx]
  if (item) URL.revokeObjectURL(item.url)
  selectedFiles.value.splice(idx, 1)
}

const submitTicket = async () => {
  if (!form.ticketContent || !form.ticketContent.trim()) {
    message.error('请填写工单内容')
    return
  }
  if (form.ticketType == null) {
    message.error('请选择问题类型')
    return
  }

  submitting.value = true
  try {
    let imageUrls = []
    if (selectedFiles.value.length) {
      const res = await ticketApi.upload(selectedFiles.value.map((s) => s.file))
      imageUrls = res.data || []
    }
    await ticketApi.create({
      ticketContent: form.ticketContent.trim(),
      ticketType: Number(form.ticketType),
      ticketImages: imageUrls,
    })
    message.success('工单已提交，我们会尽快处理')
    modalVisible.value = false
    query.page = 1
    loadTickets()
  } catch (error) {
    message.error(error.message)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="ticket-page">
    <header class="page-header">
      <div>
        <h2>联系客服</h2>
        <p>遇到问题？提交工单并上传截图，客服会尽快为你处理</p>
      </div>
      <Button type="primary" @click="openModal">
        <Icon icon="PlusOutlined" /> 添加问题
      </Button>
    </header>

    <section class="panel">
      <!-- 筛选工具栏 -->
      <div class="toolbar">
        <h3>我的工单</h3>
        <div class="filters">
          <Select
            v-model:value="query.type"
            :options="typeOptions"
            placeholder="全部类型"
            allow-clear
            :style="{ width: isMobile ? '100%' : '168px' }"
            @change="search"
          />
          <Select
            v-model:value="query.status"
            :options="statusOptions"
            placeholder="全部状态"
            allow-clear
            :style="{ width: isMobile ? '100%' : '150px' }"
            @change="search"
          />
          <Input.Search
            v-model:value="query.keyword"
            placeholder="搜索工单编号 / 内容"
            allow-clear
            :style="{ width: isMobile ? '100%' : '240px' }"
            @search="search"
          />
        </div>
      </div>

      <!-- ═══════ 桌面端：表格 ═══════ -->
      <Spin v-if="!isMobile" :spinning="loading">
        <Table
          v-if="tickets.length"
          :columns="columns"
          :data-source="tickets"
          :pagination="false"
          row-key="ticketId"
          size="middle"
          :scroll="{ x: 952 }"
          class="ticket-table"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'ticketNo'">
              <span class="no-cell" :title="record.ticketNo">{{ record.ticketNo || '—' }}</span>
            </template>
            <template v-else-if="column.key === 'ticketContent'">
              <div class="content-cell">
                <span :title="record.ticketContent">{{ contentShort(record.ticketContent) }}</span>
                <Button type="link" size="small" class="view-content-btn" @click="openDetail(record)">查看内容</Button>
              </div>
            </template>
            <template v-else-if="column.key === 'ticketType'">
              <Tag :color="typeMeta[record.ticketType]?.color || 'default'">
                {{ typeMeta[record.ticketType]?.text || `类型${record.ticketType}` }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'ticketStatus'">
              <Tag :color="statusMeta[record.ticketStatus]?.color || 'default'">
                {{ statusMeta[record.ticketStatus]?.text || `状态${record.ticketStatus}` }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'ticketImages'">
              <span v-if="record.ticketImages && record.ticketImages.length" class="img-cell">
                <Image.PreviewGroup>
                  <Image
                    :width="40"
                    :height="40"
                    :src="resolveAssetUrl(record.ticketImages[0])"
                    :preview-src="resolveAssetUrl(record.ticketImages[0])"
                  />
                </Image.PreviewGroup>
                <span v-if="record.ticketImages.length > 1" class="img-more">+{{ record.ticketImages.length - 1 }}</span>
              </span>
              <span v-else class="muted">—</span>
            </template>
            <template v-else-if="column.key === 'createTime'">
              {{ fmtTime(record.createTime) }}
            </template>
          </template>
        </Table>
        <Empty v-else-if="!loading" description="暂无工单，点击右上角「添加问题」提交" />
      </Spin>

      <!-- ═══════ 移动端：卡片列表 ═══════ -->
      <div v-else class="mobile-list">
        <Spin :spinning="loading">
          <div v-if="tickets.length" class="mobile-cards">
            <div v-for="t in tickets" :key="t.ticketId" class="ticket-card">
              <div class="card-no">工单编号：{{ t.ticketNo || '—' }}</div>
              <div class="card-head">
                <Tag :color="typeMeta[t.ticketType]?.color || 'default'" class="card-tag">
                  {{ typeMeta[t.ticketType]?.text || `类型${t.ticketType}` }}
                </Tag>
                <Tag :color="statusMeta[t.ticketStatus]?.color || 'default'" class="card-tag">
                  {{ statusMeta[t.ticketStatus]?.text || `状态${t.ticketStatus}` }}
                </Tag>
              </div>
              <div class="card-content-preview">
                <span :title="t.ticketContent">{{ contentShort(t.ticketContent) }}</span>
                <Button type="link" size="small" @click="openDetail(t)">查看内容</Button>
              </div>
              <div v-if="t.ticketImages && t.ticketImages.length" class="card-imgs">
                <Image.PreviewGroup>
                  <Image
                    v-for="(img, i) in t.ticketImages.slice(0, 4)"
                    :key="i"
                    :width="56"
                    :height="56"
                    :src="resolveAssetUrl(img)"
                    :preview-src="resolveAssetUrl(img)"
                  />
                </Image.PreviewGroup>
              </div>
              <div class="card-foot">
                <span class="card-time">{{ fmtTime(t.createTime) }}</span>
              </div>
            </div>
          </div>
          <Empty v-else-if="!loading" description="暂无工单，点击右上角「添加问题」提交" />
        </Spin>
      </div>

      <!-- 分页 -->
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

    <!-- ═══════ 提交工单弹窗 ═══════ -->
    <Modal
      v-model:open="modalVisible"
      title="添加问题"
      :width="isMobile ? '95vw' : '560px'"
      :confirm-loading="submitting"
      :footer="null"
      @cancel="closeModal"
    >
      <div class="ticket-form">
        <div class="form-item">
          <label class="form-label">问题类型 <span class="req">*</span></label>
          <Select
            v-model:value="form.ticketType"
            :options="typeEnum.map((i) => ({ value: i.value, label: i.label }))"
            placeholder="请选择问题类型"
            style="width: 100%"
          />
        </div>

        <div class="form-item">
          <label class="form-label">工单内容 <span class="req">*</span></label>
          <Input.TextArea
            v-model:value="form.ticketContent"
            :rows="4"
            :maxlength="3000"
            show-count
            placeholder="请描述您遇到的问题（最多 3000 字）"
          />
        </div>

        <div class="form-item">
          <label class="form-label">上传图片（最多 5 张，单张 ≤ 5MB，仅 PNG / JPG）</label>
          <div class="upload-row">
            <Upload
              :before-upload="beforeUpload"
              :show-upload-list="false"
              :multiple="true"
              accept="image/png,image/jpeg"
              list-type="picture-card"
              class="img-uploader"
            >
              <div v-if="selectedFiles.length < 5" class="upload-trigger">
                <Icon icon="PlusOutlined" />
                <div class="upload-text">上传</div>
              </div>
            </Upload>
            <div v-if="selectedFiles.length" class="preview-grid">
              <Image.PreviewGroup>
                <div v-for="(s, idx) in selectedFiles" :key="idx" class="preview-item">
                  <Image :width="64" :height="64" :src="s.url" />
                  <span class="preview-remove" @click.stop="removeImage(idx)"><Icon icon="CloseOutlined" /></span>
                </div>
              </Image.PreviewGroup>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <Button @click="closeModal">取消</Button>
          <Button type="primary" :loading="submitting" @click="submitTicket">提交工单</Button>
        </div>
      </div>
    </Modal>

    <!-- ═══════ 工单详情弹窗（内容 + 处理结果） ═══════ -->
    <Modal
      v-model:open="detailVisible"
      :title="detailTicket ? `工单详情 · ${detailTicket.ticketNo || ''}` : '工单详情'"
      :width="isMobile ? '95vw' : '560px'"
      :footer="null"
      @cancel="closeDetail"
    >
      <div v-if="detailTicket" class="detail-body">
        <div class="detail-meta">
          <Tag :color="typeMeta[detailTicket.ticketType]?.color || 'default'">
            {{ typeMeta[detailTicket.ticketType]?.text || `类型${detailTicket.ticketType}` }}
          </Tag>
          <Tag :color="statusMeta[detailTicket.ticketStatus]?.color || 'default'">
            {{ statusMeta[detailTicket.ticketStatus]?.text || `状态${detailTicket.ticketStatus}` }}
          </Tag>
          <span class="detail-time">{{ fmtTime(detailTicket.createTime) }}</span>
        </div>

        <div class="detail-block">
          <div class="detail-label">工单内容</div>
          <div class="detail-content">{{ detailTicket.ticketContent }}</div>
        </div>

        <div v-if="detailTicket.ticketImages && detailTicket.ticketImages.length" class="detail-block">
          <div class="detail-label">图片</div>
          <Image.PreviewGroup>
            <div class="detail-imgs">
              <Image
                v-for="(img, i) in detailTicket.ticketImages"
                :key="i"
                :width="72"
                :height="72"
                :src="resolveAssetUrl(img)"
                :preview-src="resolveAssetUrl(img)"
              />
            </div>
          </Image.PreviewGroup>
        </div>

        <div class="detail-block">
          <div class="detail-label">处理结果</div>
          <div v-if="detailTicket.ticketResult" class="detail-content result">{{ detailTicket.ticketResult }}</div>
          <div v-else class="detail-empty">暂未处理</div>
        </div>
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

.ticket-page {
  display: grid;
  gap: 1rem;
  padding: 0.75rem 0 1.25rem;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  flex-wrap: wrap;
  h2 { margin: 0; font-size: 1.15rem; font-weight: 600; color: $text-primary; }
  p { margin: 0.25rem 0 0; font-size: 0.82rem; color: $text-muted; }
  :deep(.ant-btn) { display: inline-flex; align-items: center; gap: 0.3rem; }
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
.filters { display: flex; align-items: center; gap: 0.6rem; flex-wrap: wrap; }

.ticket-table {
  :deep(.ant-table) { font-size: 0.82rem; }
  :deep(.ant-table-thead > tr > th) {
    background: #f8fafb;
    color: $text-secondary;
    font-weight: 600;
    white-space: nowrap;
  }
  :deep(.ant-table-tbody > tr:hover > td) { background: #f6f8ff; }
}
.no-cell { color: $text-secondary; font-variant-numeric: tabular-nums; font-size: 0.8rem; }
.content-cell {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.25rem;
  color: $text-primary;
  line-height: 1.45;
}
.view-content-btn { padding: 0; height: auto; font-size: 0.75rem; }
.muted { color: $text-muted; }
.img-cell { display: inline-flex; align-items: center; gap: 0.3rem; }
.img-more {
  font-size: 0.72rem;
  color: $text-muted;
  background: #f1f5f9;
  border-radius: 999px;
  padding: 0.05rem 0.35rem;
}
.pager { display: flex; justify-content: flex-end; }

/* ═══════ 移动端卡片 ═══════ */
.mobile-list { min-height: 120px; }
.mobile-cards { display: grid; gap: 0.75rem; }
.ticket-card {
  background: #fafbfc;
  border: 1px solid $border-base;
  border-radius: 10px;
  padding: 0.85rem 0.75rem;
  display: grid;
  gap: 0.55rem;
  transition: box-shadow 0.2s;
  &:active { box-shadow: 0 2px 8px rgba(88, 110, 225, 0.12); }
}
.card-head { display: flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; }
.card-no {
  font-size: 0.74rem;
  color: $text-muted;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0.02em;
}
.card-tag { font-size: 0.72rem; margin: 0; }
.card-content-preview {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.82rem;
  color: $text-primary;
  line-height: 1.45;
  word-break: break-word;
}
.card-imgs { display: flex; gap: 0.4rem; flex-wrap: wrap; }
.card-foot {
  padding-top: 0.5rem;
  border-top: 1px dashed #eaecef;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}
.card-time { font-size: 0.72rem; color: $text-muted; }

/* ═══════ 详情弹窗 ═══════ */
.detail-body { display: grid; gap: 1rem; padding-top: 0.25rem; }
.detail-meta { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.detail-time { font-size: 0.74rem; color: $text-muted; margin-left: auto; }
.detail-block { display: grid; gap: 0.4rem; }
.detail-label { font-size: 0.8rem; font-weight: 600; color: $text-secondary; }
.detail-content {
  font-size: 0.85rem;
  color: $text-primary;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
  background: #f8fafb;
  border: 1px solid #eef1f5;
  border-radius: 8px;
  padding: 0.6rem 0.7rem;
}
.detail-content.result { background: #f3f8f4; border-color: #dcefe0; }
.detail-empty { font-size: 0.82rem; color: $text-muted; }
.detail-imgs { display: flex; gap: 0.5rem; flex-wrap: wrap; }

/* ═══════ 提交弹窗 ═══════ */
.ticket-form { display: grid; gap: 1rem; padding-top: 0.25rem; }
.form-item { display: grid; gap: 0.4rem; }
.form-label { font-size: 0.82rem; font-weight: 600; color: $text-primary; }
.req { color: #ef4444; }
.upload-row { display: flex; align-items: flex-start; gap: 0.75rem; flex-wrap: wrap; }
.img-uploader {
  :deep(.ant-upload.ant-upload-select-picture-card) {
    width: 72px;
    height: 72px;
    margin: 0;
    border-radius: 8px;
  }
}
.upload-trigger {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.15rem;
  color: $text-muted;
  font-size: 0.72rem;
}
.preview-grid { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.preview-item { position: relative; border-radius: 8px; }
.preview-item :deep(.ant-image-img) { border-radius: 8px; display: block; }
.preview-remove {
  position: absolute;
  top: 2px;
  right: 2px;
  width: 20px;
  height: 20px;
  display: grid;
  place-items: center;
  background: rgba(30, 41, 59, 0.9);
  color: #fff;
  border-radius: 50%;
  font-size: 0.65rem;
  cursor: pointer;
  z-index: 2;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
}
.form-actions { display: flex; justify-content: flex-end; gap: 0.6rem; }

@media (max-width: 768px) {
  .ticket-page { padding: 0.5rem 0 1rem; }
  .panel { padding: 0.75rem; border-radius: 10px; }
  .toolbar { flex-direction: column; align-items: stretch; h3 { font-size: 0.9rem; } }
  .filters { flex-direction: column; width: 100%; }
  .pager { justify-content: center; }
}
@media (max-width: 380px) {
  .ticket-card { padding: 0.7rem 0.6rem; }
}
</style>
