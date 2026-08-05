<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { Button, Modal, Progress } from 'ant-design-vue'

const props = defineProps({
  open: { type: Boolean, default: false },
  title: { type: String, default: '执行详情' },
  initialValues: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:open'])

// ── 响应式断点 ──
const isMobile = ref(false)
const checkMobile = () => { isMobile.value = window.innerWidth < 768 }
onMounted(() => { checkMobile(); window.addEventListener('resize', checkMobile) })
onUnmounted(() => { window.removeEventListener('resize', checkMobile) })

const innerVisible = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value),
})

// 初始数量 = 下单时的数量（begin_quantity）；成功数量 = 履约回填（success_quantity）
const beginQuantity = computed(() => Number(props.initialValues?.quantity) || 0)
const successQuantity = computed(() => Number(props.initialValues?.successCount) || 0)
const remainQuantity = computed(() => Math.max(beginQuantity.value - successQuantity.value, 0))

const percent = computed(() => {
  if (beginQuantity.value <= 0) return 0
  return Math.min(Math.round((successQuantity.value / beginQuantity.value) * 100), 100)
})

const progressStatus = computed(() => {
  if (percent.value >= 100) return 'success'
  return 'active'
})

const handleClose = () => emit('update:open', false)
</script>

<template>
  <Modal
    v-model:open="innerVisible"
    :title="title"
    centered
    destroy-on-close
    :width="isMobile ? '95vw' : 480"
    :footer="null"
  >
    <div class="execution-detail">
      <p v-if="initialValues?.orderNo" class="order-no">订单号 {{ initialValues.orderNo }}</p>

      <div class="stats">
        <div class="stat">
          <span class="stat-label">初始数量</span>
          <span class="stat-value">{{ beginQuantity }}</span>
        </div>
        <div class="stat">
          <span class="stat-label">成功数量</span>
          <span class="stat-value is-success">{{ successQuantity }}</span>
        </div>
        <div class="stat">
          <span class="stat-label">待完成</span>
          <span class="stat-value" :class="{ 'is-pending': remainQuantity > 0 }">{{ remainQuantity }}</span>
        </div>
      </div>

      <div class="progress-block">
        <div class="progress-head">
          <span>完成进度</span>
          <strong>{{ percent }}%</strong>
        </div>
        <Progress :percent="percent" :status="progressStatus" :show-info="false" stroke-color="#586ee1" />
      </div>

      <p class="tips">数据由渠道履约回传，存在分钟级延迟；部分完成的订单会按未完成数量退费。</p>

      <div class="modal-actions">
        <Button type="primary" @click="handleClose">关闭</Button>
      </div>
    </div>
  </Modal>
</template>

<style scoped lang="scss">
$text-primary: #1e293b;
$text-muted: #94a3b8;
$color-primary: #586ee1;
$color-accent: #f59e0b;

.execution-detail {
  display: grid;
  gap: 1rem;
}

.order-no {
  margin: 0;
  font-size: 0.8rem;
  color: $text-muted;
  word-break: break-all;
}

.stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.6rem;
}

@media (max-width: 480px) {
  .stats {
    grid-template-columns: repeat(3, 1fr);
    gap: 0.4rem;
  }
  .stat { padding: 0.55rem 0.3rem; border-radius: 8px; }
  .stat-label { font-size: 0.68rem; }
  .stat-value { font-size: 1rem; }
}

.stat {
  background: #f8fafb;
  border: 1px solid #e5e9ef;
  border-radius: 10px;
  padding: 0.7rem 0.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
}

.stat-label {
  font-size: 0.74rem;
  color: $text-muted;
}

.stat-value {
  font-size: 1.15rem;
  font-weight: 600;
  color: $text-primary;

  &.is-success { color: #16a34a; }
  &.is-pending { color: $color-accent; }
}

.progress-block {
  display: grid;
  gap: 0.35rem;
}

.progress-head {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  color: $text-muted;

  strong { color: $color-primary; }
}

.tips {
  margin: 0;
  font-size: 0.74rem;
  color: $text-muted;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
