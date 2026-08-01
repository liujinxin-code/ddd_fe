<script setup>
import { computed, reactive, watch } from 'vue'
import { Button, Form, InputNumber, Modal } from 'ant-design-vue'

const props = defineProps({
  open: { type: Boolean, default: false },
  title: { type: String, default: '执行情况' },
  initialValues: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:open'])

const formState = reactive({
  initialQuantity: 0,
  completedQuantity: 0,
})

const innerVisible = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value),
})

const syncForm = () => {
  formState.initialQuantity = props.initialValues.quantity || 0
  formState.completedQuantity = props.initialValues.successCount || 0
}

watch(
  () => props.open,
  (visible) => {
    if (visible) {
      syncForm()
    }
  },
)

watch(
  () => props.initialValues,
  () => {
    if (props.open) {
      syncForm()
    }
  },
  { deep: true },
)

const handleClose = () => {
  emit('update:open', false)
}
</script>

<template>
  <Modal
    v-model:open="innerVisible"
    :title="title"
    centered
    destroy-on-close
    :width="520"
    ok-text="关闭"
    cancel-text="取消"
    :footer="null"
  >
    <Form layout="vertical" class="order-modal-form">
      <Form.Item label="初始数量" name="initialQuantity">
        <InputNumber
          v-model:value="formState.initialQuantity"
          :min="0"
          disabled
          style="width: 100%"
        />
      </Form.Item>
      <Form.Item label="完成数量" name="completedQuantity">
        <InputNumber
          v-model:value="formState.completedQuantity"
          :min="0"
          disabled
          style="width: 100%"
        />
      </Form.Item>
    </Form>

    <div class="modal-actions">
      <Button type="primary" @click="handleClose">关闭</Button>
    </div>
  </Modal>
</template>

<style scoped lang="scss">
.order-modal-form {
  .ant-form-item-label > label {
    color: #1f2937;
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 0.5rem;
}
</style>
