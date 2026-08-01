<script setup>
import { computed, reactive, ref, watch } from 'vue'
import { Button, Form, Input, Modal } from 'ant-design-vue'

const props = defineProps({
  open: { type: Boolean, default: false },
})

const emit = defineEmits(['update:open', 'submit'])

const formRef = ref()
const formState = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const innerVisible = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value),
})

const resetForm = () => {
  formState.oldPassword = ''
  formState.newPassword = ''
  formState.confirmPassword = ''
  formRef.value?.resetFields()
}

watch(
  () => props.open,
  (visible) => {
    if (!visible) {
      resetForm()
    }
  },
)

const handleSubmit = async () => {
  try {
    await formRef.value.validateFields()
    if (formState.newPassword !== formState.confirmPassword) {
      formRef.value.setFields([{ name: 'confirmPassword', errors: ['两次新密码输入不一致'] }])
      return
    }
    emit('submit', { ...formState })
    emit('update:open', false)
    resetForm()
  } catch (error) {
    console.error(error)
  }
}
</script>

<template>
  <Modal
    v-model:open="innerVisible"
    title="修改密码"
    centered
    destroy-on-close
    :width="480"
    ok-text="提交"
    cancel-text="取消"
    @ok="handleSubmit"
  >
    <Form ref="formRef" :model="formState" layout="vertical" class="agent-modal-form">
      <Form.Item
        label="原始密码"
        name="oldPassword"
        :rules="[{ required: true, message: '请输入原始密码' }]"
      >
        <Input v-model:value="formState.oldPassword" type="password" placeholder="请输入原始密码" />
      </Form.Item>
      <Form.Item
        label="新密码"
        name="newPassword"
        :rules="[{ required: true, message: '请输入新密码' }]"
      >
        <Input v-model:value="formState.newPassword" type="password" placeholder="请输入新密码" />
      </Form.Item>
      <Form.Item
        label="确认新密码"
        name="confirmPassword"
        :rules="[{ required: true, message: '请再次输入新密码' }]"
      >
        <Input
          v-model:value="formState.confirmPassword"
          type="password"
          placeholder="请再次输入新密码"
        />
      </Form.Item>
    </Form>

    <template #footer>
      <div class="modal-actions">
        <Button @click="emit('update:open', false)">取消</Button>
        <Button type="primary" @click="handleSubmit">提交</Button>
      </div>
    </template>
  </Modal>
</template>

<style scoped lang="scss">
.agent-modal-form {
  .ant-form-item-label > label {
    color: #1f2937;
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}
</style>
