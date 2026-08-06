<script setup>
import { reactive, ref, onMounted } from 'vue'
import { Button, Card, Form, Input, Tabs, message } from 'ant-design-vue'
import { useRouter, useRoute } from 'vue-router'
import { authApi } from '../../api'
import { useAuth } from '../../stores/auth'

const router = useRouter()
const route = useRoute()
const auth = useAuth()

// 从 401 / 403 跳转过来时提示对应原因
onMounted(() => {
  if (route.query.reason === 'expired') {
    message.warning('登录已失效，请重新登录')
    // 清理 URL 参数，避免刷新重复提示
    router.replace({ query: {} })
  } else if (route.query.reason === 'forbidden') {
    message.warning('权限不足，请重新登录')
    router.replace({ query: {} })
  }
})

const activeTab = ref('login')
const loginFormRef = ref()
const registerFormRef = ref()
const captchaText = ref(generateCaptcha())
const submitting = ref(false)

const loginForm = reactive({
  account: '',
  password: '',
  captcha: '',
})

const registerForm = reactive({
  email: '',
  username: '',
  password: '',
  confirmPassword: '',
  captcha: '',
  agentDomain: '',
})

function generateCaptcha() {
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'
  let result = ''
  for (let i = 0; i < 4; i += 1) {
    result += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return result
}

const refreshCaptcha = () => {
  captchaText.value = generateCaptcha()
  loginForm.captcha = ''
  registerForm.captcha = ''
}

const emailRule = [
  { required: true, message: '请输入邮箱' },
  {
    pattern: /^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/,
    message: '请输入正确的邮箱格式',
  },
]

const usernameRule = [
  { required: true, message: '请输入账号' },
  {
    pattern: /^[A-Za-z0-9]{5,15}$/,
    message: '账号仅支持 5-15 位字母或数字',
  },
]

const handleLogin = async () => {
  try {
    await loginFormRef.value.validateFields()
    if (loginForm.captcha.toUpperCase() !== captchaText.value.toUpperCase()) {
      message.error('验证码不正确')
      refreshCaptcha()
      return
    }
    submitting.value = true
    await auth.login({ login: loginForm.account, password: loginForm.password })
    message.success('登录成功')
    await router.push('/app/homeIndex')
  } catch (error) {
    if (error?.message) message.error(error.message)
  } finally {
    submitting.value = false
  }
}

const handleRegister = async () => {
  try {
    await registerFormRef.value.validateFields()
    if (registerForm.password !== registerForm.confirmPassword) {
      message.error('两次密码输入不一致')
      return
    }
    if (registerForm.captcha.toUpperCase() !== captchaText.value.toUpperCase()) {
      message.error('验证码不正确')
      refreshCaptcha()
      return
    }
    submitting.value = true
    await authApi.register({
      email: registerForm.email,
      username: registerForm.username,
      password: registerForm.password,
      agentDomain: registerForm.agentDomain || null,
    })
    message.success('注册成功，请登录')
    activeTab.value = 'login'
    Object.assign(registerForm, {
      email: '',
      username: '',
      password: '',
      confirmPassword: '',
      captcha: '',
      agentDomain: '',
    })
    refreshCaptcha()
  } catch (error) {
    if (error?.message) message.error(error.message)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <h1 class="tips1">提升你的 <span class="tip">社交媒体</span> 影响力</h1>
    <Card class="login-card" :bordered="false">
      <div class="brand-area">
        <div class="brand-mark">FS</div>
        <div>
          <h2>欢迎回来</h2>
          <p>管理您的代理与运营数据</p>
        </div>
      </div>

      <Tabs v-model:activeKey="activeTab" centered>
        <Tabs.TabPane key="login" tab="登录">
          <Form ref="loginFormRef" :model="loginForm" layout="vertical" class="auth-form">
            <Form.Item
              label="邮箱或账号"
              name="account"
              :rules="[{ required: true, message: '请输入邮箱或账号' }]"
            >
              <Input v-model:value="loginForm.account" placeholder="请输入邮箱或账号" />
            </Form.Item>
            <Form.Item
              label="密码"
              name="password"
              :rules="[{ required: true, message: '请输入密码' }]"
            >
              <Input v-model:value="loginForm.password" type="password" placeholder="请输入密码" />
            </Form.Item>
            <Form.Item
              label="图形验证码"
              name="captcha"
              :rules="[{ required: true, message: '请输入验证码' }]"
            >
              <div class="captcha-row">
                <Input v-model:value="loginForm.captcha" placeholder="请输入验证码" />
                <div class="captcha-box" @click="refreshCaptcha">{{ captchaText }}</div>
              </div>
            </Form.Item>
            <Button type="primary" block class="submit-btn" :loading="submitting" @click="handleLogin">登录</Button>
          </Form>
        </Tabs.TabPane>

        <Tabs.TabPane key="register" tab="注册">
          <Form ref="registerFormRef" :model="registerForm" layout="vertical" class="auth-form">
            <Form.Item label="邮箱" name="email" :rules="emailRule">
              <Input v-model:value="registerForm.email" placeholder="请输入邮箱" />
            </Form.Item>
            <Form.Item label="账号" name="username" :rules="usernameRule">
              <Input v-model:value="registerForm.username" placeholder="请输入 5-15 位字母或数字" />
            </Form.Item>
            <Form.Item
              label="密码"
              name="password"
              :rules="[{ required: true, message: '请输入密码' }]"
            >
              <Input
                v-model:value="registerForm.password"
                type="password"
                placeholder="请输入密码"
              />
            </Form.Item>
            <Form.Item label="代理域名（选填）" name="agentDomain">
              <Input v-model:value="registerForm.agentDomain" placeholder="有代理时填写代理域名" />
            </Form.Item>
            <Form.Item
              label="确认密码"
              name="confirmPassword"
              :rules="[{ required: true, message: '请再次输入密码' }]"
            >
              <Input
                v-model:value="registerForm.confirmPassword"
                type="password"
                placeholder="请再次输入密码"
              />
            </Form.Item>
            <Form.Item
              label="图形验证码"
              name="captcha"
              :rules="[{ required: true, message: '请输入验证码' }]"
            >
              <div class="captcha-row">
                <Input v-model:value="registerForm.captcha" placeholder="请输入验证码" />
                <div class="captcha-box" @click="refreshCaptcha">{{ captchaText }}</div>
              </div>
            </Form.Item>
            <Button type="primary" block class="submit-btn" :loading="submitting" @click="handleRegister">注册</Button>
          </Form>
        </Tabs.TabPane>
      </Tabs>
    </Card>
  </div>
</template>

<style scoped lang="scss">
.login-page {
  min-height: 100vh;
  display: grid;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
  background-color: #0a0a0a;
  background-image: radial-gradient(circle at top, rgba(255, 255, 255, 0.06), transparent 60%);
  align-content: center;
  .tips1 {
    margin-bottom: 2rem;
    font-size: 2.4rem;
    font-weight: 700;
    color: #f9fafb;

    .tip {
      color: #f59e0b;
    }
  }
}

@media (max-width: 480px) {
  .login-page {
    align-content: start;
    padding: 2rem 1rem;

    .tips1 {
      font-size: 2rem;
      line-height: 1.35;
      margin-bottom: 1.25rem;
    }
  }

  .login-card {
    padding: 0;
  }
}

.login-card {
  width: min(460px, 100%);
  background: rgba(17, 24, 39, 0.96);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.35);
  padding: 0.5rem;
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.25rem;

  h2 {
    margin: 0 0 0.25rem;
    color: #f9fafb;
  }

  p {
    margin: 0;
    color: #9ca3af;
  }
}

.brand-mark {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #111827;
  background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);
}

.auth-form {
  margin-top: 0.5rem;

  :deep(.ant-form-item-label > label) {
    color: #f9fafb;
  }
}

.captcha-row {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.captcha-box {
  min-width: 96px;
  height: 40px;
  padding: 0 0.8rem;
  border-radius: 10px;
  border: 1px dashed #f59e0b;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #f59e0b;
  font-weight: 700;
  letter-spacing: 0.2rem;
  cursor: pointer;
  user-select: none;
}

.submit-btn {
  margin-top: 0.5rem;
  height: 42px;
  border-radius: 10px;
  background: linear-gradient(120deg, #f59e0b 0%, #d97706 100%);
  border: none;
}
:deep(.ant-tabs-nav-wrap) {
  color: white;
}
</style>
